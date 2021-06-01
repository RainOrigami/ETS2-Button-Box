using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ETS2_Button_Box_Host
{
    public class ButtonController
    {
        public delegate void OnButtonStateChanged(Dictionary<Button, bool> newButtonStates, Dictionary<Button, bool> previousButtonStates);
        public event OnButtonStateChanged ButtonStateChanged;

        /// <summary>
        /// List of all current button states
        /// </summary>
        private Dictionary<Button, bool> buttonStates;

        public bool GetButtonState(Button button) => this.buttonStates[button];

        /// <summary>
        /// Define a key button action
        /// </summary>
        public struct ButtonAction
        {
            /// <summary>
            /// Which button is being pressed or released
            /// </summary>
            public Button Button;

            /// <summary>
            /// Which press type the button experiences
            /// </summary>
            public ButtonPressType ButtonPressType;

            /// <summary>
            /// Validator whether the action is or is not valid in the current context
            /// </summary>
            public Func<Dictionary<Button, bool>, Dictionary<Button, bool>, ButtonAction, bool> IsActionValid;

            /// <summary>
            /// Action to be executed when IsActionValid is true and button action is performed
            /// </summary>
            public Action<Dictionary<Button, bool>, Dictionary<Button, bool>, ButtonAction> Action;
        }

        public void ParseButtonString(string buttons)
        {
            // Validate length of button line
            if (buttons.Length != Enum.GetValues(typeof(Button)).Length)
            {
                Console.WriteLine($"Received invalid button state (is {buttons.Length} should {Enum.GetValues(typeof(Button)).Length}): {buttons}");
                return;
            }

            // Build previous button states map for event
            Dictionary<Button, bool> previousButtonStates = new Dictionary<Button, bool>();
            foreach (Button button in this.buttonStates.Keys)
                previousButtonStates.Add(button, this.buttonStates[button]);

            // Read button states into button states map
            for (int i = 0; i < buttons.Length; i++)
                if (this.buttonStates.ContainsKey((Button)i))
                    this.buttonStates[(Button)i] = buttons[i] == '1';

            // Invoke event
            this.ButtonStateChanged?.Invoke(this.buttonStates, previousButtonStates);
        }

        internal void AddButtonAction(ButtonAction buttonAction)
        {
            this.buttonActions.Add(buttonAction);
        }

        /// <summary>
        /// Button press types
        /// </summary>
        public enum ButtonPressType
        {
            /// <summary>
            /// Button was not previously pressed but is now pressed
            /// </summary>
            Pressed,

            /// <summary>
            /// Button was previously pressed but is not not pressed
            /// </summary>
            Released,

            /// <summary>
            /// Button was previously pressed and is still pressed
            /// </summary>
            Held
        }

        private List<ButtonAction> buttonActions;

        public void LightsSelection(Dictionary<Button, bool> newButtonStates, Dictionary<Button, bool> previousButtonStates, ButtonAction buttonAction)
        {

        }

        public void WiperSelection(Dictionary<Button, bool> newButtonStates, Dictionary<Button, bool> previousButtonStates, ButtonAction buttonAction)
        {
            this.distanceToWiperTarget = -1;
        }

        private TelemetryController telemetryController;

        /// <summary>
        /// Initialise button controller
        /// </summary>
        public ButtonController(TelemetryController telemetryController)
        {
            this.telemetryController = telemetryController;

            this.telemetryController.TelemetryChanged += TelemetryController_TelemetryChanged;

            // Initialise the button states map
            this.buttonStates = new Dictionary<Button, bool>();

            // Fill button states map with all available buttons and turn them OFF
            foreach (Button button in Enum.GetValues(typeof(Button)))
                this.buttonStates.Add(button, false);

            // Hook on button state change event
            this.ButtonStateChanged += buttonStateChangedHandler;

            this.buttonActions = new List<ButtonAction>();
        }

        int distanceToWiperTarget = -1;

        private void TelemetryController_TelemetryChanged(SCSSdkClient.Object.SCSTelemetry telemetry)
        {
            // Try to ensure the correct lights position
            int ensureLightPosition = 0;
            if (this.GetButtonState(Button.LI1))
                ensureLightPosition = 1;
            else if (this.GetButtonState(Button.LI2))
                ensureLightPosition = 2;
            else if (this.GetButtonState(Button.LI3))
                ensureLightPosition = 3;
            else if (this.GetButtonState(Button.LI4))
                ensureLightPosition = 3;

            int currentLightPosition = 0;
            if (!this.telemetryController.LastTelemetryData.TruckValues.CurrentValues.LightsValues.Parking && !this.telemetryController.LastTelemetryData.TruckValues.CurrentValues.LightsValues.BeamLow)
                currentLightPosition = 1;
            else if (this.telemetryController.LastTelemetryData.TruckValues.CurrentValues.LightsValues.Parking && !this.telemetryController.LastTelemetryData.TruckValues.CurrentValues.LightsValues.BeamLow)
                currentLightPosition = 2;
            else if (this.telemetryController.LastTelemetryData.TruckValues.CurrentValues.LightsValues.Parking && this.telemetryController.LastTelemetryData.TruckValues.CurrentValues.LightsValues.BeamLow)
                currentLightPosition = 3;

            if (ensureLightPosition > 0 && currentLightPosition > 0)
                if (currentLightPosition != ensureLightPosition)
                    KeyboardController.InvokeKeyPress(KeyboardController.ButtonToKeyStroke[Button.LI1]);

            // Try to ensure the correct wipers position
            if (this.distanceToWiperTarget == -1)
            {
                if (this.telemetryController.LastTelemetryData.TruckValues.CurrentValues.DashboardValues.Wipers)
                {
                    KeyboardController.InvokeKeyPress(KeyboardController.ButtonToKeyStroke[Button.WI1]);
                }
                else
                {
                    int ensureWiperPosition = -1;
                    if (this.GetButtonState(Button.WI1))
                        ensureWiperPosition = 0;
                    else if (this.GetButtonState(Button.WI2))
                        ensureWiperPosition = 1;
                    else if (this.GetButtonState(Button.WI3))
                        ensureWiperPosition = 2;
                    else if (this.GetButtonState(Button.WI4))
                        ensureWiperPosition = 3;
                    if (ensureWiperPosition > -1)
                        this.distanceToWiperTarget = ensureWiperPosition;
                }
            }
            else if (this.distanceToWiperTarget > 0)
            {
                KeyboardController.InvokeKeyPress(KeyboardController.ButtonToKeyStroke[Button.WI1]);
                this.distanceToWiperTarget--;
            }
        }

        private void buttonStateChangedHandler(Dictionary<Button, bool> newButtonStates, Dictionary<Button, bool> previousButtonStates)
        {
            if (!this.telemetryController.IsConnected)
                return;

            // Check each button action
            foreach (ButtonAction buttonAction in this.buttonActions)
            {
                // Check button action validity
                if (buttonAction.IsActionValid != null && !buttonAction.IsActionValid(newButtonStates, previousButtonStates, buttonAction))
                    continue;

                bool isPressType = false;

                switch (buttonAction.ButtonPressType)
                {
                    case ButtonPressType.Pressed:
                        isPressType = checkButtonPressed(newButtonStates, previousButtonStates, buttonAction.Button);
                        break;
                    case ButtonPressType.Released:
                        isPressType = checkButtonReleased(newButtonStates, previousButtonStates, buttonAction.Button);
                        break;
                    case ButtonPressType.Held:
                        isPressType = newButtonStates[buttonAction.Button] && previousButtonStates[buttonAction.Button];
                        break;
                }

                if (isPressType)
                    buttonAction.Action?.Invoke(newButtonStates, previousButtonStates, buttonAction);
            }
        }

        private static bool checkButtonPressed(Dictionary<Button, bool> newButtonStates, Dictionary<Button, bool> previousButtonStates, Button button) => newButtonStates[button] && !previousButtonStates[button];
        private static bool checkButtonReleased(Dictionary<Button, bool> newButtonStates, Dictionary<Button, bool> previousButtonStates, Button button) => !newButtonStates[button] && previousButtonStates[button];
        private static bool checkButtonHeld(Dictionary<Button, bool> newButtonStates, Dictionary<Button, bool> previousButtonStates, Button button) => newButtonStates[button] && previousButtonStates[button];

        public static EncoderDirection GetEncoderDirection(Dictionary<Button, bool> newButtonStates, Dictionary<Button, bool> previousButtonStates, Button button)
        {
            // Detect rising edge
            if (!previousButtonStates[Button.CCS1] && newButtonStates[Button.CCS1])
            {
                if (newButtonStates[Button.CCS2])
                    return EncoderDirection.Right;
                else
                    return EncoderDirection.Left;
            }
            else
                return EncoderDirection.None;
        }

        public enum EncoderDirection
        {
            None,
            Left,
            Right
        }
    }
}
