using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            public Func<bool> IsActionValid;

            /// <summary>
            /// Action to be executed when IsActionValid is true and button action is performed
            /// </summary>
            public Action<ButtonAction> Action;
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


        public void LightsSelection(ButtonAction buttonAction)
        {

        }

        public void ViewSelection(ButtonAction buttonAction)
        {

        }

        public void WiperSelection(ButtonAction buttonAction)
        {

        }

        /// <summary>
        /// Initialise button controller
        /// </summary>
        public ButtonController()
        {
            // Initialise the button states map
            this.buttonStates = new Dictionary<Button, bool>();

            // Fill button states map with all available buttons and turn them OFF
            foreach (Button button in Enum.GetValues(typeof(Button)))
                this.buttonStates.Add(button, false);

            // Hook on button state change event
            this.ButtonStateChanged += buttonStateChangedHandler;

            this.buttonActions = new List<ButtonAction>();

        }

        private void buttonStateChangedHandler(Dictionary<Button, bool> newButtonStates, Dictionary<Button, bool> previousButtonStates)
        {
            // Check each button action
            foreach (ButtonAction buttonAction in this.buttonActions)
            {
                // Check button action validity
                if (buttonAction.IsActionValid != null && !buttonAction.IsActionValid())
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
                    buttonAction.Action?.Invoke(buttonAction);
            }
        }

        private static bool checkButtonPressed(Dictionary<Button, bool> newButtonStates, Dictionary<Button, bool> previousButtonStates, Button button) => newButtonStates[button] && !previousButtonStates[button];
        private static bool checkButtonReleased(Dictionary<Button, bool> newButtonStates, Dictionary<Button, bool> previousButtonStates, Button button) => !newButtonStates[button] && previousButtonStates[button];
        private static bool checkButtonHeld(Dictionary<Button, bool> newButtonStates, Dictionary<Button, bool> previousButtonStates, Button button) => newButtonStates[button] && previousButtonStates[button];
    }
}
