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

        private static readonly TimeSpan TelemetryTimeout = new TimeSpan(0, 0, 2);

        private DateTime lastReceivedTelemetryDataTime = DateTime.MinValue;

        /// <summary>
        /// ETS2 SDK client
        /// </summary>
        private SCSSdkTelemetry telemetry;

        /// <summary>
        /// Last received set of telemetry data
        /// </summary>
        public SCSTelemetry LastTelemetryData { get; private set; }

        public bool IsTelemetryConnected => (DateTime.Now - this.lastReceivedTelemetryDataTime) > TelemetryTimeout;

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
            this.lastReceivedTelemetryDataTime = DateTime.Now;
            this.LastTelemetryData = data;

            this.TelemetryChanged?.Invoke(this.LastTelemetryData);
        }

    }
}
