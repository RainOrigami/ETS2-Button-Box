using SCSSdkClient;
using SCSSdkClient.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETS2_Button_Box_Host
{
    public class TelemetryController
    {
        public delegate void OnTelemetryChanged(SCSTelemetry telemetry);
        public event OnTelemetryChanged TelemetryChanged;

        /// <summary>
        /// ETS2 SDK client
        /// </summary>
        private SCSSdkTelemetry telemetry;

        /// <summary>
        /// Last received set of telemetry data
        /// </summary>
        public SCSTelemetry LastTelemetryData { get; private set; }

        /// <summary>
        /// Returns true if telemetry data was received within TelemetryTimeout
        /// </summary>
        public bool IsConnected => this.LastTelemetryData != null && this.LastTelemetryData.SdkActive;

        public TelemetryController()
        {
            // Initialise ETS2 SDK client
            this.telemetry = new SCSSdkTelemetry();
            this.telemetry.Data += this.telemetry_Data;
        }

        /// <summary>
        /// ETS2 SDK data has been received
        /// </summary>
        private void telemetry_Data(SCSTelemetry data, bool newTimestamp)
        {
            this.LastTelemetryData = data;

            if (this.IsConnected)
                this.TelemetryChanged?.Invoke(this.LastTelemetryData);
        }

    }
}
