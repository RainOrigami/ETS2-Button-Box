using SCSSdkClient.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETS2_Button_Box_Host
{
    public class LEDController
    {
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
        /// Initialise LED controller
        /// </summary>
        public LEDController()
        {
            // Initialise the LED states map
            this.ledStates = new Dictionary<LED, bool>();

            // Fill LED states map with all available LEDs and turn them OFF
            foreach (LED led in Enum.GetValues(typeof(LED)))
                this.ledStates.Add(led, false);
        }

        /// <summary>
        /// Disables all LEDs.
        /// This has no effect until writeLEDStates is called.
        /// </summary>
        public void ResetLeds()
        {
            foreach (LED led in Enum.GetValues(typeof(LED)))
                DisableLed(led);
        }

        /// <summary>
        /// Disable specific LED.
        /// This has no effect until writeLEDStates is called.
        /// </summary>
        /// <param name="led">LED to disable</param>
        public void DisableLed(LED led)
        {
            lock (this.ledStates)
                this.ledStates[led] = false;
        }

        /// <summary>
        /// Enables specific LED.
        /// This has no effect until writeLEDStates is called.
        /// </summary>
        /// <param name="led">LED to enable</param>
        public void EnableLed(LED led)
        {
            lock (this.ledStates)
                this.ledStates[led] = true;
        }

        /// <summary>
        /// Toggle (invert) the state of a specific LED.
        /// This has no effect until writeLEDStates is called.
        /// </summary>
        /// <param name="led">LED to toggle</param>
        public void ToggleLed(LED led)
        {
            lock (this.ledStates)
                this.ledStates[led] = !this.ledStates[led];
        }

        /// <summary>
        /// Sets the state of an LED
        /// </summary>
        /// <param name="led">Which LED to change the status of</param>
        /// <param name="enabled">true to enable LED, false to disable LED</param>
        public void SetLedState(LED led, bool enabled)
        {
            if (enabled)
                EnableLed(led);
            else
                DisableLed(led);
        }

        /// <summary>
        /// Reset all LEDs except the specified ones
        /// </summary>
        /// <param name="leds">Which LEDs not to reset</param>
        public void ResetLedsExcept(params LED[] leds)
        {
            foreach (LED led in Enum.GetValues(typeof(LED)))
                if (!leds.Contains(led))
                    DisableLed(led);
        }

        /// <summary>
        /// Build the LED state string that is being sent to the button box
        /// </summary>
        /// <returns>String of 0 or 1 for each available LED</returns>
        public string GetLedStateString()
        {
            lock (this.ledStates)
                return String.Join("", Enum.GetValues(typeof(LED)).OfType<LED>().OrderBy(led => (int)led).Select(led => this.ledStates[led] ? "1" : "0"));
        }

        public void HandleTelemetryData(SCSTelemetry data)
        {
            // Calculate percentage of fuel
            int fuelPercentage = (int)Math.Round(data.TruckValues.CurrentValues.DashboardValues.FuelValue.Amount / data.TruckValues.ConstantsValues.CapacityValues.Fuel * 100f);

            // Get fuel warning indicator
            this.isFuelWarning = data.TruckValues.CurrentValues.DashboardValues.WarningValues.FuelW;

            // When no fuel warning occurs set F0 LED fixed on
            if (!this.isFuelWarning && data.TruckValues.ConstantsValues.CapacityValues.Fuel > 0)
                this.EnableLed(LED.F0);

            // Set individual F1-F10 LEDs based on the fuelLedThreshold
            foreach (int threshold in fuelLedThreshold.Keys)
            {
                if (fuelPercentage > threshold)
                    this.EnableLed(fuelLedThreshold[threshold]);
                else
                    this.DisableLed(fuelLedThreshold[threshold]);
            }

            // Handle system power LED
            this.SetLedState(LED.PWR, data.TruckValues.CurrentValues.ElectricEnabled);

            // Handle engine power LED
            this.SetLedState(LED.ENG, data.TruckValues.CurrentValues.EngineEnabled);

            // Handle brake LED
            this.SetLedState(LED.BR, data.TruckValues.CurrentValues.LightsValues.Brake);

            // Handle cruise control LED
            this.SetLedState(LED.CC, data.TruckValues.CurrentValues.DashboardValues.CruiseControl);

            // Handle motor brake LED
            this.SetLedState(LED.ENB, data.TruckValues.CurrentValues.MotorValues.BrakeValues.MotorBrake);

            // Handle parking brake LED
            this.SetLedState(LED.EB, data.TruckValues.CurrentValues.MotorValues.BrakeValues.ParkingBrake);

            // Handle retarder LED
            this.SetLedState(LED.RET, data.TruckValues.CurrentValues.MotorValues.BrakeValues.RetarderLevel > 0);

            // Handle engine fault LED
            this.SetLedState(LED.EF, data.TruckValues.CurrentValues.DamageValues.Engine > 0.25);

            // Handle high beam/flashers LED
            this.SetLedState(LED.FLS, data.TruckValues.CurrentValues.LightsValues.BeamHigh || this.flasher);

            // Handle beacon LED
            this.SetLedState(LED.BC, data.TruckValues.CurrentValues.LightsValues.Beacon);

            // Handle indicators
            this.SetLedState(LED.IND_H, data.TruckValues.CurrentValues.LightsValues.BlinkerLeftOn && data.TruckValues.CurrentValues.LightsValues.BlinkerRightOn);
            this.SetLedState(LED.IND_L, data.TruckValues.CurrentValues.LightsValues.BlinkerLeftOn);
            this.SetLedState(LED.IND_R, data.TruckValues.CurrentValues.LightsValues.BlinkerRightOn);

            // Handle trailer LED
            this.SetLedState(LED.TOW, data.TrailerValues.Any(t => t.Attached));
        }

        private bool flasher = false;

        public void HandleChangedButtonState(Dictionary<Button, bool> newButtonStates, Dictionary<Button, bool> previousButtonStates)
        {
            // Handle horn LED
            this.SetLedState(LED.HRN, newButtonStates[Button.HRN]);

            // Handle scream LED
            this.SetLedState(LED.SCR, newButtonStates[Button.SCR]);

            // Handle flasher LED
            this.SetLedState(LED.FLS, newButtonStates[Button.FLS]);
            this.flasher = newButtonStates[Button.FLS];

        }

        /// <summary>
        /// Handle LED interval, should be called periodically at an interval at which flashing LEDs should toggle
        /// </summary>
        public void HandleLedInterval()
        {
            if (this.isFuelWarning)
                this.ToggleLed(LED.F0);
        }
    }
}
