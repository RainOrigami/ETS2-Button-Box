using ETS2_Button_Box_Library;
using static ETS2_Button_Box_Host.ButtonController;
using Timer = System.Threading.Timer;

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
        public ButtonController ButtonController { get; private set; }

        /// <summary>
        /// The controller handling all LED interaction
        /// </summary>
        public LEDController LedController { get; private set; }

        /// <summary>
        /// The controller handling all serial interaction
        /// </summary>
        public SerialController SerialController { get; private set; }

        /// <summary>
        /// The controller handling all telemetry interaction
        /// </summary>
        public TelemetryController TelemetryController { get; private set; }

        /// <summary>
        /// The timer that periodically sends LED updates
        /// </summary>
        private Timer ledUpdateTimer;

        public ButtonBoxController()
        {
            // Initialise controllers
            this.TelemetryController = new TelemetryController();
            this.ButtonController = new ButtonController(this.TelemetryController);
            this.LedController = new LEDController();
            this.SerialController = new SerialController();

            this.initialiseButtonLayout();

            // Initialise button state change events
            this.SerialController.ButtonChanged += this.ButtonController.ReceiveButtonChangedEvent;
            this.ButtonController.ButtonStateChanged += this.LedController.HandleChangedButtonState;
            this.ButtonController.ButtonStateChanged += ButtonController_ButtonStateChanged;

            // Initialise telemetry change events
            this.TelemetryController.TelemetryChanged += this.LedController.HandleTelemetryData;
            this.TelemetryController.TelemetryChanged += TelemetryController_TelemetryChanged;

            // Initialise LED update timer
            this.ledUpdateTimer = new Timer(this.doLedInterval, null, 0, LedUpdateTimerInterval);
        }

        /// <summary>
        /// Handles button state change post button handling to send the current LED state directly after buttons were changed
        /// </summary>
        /// <param name="newButtonStates">New button state dictionary</param>
        /// <param name="previousButtonStates">Previous button state dictionary</param>
        private void ButtonController_ButtonStateChanged(Dictionary<Button, bool> newButtonStates, Dictionary<Button, bool> previousButtonStates)
        {
            if (!this.TelemetryController.IsConnected) return;

            this.sendLedUpdate();
        }

        /// <summary>
        /// Handles telemetry change post LED handling to send the current LED state directly after LEDs were updated
        /// </summary>
        /// <param name="telemetry">Latest telemetry data set</param>
        private void TelemetryController_TelemetryChanged(SCSSdkClient.Object.SCSTelemetry telemetry)
        {
            if (!this.TelemetryController.IsConnected) return;

            this.sendLedUpdate();
        }

        #region Button Layout
        private void initialiseButtonLayout()
        {
            // Initialise button actions
            // This will connect all box buttons to their respective actions, like invoking key presses or handling selector switch selection

            // Power on only if elecricity is off
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.PWR,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = (n, p, a) => !this.TelemetryController.LastTelemetryData.TruckValues.CurrentValues.ElectricEnabled,
                    Action = KeyboardController.InvokeKeyPress
                });

            // Power off only if electricity is on
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.PWR,
                    ButtonPressType = ButtonPressType.Released,
                    IsActionValid = (n, p, a) => this.TelemetryController.LastTelemetryData.TruckValues.CurrentValues.ElectricEnabled,
                    Action = KeyboardController.InvokeKeyPress
                });

            // Engine on only if engine is off
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.ENG,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = (n, p, a) => !this.TelemetryController.LastTelemetryData.TruckValues.CurrentValues.EngineEnabled,
                    Action = KeyboardController.InvokeKeyPress
                });

            // Engine off only if engine is off
            // TODO: validate that engine can be turned off seperately from elecricity
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.ENG,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = (n, p, a) => this.TelemetryController.LastTelemetryData.TruckValues.CurrentValues.EngineEnabled,
                    Action = KeyboardController.InvokeKeyPress
                });

            // Cruise control on only if cruise control is off
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.CCO,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = (n, p, a) => !this.TelemetryController.LastTelemetryData.TruckValues.CurrentValues.DashboardValues.CruiseControl,
                    Action = KeyboardController.InvokeKeyPress
                });

            // Cruise control off only if cruise control is on
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.CCO,
                    ButtonPressType = ButtonPressType.Released,
                    IsActionValid = (n, p, a) => this.TelemetryController.LastTelemetryData.TruckValues.CurrentValues.DashboardValues.CruiseControl,
                    Action = KeyboardController.InvokeKeyPress
                });

            // Cruise control resume only if cruise control is off
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.CCR,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = (n, p, a) => !this.TelemetryController.LastTelemetryData.TruckValues.CurrentValues.DashboardValues.CruiseControl,
                    Action = KeyboardController.InvokeKeyPress
                });

            // Cruise control off only if cruise control is on
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.CCR,
                    ButtonPressType = ButtonPressType.Released,
                    IsActionValid = (n, p, a) => this.TelemetryController.LastTelemetryData.TruckValues.CurrentValues.DashboardValues.CruiseControl,
                    Action = (n, p, a) => KeyboardController.InvokeKeyPress(KeyboardController.ButtonToKeyStroke[Button.CCO])
                });

            // Cruise control increase
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.CCS1,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = (n, p, a) => n[Button.CCS2],
                    Action = KeyboardController.InvokeKeyPress
                });

            // Cruise control decrease
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.CCS1,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = (n, p, a) => !n[Button.CCS2],
                    Action = (n, p, a) => KeyboardController.InvokeKeyPress(KeyboardController.ButtonToKeyStroke[Button.CCS2])
                });

            // Low gear range selection can only be done in high gear range
            // TODO: figure out where we could get the current gear range status from
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.GR,
                    ButtonPressType = ButtonPressType.Released,
                    IsActionValid = (n, p, a) => true,
                    Action = KeyboardController.InvokeKeyPress
                });

            // High gear range selection can only be done in low gear range
            // TODO: figure out where we could get the current gear range status from
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.GR,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = (n, p, a) => true,
                    Action = KeyboardController.InvokeKeyPress
                });

            // Differential can only be activated when it's inactive
            // TODO: figure out where we could get the differential status from
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.DIF,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = (n, p, a) => true,
                    Action = KeyboardController.InvokeKeyPress
                });

            // Differential can only be deactivated when it's active
            // TODO: figure out where we could get the differential status from
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.DIF,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = (n, p, a) => true,
                    Action = KeyboardController.InvokeKeyPress
                });

            // View selector requires special attention
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.VW1,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = (n, p, a) => true,
                    Action = KeyboardController.InvokeKeyPress
                });
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.VW2,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = (n, p, a) => true,
                    Action = KeyboardController.InvokeKeyPress
                });
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.VW3,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = (n, p, a) => true,
                    Action = KeyboardController.InvokeKeyPress
                });
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.VW4,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = (n, p, a) => true,
                    Action = KeyboardController.InvokeKeyPress
                });

            // Hazards toggle any time
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.INDH,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = (n, p, a) => true,
                    Action = KeyboardController.InvokeKeyPress
                });

            // Beacon can only turn on when it's off
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.BC,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = (n, p, a) => !this.TelemetryController.LastTelemetryData.TruckValues.CurrentValues.LightsValues.Beacon,
                    Action = KeyboardController.InvokeKeyPress
                });

            // Beacon can only turn off when it's on
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.BC,
                    ButtonPressType = ButtonPressType.Released,
                    IsActionValid = (n, p, a) => this.TelemetryController.LastTelemetryData.TruckValues.CurrentValues.LightsValues.Beacon,
                    Action = KeyboardController.InvokeKeyPress
                });

            // Wiper selector requires special attention
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.WI1,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = (n, p, a) => true,
                    Action = this.ButtonController.WiperSelection
                });
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.WI2,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = (n, p, a) => true,
                    Action = this.ButtonController.WiperSelection
                });
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.WI3,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = (n, p, a) => true,
                    Action = this.ButtonController.WiperSelection
                });
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.WI4,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = (n, p, a) => true,
                    Action = this.ButtonController.WiperSelection
                });

            // Lights selector requires special attention
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.LI1,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = (n, p, a) => true,
                    Action = this.ButtonController.LightsSelection
                });
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.LI2,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = (n, p, a) => true,
                    Action = this.ButtonController.LightsSelection
                });
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.LI3,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = (n, p, a) => true,
                    Action = this.ButtonController.LightsSelection
                });
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.LI4,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = (n, p, a) => !this.TelemetryController.LastTelemetryData.TruckValues.CurrentValues.LightsValues.BeamHigh,
                    Action = KeyboardController.InvokeKeyPress
                });
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.LI4,
                    ButtonPressType = ButtonPressType.Released,
                    IsActionValid = (n, p, a) => this.TelemetryController.LastTelemetryData.TruckValues.CurrentValues.LightsValues.BeamHigh,
                    Action = KeyboardController.InvokeKeyPress
                });

            // Tow can be used any time
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.TOW,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = (n, p, a) => true,
                    Action = KeyboardController.InvokeKeyPress
                });

            // Horns can be used any time
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.HRN,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = (n, p, a) => true,
                    Action = KeyboardController.InvokeKeyDown
                });
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.HRN,
                    ButtonPressType = ButtonPressType.Released,
                    IsActionValid = (n, p, a) => true,
                    Action = KeyboardController.InvokeKeyUp
                });

            // Screamer can be used any time
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.SCR,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = (n, p, a) => true,
                    Action = KeyboardController.InvokeKeyDown
                });
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.SCR,
                    ButtonPressType = ButtonPressType.Released,
                    IsActionValid = (n, p, a) => true,
                    Action = KeyboardController.InvokeKeyUp
                });

            // Flash can be used any time
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.FLS,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = (n, p, a) => true,
                    Action = KeyboardController.InvokeKeyDown
                });
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.FLS,
                    ButtonPressType = ButtonPressType.Released,
                    IsActionValid = (n, p, a) => true,
                    Action = KeyboardController.InvokeKeyUp
                });

            // Trailer brake can only be enabled when it's disabled
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.TB,
                    ButtonPressType = ButtonPressType.Pressed,
                    // TODO: figure out where to get the trailer brake status from
                    IsActionValid = (n, p, a) => true,
                    Action = KeyboardController.InvokeKeyPress
                });

            // Trailer brake can only be disabled when it's enabled
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.TB,
                    ButtonPressType = ButtonPressType.Released,
                    // TODO: figure out where to get the trailer brake status from
                    IsActionValid = (n, p, a) => true,
                    Action = KeyboardController.InvokeKeyPress
                });

            // Engine brake can only be enabled when it's disabled
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.ENB,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = (n, p, a) => !this.TelemetryController.LastTelemetryData.TruckValues.CurrentValues.MotorValues.BrakeValues.MotorBrake,
                    Action = KeyboardController.InvokeKeyPress
                });

            // Engine brake can only be disabled when it's enabled
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.ENB,
                    ButtonPressType = ButtonPressType.Released,
                    IsActionValid = (n, p, a) => this.TelemetryController.LastTelemetryData.TruckValues.CurrentValues.MotorValues.BrakeValues.MotorBrake,
                    Action = KeyboardController.InvokeKeyPress
                });

            // Retarder strengh can be decreased any time
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.RET1,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = (n, p, a) => n[Button.RET2],
                    Action = KeyboardController.InvokeKeyPress
                });

            // Retarder strengh can be increased any time
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.RET2,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = (n, p, a) => n[Button.RET1],
                    Action = KeyboardController.InvokeKeyPress
                });

            // Engine brake strengh can be decreased any time
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.ENB1,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = (n, p, a) => n[Button.ENB2],
                    Action = KeyboardController.InvokeKeyPress
                });

            // Engine brake strengh can be increased any time
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.ENB2,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = (n, p, a) => n[Button.ENB1],
                    Action = KeyboardController.InvokeKeyPress
                });

            // E-brake can only be engaged when it's disengaged
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.EB,
                    ButtonPressType = ButtonPressType.Pressed,
                    IsActionValid = (n, p, a) => !this.TelemetryController.LastTelemetryData.TruckValues.CurrentValues.MotorValues.BrakeValues.ParkingBrake,
                    Action = KeyboardController.InvokeKeyPress
                });

            // E-brake can only be disengaged when it's engaged
            this.ButtonController.AddButtonAction(
                new ButtonAction()
                {
                    Button = Button.EB,
                    ButtonPressType = ButtonPressType.Released,
                    IsActionValid = (n, p, a) => this.TelemetryController.LastTelemetryData.TruckValues.CurrentValues.MotorValues.BrakeValues.ParkingBrake,
                    Action = KeyboardController.InvokeKeyPress
                });
        }
        #endregion

        /// <summary>
        /// Do the LED interval logic
        /// </summary>
        /// <param name="state">Unused</param>
        private void doLedInterval(object? state = null)
        {
            // Handle LED interval on the controller
            this.LedController.HandleLedInterval();

            // Show readiness state while telemetry is not connected
            if (!this.TelemetryController.IsConnected)
            {
                this.LedController.ResetLedsExcept(LED.IND_H);
                this.LedController.EnableLed(LED.PWR);
                this.LedController.ToggleLed(LED.IND_H);
            }

            // Send LED state string to the box
            this.sendLedUpdate();
        }

        /// <summary>
        /// Send an LED state string to the box
        /// </summary>
        /// <param name="state">Unused</param>
        private void sendLedUpdate()
        {
            // Send LED state string
            this.SerialController.SendLedsUpdate(this.LedController.GetLedStates());
        }

        /// <summary>
        /// Try and connect to the button box through the specified COM port.
        /// This will connect and perform a handshake to verify proper connection.
        /// </summary>
        /// <param name="comPortName">Name of the COM port to connect to</param>
        /// <returns>True on successful connection, otherwise false</returns>
        public bool ConnectToBox(string comPortName)
        {
            AutoResetEvent waitForConnection = new AutoResetEvent(false);
            bool connected = false;
            SerialController.Connected += () => { connected = true; waitForConnection.Set(); };
            SerialController.VersionMismatch += upgradeRequired => { if (!upgradeRequired) return; connected = false; waitForConnection.Set(); };
            SerialController.Connect(comPortName);
            SerialController.Handshake();
            waitForConnection.WaitOne(TimeSpan.FromMilliseconds(500));
            return connected;
        }
    }
}
