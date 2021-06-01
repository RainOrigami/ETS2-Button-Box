using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ETS2_Button_Box_Host.ButtonController;

namespace ETS2_Button_Box_Host
{
    /// <summary>
    /// Handle the interaction of telemetry, buttons, LEDs and the serial connection
    /// </summary>
    public class ButtonBoxController
    {
        /// <summary>
        /// Interval in milliseconds of how often an LED update will be sent independent of button changes or telemetry data
        /// </summary>
        private const int LedUpdateTimerInterval = 1000;

        /// <summary>
        /// The controller handling all button interaction
        /// </summary>
        private ButtonController buttonController;

        /// <summary>
        /// The controller handling all LED interaction
        /// </summary>
        private LEDController ledController;

        /// <summary>
        /// The controller handling all serial interaction
        /// </summary>
        private SerialController serialController;

        /// <summary>
        /// The controller handling all telemetry interaction
        /// </summary>
        private TelemetryController telemetryController;

        /// <summary>
        /// The timer that periodically sends LED updates
        /// </summary>
        private Timer ledUpdateTimer;

        public ButtonBoxController()
        {
            // Initialise controllers
            this.buttonController = new ButtonController();
            this.ledController = new LEDController();
            this.serialController = new SerialController();
            this.telemetryController = new TelemetryController();

            // Initialise button state change events
            this.serialController.ButtonStateChangeReceived += this.buttonController.ParseButtonString;
            this.buttonController.ButtonStateChanged += this.ledController.HandleChangedButtonState;
            this.buttonController.ButtonStateChanged += ButtonController_ButtonStateChanged;

            // Initialise telemetry change events
            this.telemetryController.TelemetryChanged += this.ledController.HandleTelemetryData;
            this.telemetryController.TelemetryChanged += TelemetryController_TelemetryChanged;

            // Initialise LED update timer
            this.ledUpdateTimer = new Timer(this.doLedInterval, null, 0, LedUpdateTimerInterval);
        }

        /// <summary>
        /// Handles telemetry change post LED handling to send the current LED state directly after LEDs were updated
        /// </summary>
        /// <param name="telemetry">Latest telemetry data set</param>
        private void TelemetryController_TelemetryChanged(SCSSdkClient.Object.SCSTelemetry telemetry) => this.sendLedUpdate();

        /// <summary>
        /// Handles button state change post button handling to send the current LED state directly after buttons were changed
        /// </summary>
        /// <param name="newButtonStates">New button state dictionary</param>
        /// <param name="previousButtonStates">Previous button state dictionary</param>
        private void ButtonController_ButtonStateChanged(Dictionary<Button, bool> newButtonStates, Dictionary<Button, bool> previousButtonStates) => this.sendLedUpdate();

        #region Button Layout
        private void initialiseButtonLayout()
        {
            // Initialise button actions
            // This will connect all box buttons to their respective actions, like invoking key presses or handling selector switch selection

            // Power on only if elecricity is off
            this.buttonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.PWR,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = () => !this.telemetryController.LastTelemetryData.TruckValues.CurrentValues.ElectricEnabled,
                    Action = KeyboardController.InvokeKeyPress
                });

            // Power off only if electricity is on
            this.buttonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.PWR,
                    ButtonPressType = ButtonPressType.Released,
                    IsActionValid = () => this.telemetryController.LastTelemetryData.TruckValues.CurrentValues.ElectricEnabled,
                    Action = KeyboardController.InvokeKeyPress
                });

            // Engine on only if engine is off
            this.buttonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.ENG,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = () => !this.telemetryController.LastTelemetryData.TruckValues.CurrentValues.EngineEnabled,
                    Action = KeyboardController.InvokeKeyPress
                });

            // Engine off only if engine is off
            // TODO: validate that engine can be turned off seperately from elecricity
            this.buttonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.ENG,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = () => this.telemetryController.LastTelemetryData.TruckValues.CurrentValues.EngineEnabled,
                    Action = KeyboardController.InvokeKeyPress
                });

            // Cruise control on only if cruise control is off
            this.buttonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.CCO,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = () => !this.telemetryController.LastTelemetryData.TruckValues.CurrentValues.DashboardValues.CruiseControl,
                    Action = KeyboardController.InvokeKeyPress
                });

            // Cruise control off only if cruise control is on
            this.buttonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.CCO,
                    ButtonPressType = ButtonPressType.Released,
                    IsActionValid = () => this.telemetryController.LastTelemetryData.TruckValues.CurrentValues.DashboardValues.CruiseControl,
                    Action = KeyboardController.InvokeKeyPress
                });

            // Cruise control resume only if cruise control is off
            this.buttonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.CCR,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = () => !this.telemetryController.LastTelemetryData.TruckValues.CurrentValues.DashboardValues.CruiseControl,
                    Action = KeyboardController.InvokeKeyPress
                });

            // Cruise control off only if cruise control is on
            this.buttonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.CCR,
                    ButtonPressType = ButtonPressType.Released,
                    IsActionValid = () => this.telemetryController.LastTelemetryData.TruckValues.CurrentValues.DashboardValues.CruiseControl,
                    Action = a => KeyboardController.InvokeKeyPress(KeyboardController.ButtonToKeyStroke[Button.CCO])
                });

            // Cruise control decrease (turn clockwise) can be used anytime
            this.buttonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.CCS1,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = () => true,
                    Action = KeyboardController.InvokeKeyPress
                });

            // Cruise control increase (turn counterclockwise) can be used anytime
            this.buttonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.CCS2,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = () => true,
                    Action = KeyboardController.InvokeKeyPress
                });

            //// Low gear range selection can only be done in high gear range
            //// TODO: figure out where we could get the current gear range status from
            //this.buttonController.AddButtonAction(
            //new ButtonAction()
            //{
            //    Button = Button.GR,
            //    ButtonPressType = ButtonPressType.Released,
            //    IsActionValid = () => ???,
            //    Action = KeyboardController.InvokeKeyPress
            //});

            //// High gear range selection can only be done in low gear range
            //// TODO: figure out where we could get the current gear range status from
            //this.buttonController.AddButtonAction(
            //new ButtonAction()
            //{
            //    Button = Button.GR,
            //    ButtonPressType = ButtonPressType.Pressed,
            //    IsActionValid = () => ???,
            //    Action = KeyboardController.InvokeKeyPress
            //});

            //// Differential can only be activated when it's inactive
            //// TODO: figure out where we could get the differential status from
            //this.buttonController.AddButtonAction(
            //new ButtonAction()
            //{
            //    Button = Button.DIF,
            //    ButtonPressType = ButtonPressType.Pressed,
            //    IsActionValid = () => ???,
            //    Action = KeyboardController.InvokeKeyPress
            //});

            //// Differential can only be deactivated when it's active
            //// TODO: figure out where we could get the differential status from
            //this.buttonController.AddButtonAction(
            //new ButtonAction()
            //{
            //    Button = Button.DIF,
            //    ButtonPressType = ButtonPressType.Pressed,
            //    IsActionValid = () => ???,
            //    Action = KeyboardController.InvokeKeyPress
            //});

            // View selector requires special attention
            this.buttonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.VW1,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = () => true,
                    Action = this.buttonController.ViewSelection
                });
            this.buttonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.VW2,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = () => true,
                    Action = this.buttonController.ViewSelection
                });
            this.buttonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.VW3,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = () => true,
                    Action = this.buttonController.ViewSelection
                });
            this.buttonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.VW4,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = () => true,
                    Action = this.buttonController.ViewSelection
                });

            // Hazards toggle any time
            this.buttonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.INDH,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = () => true,
                    Action = KeyboardController.InvokeKeyPress
                });

            // Beacon can only turn on when it's off
            this.buttonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.BC,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = () => !this.telemetryController.LastTelemetryData.TruckValues.CurrentValues.LightsValues.Beacon,
                    Action = KeyboardController.InvokeKeyPress
                });

            // Beacon can only turn off when it's on
            this.buttonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.BC,
                    ButtonPressType = ButtonPressType.Released,
                    IsActionValid = () => this.telemetryController.LastTelemetryData.TruckValues.CurrentValues.LightsValues.Beacon,
                    Action = KeyboardController.InvokeKeyPress
                });

            // Wiper selector requires special attention
            this.buttonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.WI1,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = () => true,
                    Action = this.buttonController.WiperSelection
                });
            this.buttonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.WI2,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = () => true,
                    Action = this.buttonController.WiperSelection
                });
            this.buttonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.WI3,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = () => true,
                    Action = this.buttonController.WiperSelection
                });
            this.buttonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.WI4,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = () => true,
                    Action = this.buttonController.WiperSelection
                });

            // Lights selector requires special attention
            this.buttonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.LI1,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = () => true,
                    Action = this.buttonController.LightsSelection
                });
            this.buttonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.LI2,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = () => true,
                    Action = this.buttonController.LightsSelection
                });
            this.buttonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.LI3,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = () => true,
                    Action = this.buttonController.LightsSelection
                });
            this.buttonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.LI4,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = () => true,
                    Action = this.buttonController.LightsSelection
                });

            // Tow can be used any time
            this.buttonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.TOW,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = () => true,
                    Action = KeyboardController.InvokeKeyPress
                });

            // Horns can be used any time
            this.buttonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.HRN,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = () => true,
                    Action = KeyboardController.InvokeKeyDown
                });
            this.buttonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.HRN,
                    ButtonPressType = ButtonPressType.Released,
                    IsActionValid = () => true,
                    Action = KeyboardController.InvokeKeyUp
                });

            // Screamer can be used any time
            this.buttonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.SCR,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = () => true,
                    Action = KeyboardController.InvokeKeyDown
                });
            this.buttonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.SCR,
                    ButtonPressType = ButtonPressType.Released,
                    IsActionValid = () => true,
                    Action = KeyboardController.InvokeKeyUp
                });

            // Flash can be used any time
            this.buttonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.FLS,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = () => true,
                    Action = KeyboardController.InvokeKeyDown
                });
            this.buttonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.FLS,
                    ButtonPressType = ButtonPressType.Released,
                    IsActionValid = () => true,
                    Action = KeyboardController.InvokeKeyUp
                });

            //// Trailer brake can only be enabled when it's disabled
            //// TODO: figure out where to get the trailer brake status from
            //this.buttonController.AddButtonAction(
            //new ButtonAction()
            //{
            //    Button = Button.TB,
            //    ButtonPressType = ButtonPressType.Pressed,
            //    IsActionValid = () => ???,
            //    Action = KeyboardController.InvokeKeyPress
            //});

            //// Trailer brake can only be disabled when it's enabled
            //// TODO: figure out where to get the trailer brake status from
            //this.buttonController.AddButtonAction(
            //new ButtonAction()
            //{
            //    Button = Button.TB,
            //    ButtonPressType = ButtonPressType.Released,
            //    IsActionValid = () => ???,
            //    Action = KeyboardController.InvokeKeyPress
            //});

            // Engine brake can only be enabled when it's disabled
            this.buttonController.AddButtonAction(
            new ButtonAction()
            {
                Button = Button.ENB,
                ButtonPressType = ButtonPressType.Pressed,
                IsActionValid = () => !this.telemetryController.LastTelemetryData.TruckValues.CurrentValues.MotorValues.BrakeValues.MotorBrake,
                Action = KeyboardController.InvokeKeyPress
            });

            // Engine brake can only be disabled when it's enabled
            this.buttonController.AddButtonAction(
            new ButtonAction()
            {
                Button = Button.ENB,
                ButtonPressType = ButtonPressType.Released,
                IsActionValid = () => this.telemetryController.LastTelemetryData.TruckValues.CurrentValues.MotorValues.BrakeValues.MotorBrake,
                Action = KeyboardController.InvokeKeyPress
            });

            // Retarder strengh can be decreased any time
            this.buttonController.AddButtonAction(
            new ButtonAction()
            {
                Button = Button.RET1,
                ButtonPressType = ButtonPressType.Pressed,
                IsActionValid = () => true,
                Action = KeyboardController.InvokeKeyPress
            });

            // Retarder strengh can be increased any time
            this.buttonController.AddButtonAction(
            new ButtonAction()
            {
                Button = Button.RET2,
                ButtonPressType = ButtonPressType.Pressed,
                IsActionValid = () => true,
                Action = KeyboardController.InvokeKeyPress
            });

            // Engine brake strengh can be decreased any time
            this.buttonController.AddButtonAction(
            new ButtonAction()
            {
                Button = Button.ENB1,
                ButtonPressType = ButtonPressType.Pressed,
                IsActionValid = () => true,
                Action = KeyboardController.InvokeKeyPress
            });

            // Engine brake strengh can be increased any time
            this.buttonController.AddButtonAction(
            new ButtonAction()
            {
                Button = Button.ENB2,
                ButtonPressType = ButtonPressType.Pressed,
                IsActionValid = () => true,
                Action = KeyboardController.InvokeKeyPress
            });

            // E-brake can only be engaged when it's disengaged
            this.buttonController.AddButtonAction(
            new ButtonAction()
            {
                Button = Button.EB,
                ButtonPressType = ButtonPressType.Pressed,
                IsActionValid = () => !this.telemetryController.LastTelemetryData.TruckValues.CurrentValues.MotorValues.BrakeValues.ParkingBrake,
                Action = KeyboardController.InvokeKeyPress
            });

            // E-brake can only be disengaged when it's engaged
            this.buttonController.AddButtonAction(
            new ButtonAction()
            {
                Button = Button.EB,
                ButtonPressType = ButtonPressType.Released,
                IsActionValid = () => this.telemetryController.LastTelemetryData.TruckValues.CurrentValues.MotorValues.BrakeValues.ParkingBrake,
                Action = KeyboardController.InvokeKeyPress
            });
        }
        #endregion

        /// <summary>
        /// Do the LED interval logic
        /// </summary>
        /// <param name="state">Unused</param>
        private void doLedInterval(object state = null)
        {
            // Handle LED interval on the controller
            this.ledController.HandleLedInterval();

            // Send LED state string to the box
            this.sendLedUpdate();
        }

        /// <summary>
        /// Send an LED state string to the box
        /// </summary>
        /// <param name="state">Unused</param>
        private void sendLedUpdate()
        {
            // Show readiness state while telemetry is not connected
            if (!this.telemetryController.IsTelemetryConnected)
            {
                this.ledController.ResetLedsExcept(LED.IND_H);
                this.ledController.EnableLed(LED.PWR);
                this.ledController.ToggleLed(LED.IND_H);
                return;
            }

            // Otherwise send LED state string
            this.serialController.SendLedState(this.ledController.GetLedStateString());
        }

        /// <summary>
        /// Try and connect to the button box through the specified COM port.
        /// This will connect and perform a handshake to verify proper connection.
        /// </summary>
        /// <param name="comPortName">Name of the COM port to connect to</param>
        /// <returns>True on successful connection, otherwise false</returns>
        public bool ConnectToBox(string comPortName)
        {
            serialController.Connect(comPortName);
            if (serialController.Handshake())
                return true;
            serialController.Disconnect();
            return false;
        }
    }
}
