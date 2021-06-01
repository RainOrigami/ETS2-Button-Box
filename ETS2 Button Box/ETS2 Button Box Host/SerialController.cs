using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ETS2_Button_Box_Host
{
    public class SerialController
    {
        /// <summary>
        /// Handle a button state change received event
        /// </summary>
        /// <param name="buttonState">The button state that was received</param>
        public delegate void OnButtonStateChangeReceived(string buttonState);

        /// <summary>
        /// A button state change event has been received
        /// </summary>
        public event OnButtonStateChangeReceived ButtonStateChangeReceived;

        /// <summary>
        /// Serial connection to the button box
        /// </summary>
        private SerialPort buttonBoxPort;

        /// <summary>
        /// Regular expression to evaluate and parse a button line
        /// </summary>
        private Regex buttonLineRegex;

        /// <summary>
        /// Regular expression to evaluate a handshake
        /// </summary>
        private Regex handshakeLineRegex;

        /// <summary>
        /// Handshake completion source to indicate a successfully received handshake
        /// </summary>
        TaskCompletionSource<bool> handshakeCompletionSource;

        /// <summary>
        /// Returns true when serial connection is open
        /// </summary>
        public bool IsConnected => this.buttonBoxPort.IsOpen;

        /// <summary>
        /// Initialise the serial controller
        /// </summary>
        public SerialController()
        {
            // Initialise serial connection to button box
            this.buttonBoxPort = new SerialPort();
            this.buttonBoxPort.BaudRate = 9600;
            this.buttonBoxPort.Parity = Parity.None;
            this.buttonBoxPort.DataBits = 8;
            this.buttonBoxPort.StopBits = StopBits.One;
            this.buttonBoxPort.NewLine = "\n";
            this.buttonBoxPort.DataReceived += this.buttonBoxPort_DataReceived;

            // Initialise regular expressions
            this.buttonLineRegex = new Regex("^BTN\\[([01]{40})\\]$");
            this.handshakeLineRegex = new Regex("^HANDSHAKE$");
        }

        /// <summary>
        /// Serial data from button box received.
        /// Handles all buttons, switches, rotary encoders and selectors.
        /// </summary>
        private void buttonBoxPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Read until receive queue is empty
            while (buttonBoxPort.BytesToRead > 0)
            {
                // Read line
                string line = buttonBoxPort.ReadLine();

                // Check for button event
                Match match = this.buttonLineRegex.Match(line);
                if (match.Success)
                {
                    // Invoke button state change event
                    this.ButtonStateChangeReceived?.Invoke(match.Groups[1].Value);
                    continue;
                }

                // Check for handshake event
                match = this.handshakeLineRegex.Match(line);
                if (match.Success)
                {
                    // Indicate a sucessful handshake completion
                    this.handshakeCompletionSource?.TrySetResult(true);
                    return;
                }

                Console.WriteLine($"Unhandled message received: \"{line}\".");
            }
        }

        /// <summary>
        /// Perform and validate a handshake with the button box
        /// </summary>
        /// <returns>True when handshake was successful, otherwise false</returns>
        public bool Handshake()
        {
            // Verify open connection
            if (!this.buttonBoxPort.IsOpen)
                return false;

            // Send a handshake
            this.buttonBoxPort.WriteLine("HANDSHAKE");

            // Initialise handshake task
            this.handshakeCompletionSource = new TaskCompletionSource<bool>();

            // Wait for a maximum of 200ms for the handshake to complete, otherwise fail the handshake
            bool handshakeResult = this.handshakeCompletionSource.Task.Wait(200);

            return handshakeResult;
        }

        public void Disconnect()
        {
            // Close the COM port
            this.buttonBoxPort.Close();
        }

        public void SendLedState(string ledStateString)
        {
            // Verify open connection
            if (!this.buttonBoxPort.IsOpen)
                return;

            // Send LED state string
            this.buttonBoxPort.WriteLine($"LED[{ledStateString}]");
        }

        /// <summary>
        /// Connect to the specified COM port
        /// </summary>
        /// <param name="comPortName">COM port name to connect to</param>
        public void Connect(string comPortName)
        {
            // Reconnect when already connected
            if (this.buttonBoxPort.IsOpen)
                this.Disconnect();

            // Assign COM port and open connection to button box
            this.buttonBoxPort.PortName = comPortName;
            this.buttonBoxPort.Open();
        }
    }
}
