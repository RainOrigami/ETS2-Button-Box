using SCSSdkClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ETS2_Button_Box_Host
{
    public partial class ETS2ButtonBoxHost : Form
    {
        #region Serial Communication
        /// <summary>
        /// Delimiter character for sending and receiving serial data
        /// </summary>
        private const char delimiter = '\x00';

        /// <summary>
        /// Serial connection to the button box
        /// </summary>
        private SerialPort buttonBoxPort;

        /// <summary>
        /// Serial data from button box received.
        /// Handles all buttons, switches, rotary encoders and selectors.
        /// </summary>
        private void buttonBoxPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Read until receive queue is empty
            while (buttonBoxPort.BytesToRead > 0)
            {
                // Read button line
                string buttons = buttonBoxPort.ReadLine();

                // Validate length of button line
                if (buttons.Length != Enum.GetValues(typeof(Button)).Length)
                {
                    Console.WriteLine($"Received invalid button state (is {buttons.Length} should {Enum.GetValues(typeof(Button)).Length}): {buttons}");
                    continue;
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
        }
        #endregion

        #region LED Handling
        /// <summary>
        /// Time between toggling blinking LEDs in milliseconds
        /// </summary>
        private const double ledBlinkTimeout = 1000;

        /// <summary>
        /// Time between sending LED status to button box in milliseconds
        /// </summary>
        private const double ledSendInterval = 100;

        /// <summary>
        /// In (RunMode.Connected | RunMode.Inactive) mode stores the last LED toggle time for blinking LEDs
        /// </summary>
        private DateTime inactiveLedLastBlinkTime;

        /// <summary>
        /// Stores the time of the last time the LED status was sent to the button box
        /// </summary>
        private DateTime lastLedStatusSendTime;

        /// <summary>
        /// Stores the time of the last time the fuel LED has toggled in low fuel state
        /// </summary>
        private DateTime lastFuelWarningTime;

        /// <summary>
        /// Whether or not there is a fuel warning (fuel < fuelWarn%)
        /// </summary>
        private bool isFuelWarning;

        /// <summary>
        /// Defines the threshold under which fuel percentage value which LEDs are turned on
        /// </summary>
        private readonly IReadOnlyDictionary<int, LED> fuelLedThreshold = new Dictionary<int, LED>()
        {
            { 5, LED.F1 },
            { 10, LED.F2 },
            { 20, LED.F3 },
            { 30, LED.F4 },
            { 40, LED.F5 },
            { 50, LED.F6 },
            { 60, LED.F7 },
            { 70, LED.F8 },
            { 80, LED.F9 },
            { 90, LED.F10 }
        };

        /// <summary>
        /// Map of all current LED states
        /// </summary>
        private Dictionary<LED, bool> ledStates;

        /// <summary>
        /// Disables all LEDs.
        /// This has no effect until writeLEDStates is called.
        /// </summary>
        private void resetLeds()
        {
            foreach (LED led in Enum.GetValues(typeof(LED)))
                disableLed(led);
        }

        /// <summary>
        /// Disable specific LED.
        /// This has no effect until writeLEDStates is called.
        /// </summary>
        /// <param name="led">LED to disable</param>
        private void disableLed(LED led)
        {
            lock (this.ledStates)
                this.ledStates[led] = false;
        }

        /// <summary>
        /// Enables specific LED.
        /// This has no effect until writeLEDStates is called.
        /// </summary>
        /// <param name="led">LED to enable</param>
        private void enableLed(LED led)
        {
            lock (this.ledStates)
                this.ledStates[led] = true;
        }

        /// <summary>
        /// Toggle (invert) the state of a specific LED.
        /// This has no effect until writeLEDStates is called.
        /// </summary>
        /// <param name="led">LED to toggle</param>
        private void toggleLed(LED led)
        {
            lock (this.ledStates)
                this.ledStates[led] = !this.ledStates[led];
        }

        /// <summary>
        /// Sets the state of an LED
        /// </summary>
        /// <param name="led">Which LED to change the status of</param>
        /// <param name="enabled">true to enable LED, false to disable LED</param>
        private void setLedState(LED led, bool enabled)
        {
            if (enabled)
                enableLed(led);
            else
                disableLed(led);
        }

        /// <summary>
        /// Reset all LEDs except the specified ones
        /// </summary>
        /// <param name="leds">Which LEDs not to reset</param>
        private void resetLedsExcept(params LED[] leds)
        {
            foreach (LED led in Enum.GetValues(typeof(LED)))
                if (!leds.Contains(led))
                    disableLed(led);
        }

        /// <summary>
        /// LED scheduler handles updates to the LEDs that happen outside of the ETS2 SDK data handler (telemetry_Data)
        /// </summary>
        private async void ledScheduler()
        {
            this.runMode |= RunMode.Scheduler;
            this.resetLeds();

            while (true)
            {
                // Exit LED scheduler if not connected to button box anymore
                if (!this.buttonBoxPort.IsOpen || (this.runMode & RunMode.Connected) == 0)
                {
                    this.runMode &= ~RunMode.Scheduler;
                    return;
                }

                // Handle LEDs in (RunMode.Connected | RunMode.Inactive) mode
                if ((this.runMode & RunMode.Connected) != 0 && (this.runMode & RunMode.Active) == 0)
                {
                    // Reset all LEDs except INDH
                    this.resetLedsExcept(LED.IND_H);

                    // Enable PWR to show powered state
                    this.enableLed(LED.PWR);

                    // Wait for LED blink timeout
                    if ((DateTime.Now - this.inactiveLedLastBlinkTime).TotalMilliseconds >= ledBlinkTimeout)
                    {
                        // Toggle LED INDH to show connection to button box but no connection to SDK
                        this.toggleLed(LED.IND_H);

                        // Set last inactive LED toggle time to now
                        this.inactiveLedLastBlinkTime = DateTime.Now;
                    }
                }
                else if ((this.runMode & RunMode.Connected) != 0 && (this.runMode & RunMode.Active) != 0)
                {
                    if (this.isFuelWarning)
                    {
                        this.disableLed(LED.F1);
                        this.disableLed(LED.F2);
                        this.disableLed(LED.F3);
                        this.disableLed(LED.F4);
                        this.disableLed(LED.F5);
                        this.disableLed(LED.F6);
                        this.disableLed(LED.F7);
                        this.disableLed(LED.F8);
                        this.disableLed(LED.F9);
                        this.disableLed(LED.F10);

                        if ((DateTime.Now - this.lastFuelWarningTime).TotalMilliseconds >= ledBlinkTimeout)
                        {
                            this.toggleLed(LED.F0);
                            this.lastFuelWarningTime = DateTime.Now;
                        }
                    }
                }

                // Send the current LED states to the button box
                await this.writeLEDStates();
            }
        }

        /// <summary>
        /// Writes LED states to button box.
        /// Only does so when connection is established and program state is Connected.
        /// Respects and waits for ledSendInterval between sends.
        /// </summary>
        private async Task writeLEDStates()
        {
            // Check if connection to button box is open and if run mode is in valid state for sending LED states
            if (!this.buttonBoxPort.IsOpen || (this.runMode & RunMode.Connected) == 0)
                return;

            // Wait, if necessary, for the next ledSendInterval
            TimeSpan timeSinceLastLEDStatusSend = DateTime.Now - this.lastLedStatusSendTime;
            if (timeSinceLastLEDStatusSend.TotalMilliseconds < ledSendInterval)
                await Task.Delay((int)(ledSendInterval - timeSinceLastLEDStatusSend.TotalMilliseconds));

            // Send LED state to button box
            this.buttonBoxPort.Write($"{this.getLedStateString()}{delimiter}");
            this.lastLedStatusSendTime = DateTime.Now;
        }

        /// <summary>
        /// Build the LED state string that is being sent to the button box
        /// </summary>
        /// <returns>String of 0 or 1 for each available LED</returns>
        private string getLedStateString()
        {
            lock (this.ledStates)
                return String.Join("", Enum.GetValues(typeof(LED)).OfType<LED>().OrderBy(led => (int)led).Select(led => this.ledStates[led] ? "1" : "0"));
        }
        #endregion

        #region Button Handling
        private delegate void OnButtonStateChanged(Dictionary<Button, bool> newButtonStates, Dictionary<Button, bool> previousButtonStates);
        private event OnButtonStateChanged ButtonStateChanged;

        private Dictionary<Button, bool> buttonStates;

        private void buttonStateChangedHandler(Dictionary<Button, bool> newButtonStates, Dictionary<Button, bool> previousButtonStates)
        {
        }

        private static bool checkButtonPressed(Dictionary<Button, bool> newButtonStates, Dictionary<Button, bool> previousButtonStates, Button button) => newButtonStates[button] && !previousButtonStates[button];
        private static bool checkButtonReleased(Dictionary<Button, bool> newButtonStates, Dictionary<Button, bool> previousButtonStates, Button button) => !newButtonStates[button] && previousButtonStates[button];
        #endregion

        #region SCS Telemetry SDK
        /// <summary>
        /// ETS2 SDK client
        /// </summary>
        private SCSSdkTelemetry telemetry;

        /// <summary>
        /// ETS2 SDK data has been received
        /// </summary>
        private void telemetry_Data(SCSSdkClient.Object.SCSTelemetry data, bool newTimestamp)
        {
            // Check for SDK connection to a game instance
            if (!data.SdkActive)
            {
                // Disable Active run mode
                this.runMode &= ~RunMode.Active;
            }
            else if ((this.runMode & RunMode.Active) == 0)
            {
                // Enable Active run mode
                this.runMode |= RunMode.Active;

                // Reset LEDs
                this.resetLeds();

                // Enable the LED scheduler
                if ((this.runMode & RunMode.Scheduler) == 0)
                    Task.Run(ledScheduler);
            }

            // Do nothing if SDK not connected or button box not connected
            if ((this.runMode & RunMode.Active) == 0 || (this.runMode & RunMode.Connected) == 0)
                return;

            // Calculate percentage of fuel
            int fuelPercentage = (int)Math.Round(data.TruckValues.CurrentValues.DashboardValues.FuelValue.Amount / data.TruckValues.ConstantsValues.CapacityValues.Fuel * 100f);

            // Get fuel warning indicator
            this.isFuelWarning = data.TruckValues.CurrentValues.DashboardValues.WarningValues.FuelW;

            // When no fuel warning occurs set F0 LED fixed on
            if (!this.isFuelWarning && data.TruckValues.ConstantsValues.CapacityValues.Fuel > 0)
                this.enableLed(LED.F0);

            // Set individual F1-F10 LEDs based on the fuelLedThreshold
            foreach (int threshold in fuelLedThreshold.Keys)
            {
                if (fuelPercentage > threshold)
                    this.enableLed(fuelLedThreshold[threshold]);
                else
                    this.disableLed(fuelLedThreshold[threshold]);
            }

            // Handle system power LED
            this.setLedState(LED.PWR, data.TruckValues.CurrentValues.ElectricEnabled);

            // Handle engine power LED
            this.setLedState(LED.ENG, data.TruckValues.CurrentValues.EngineEnabled);

            // Handle brake LED
            this.setLedState(LED.BR, data.TruckValues.CurrentValues.LightsValues.Brake);

            // Handle cruise control LED
            this.setLedState(LED.CC, data.TruckValues.CurrentValues.DashboardValues.CruiseControl);

            // Handle motor brake LED
            this.setLedState(LED.ENB, data.TruckValues.CurrentValues.MotorValues.BrakeValues.MotorBrake);

            // Handle parking brake LED
            this.setLedState(LED.EB, data.TruckValues.CurrentValues.MotorValues.BrakeValues.ParkingBrake);

            // Handle retarder LED
            this.setLedState(LED.RET, data.TruckValues.CurrentValues.MotorValues.BrakeValues.RetarderLevel > 0);

            // Handle engine fault LED
            this.setLedState(LED.EF, data.TruckValues.CurrentValues.DamageValues.Engine > 0.25);

            // Handle high beam/flashers LED
            this.setLedState(LED.FLS, data.TruckValues.CurrentValues.LightsValues.BeamHigh);

            // Handle indicators
            this.setLedState(LED.IND_H, data.TruckValues.CurrentValues.LightsValues.BlinkerLeftOn && data.TruckValues.CurrentValues.LightsValues.BlinkerRightOn);
            this.setLedState(LED.IND_L, data.TruckValues.CurrentValues.LightsValues.BlinkerLeftOn);
            this.setLedState(LED.IND_R, data.TruckValues.CurrentValues.LightsValues.BlinkerRightOn);

            // Handle trailer LED
            this.setLedState(LED.TOW, data.TrailerValues.Any(t => t.Attached));
        }
        #endregion

        #region Logic
        /// <summary>
        /// Run mode of the application
        /// </summary>
        private enum RunMode
        {
            /// <summary>
            /// Connection to the button box has been established
            /// </summary>
            Connected = 1,
            /// <summary>
            /// ETS2 SDK is connected to a game instance
            /// </summary>
            Active = 2,
            /// <summary>
            /// Whether or not the scheduler is currently active
            /// </summary>
            Scheduler = 4
        }

        /// <summary>
        /// Current run mode of the application
        /// </summary>
        private RunMode runMode;
        #endregion

        #region Form and UI
        public ETS2ButtonBoxHost()
        {
            // WinForms initialisation
            InitializeComponent();

            // Initialise run mode to not connected, inactive and no scheduler running
            this.runMode = 0;

            // Initialise last LED toggle time to the current time
            this.inactiveLedLastBlinkTime = DateTime.Now;

            // Initialise last LED status time to the current time
            this.lastLedStatusSendTime = DateTime.Now;

            // Initialise last fuel warning LED time to current time
            this.lastFuelWarningTime = DateTime.Now;

            // Initialise the LED states map
            this.ledStates = new Dictionary<LED, bool>();

            // Fill LED states map with all available LEDs and turn them OFF
            foreach (LED led in Enum.GetValues(typeof(LED)))
                this.ledStates.Add(led, false);

            // Initialise the button states map
            this.buttonStates = new Dictionary<Button, bool>();

            // Fill button states map with all available buttons and turn them OFF
            foreach (Button button in Enum.GetValues(typeof(Button)))
                this.buttonStates.Add(button, false);

            // Hook on button state change event
            this.ButtonStateChanged += buttonStateChangedHandler;

            // Initialise serial connection to button box
            this.buttonBoxPort = new SerialPort();
            this.buttonBoxPort.BaudRate = 9600;
            this.buttonBoxPort.Parity = Parity.None;
            this.buttonBoxPort.DataBits = 8;
            this.buttonBoxPort.StopBits = StopBits.One;
            this.buttonBoxPort.DataReceived += this.buttonBoxPort_DataReceived;

            // Initialise ETS2 SDK client
            this.telemetry = new SCSSdkTelemetry();
            this.telemetry.Data += this.telemetry_Data;
        }

        /// <summary>
        /// Post initialisation routine gets UI elements ready for user interaction
        /// </summary>
        private void ETS2ButtonBoxHost_Load(object sender, EventArgs e)
        {
            this.reloadCOMPorts();
        }

        /// <summary>
        /// Fill available COM ports into COM port combobox
        /// </summary>
        private void reloadCOMPorts()
        {
            this.cbCOMPort.Items.Clear();
            this.cbCOMPort.Items.AddRange(SerialPort.GetPortNames());
        }

        /// <summary>
        /// Connect to COM port button click handler
        /// </summary>
        private void btnConnect_Click(object sender, EventArgs e)
        {
            // Get and validate the selected COM port
            string comPort = this.cbCOMPort.SelectedItem as string;

            if (string.IsNullOrEmpty(comPort))
            {
                MessageBox.Show("Please select a COM port from the list.", "No COM port selected", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!SerialPort.GetPortNames().Contains(comPort))
            {
                MessageBox.Show("The selected COM port is not available anymore. Please check the connection and try again.", "COM port not available", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.reloadCOMPorts();
                return;
            }

            // Assign COM port and open connection to button box
            this.buttonBoxPort.PortName = comPort;
            this.buttonBoxPort.Open();

            if (this.buttonBoxPort.IsOpen)
                this.runMode |= RunMode.Connected;

            // Enable the LED scheduler
            if ((this.runMode & RunMode.Scheduler) == 0)
                Task.Run(ledScheduler);
        }

        /// <summary>
        /// Reload COM ports button click handler
        /// </summary>
        private void btnReload_Click(object sender, EventArgs e)
        {
            this.reloadCOMPorts();
        }
        #endregion
    }
}
