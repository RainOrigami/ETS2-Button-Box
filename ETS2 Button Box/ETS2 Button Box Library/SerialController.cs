using System.Drawing;
using System.IO.Ports;
using System.Text.RegularExpressions;

namespace ETS2_Button_Box_Library
{
    /// <summary>
    /// Handle serial communication with the button box arduino
    /// </summary>
    public class SerialController
    {
        // Current version, used to check against handshake for compatibility
        private const int VERSION_MAJOR = 2;
        private const int VERSION_MINOR = 10;

        private SerialPort buttonBoxPort;   // Serial connection to the button box arduino

        private static Regex handshakeExpression = new("^HANDSHAKE: (?<major>\\d+)\\.(?<minor>\\d+)$", RegexOptions.Compiled); // Regex for matching and extracting handshake version information

        private static byte[] handshakeBuffer = new byte[]
        {
            42,
            (byte)'H', (byte)'A', (byte)'N', (byte)'D', (byte)'S', (byte)'H', (byte)'A', (byte)'K', (byte)'E', (byte)'\n'
        };

        public delegate void OnDebugMessage(string message);
        /// <summary>
        /// Debug message from button box arduino has been received
        /// </summary>
        public event OnDebugMessage? DebugMessage;

        public delegate void OnButtonChanged(byte buttonIndex, bool buttonState);
        /// <summary>
        /// Single button change event
        /// </summary>
        public event OnButtonChanged? ButtonChanged;

        public delegate void OnVersionMismatch(bool upgradeRequired);
        /// <summary>
        /// Version mismatch after handshake
        /// </summary>
        public event OnVersionMismatch? VersionMismatch;

        public delegate void OnConnected();
        /// <summary>
        /// Connection successful, after handshake
        /// </summary>
        public event OnConnected? Connected;

        /// <summary>
        /// Whether the serial port is open or not
        /// </summary>
        public bool IsConnected => buttonBoxPort.IsOpen;

        /// <summary>
        /// Initialise serial controller
        /// </summary>
        public SerialController()
        {
            // Setup serial port
            buttonBoxPort = new SerialPort
            {
                BaudRate = 115200,
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One,
                NewLine = "\n"
            };

            // Hook onto DataReceived event of serial port
            buttonBoxPort.DataReceived += ButtonBoxPort_DataReceived;

            // Handle version mismatch for possible force disconnect when update is required
            VersionMismatch += SerialController_VersionMismatch;
        }

        /// <summary>
        /// Handles version mismatch from handshake
        /// </summary>
        /// <param name="upgradeRequired"></param>
        private void SerialController_VersionMismatch(bool upgradeRequired)
        {
            // Program can continue since version mismatch does not require an upgrade
            if (!upgradeRequired)
            {
                Connected?.Invoke();
                return;
            }

            // Close port because version mismatch is too great
            buttonBoxPort.Close();
        }

        /// <summary>
        /// Handle data received by the serial port
        /// </summary>
        private void ButtonBoxPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Handle the magic number
            switch (buttonBoxPort.ReadByte())
            {
                case 42:
                    handleHandshake();
                    break;
                case 3:
                    handleButtonStates();
                    break;
                case 9:
                    handleDebugMessage();
                    break;
            }
        }

        /// <summary>
        /// Handle reading of a debug message from serial port
        /// </summary>
        private void handleDebugMessage()
        {
            // Invoke debug message event with data read from serial port
            DebugMessage?.Invoke(buttonBoxPort.ReadLine());
        }

        /// <summary>
        /// Handle reading of button states from serial port
        /// </summary>
        private void handleButtonStates()
        {
            Dictionary<int, bool> buttonStates = new();

            // Read amount of changed buttons
            int buttonCount = buttonBoxPort.ReadByte();

            for (int buttonDataIndex = 0; buttonDataIndex < buttonCount; buttonDataIndex++)
            {
                // Call button changed event for every changed button with button index and state
                ButtonChanged?.Invoke((byte)buttonBoxPort.ReadByte(), buttonBoxPort.ReadByte() == 1);
            }
        }

        /// <summary>
        /// Handle reading of the handshake from serial port
        /// </summary>
        /// <exception cref="Exception">Handshake message did not match expected value (regex handshakeExpression)</exception>
        private void handleHandshake()
        {
            // Read handshake from serial port
            string handshake = buttonBoxPort.ReadLine();

            // Try and match the handshake against the handshake expression
            Match handshakeMatch = handshakeExpression.Match(handshake);
            if (!handshakeMatch.Success)
            {
                throw new Exception($"Handshake failed, received value does not match expected value.{Environment.NewLine}Value: \"{handshake}\"");
            }

            // Extract major and minor version from handshake and pass on to version handler
            handleVersion(int.Parse(handshakeMatch.Groups["major"].Value), int.Parse(handshakeMatch.Groups["minor"].Value));
        }

        /// <summary>
        /// Handle version from handshake
        /// </summary>
        /// <param name="major">Major version</param>
        /// <param name="minor">Minor version</param>
        private void handleVersion(int major, int minor)
        {
            // Version 2.10 is exact match, do nothing
            if (major == VERSION_MAJOR && minor == VERSION_MINOR)
            {
                return;
            }

            // Mismatch detected, invoke version mismatch event and calculate whether an update is required or not
            VersionMismatch?.Invoke(true);
        }

        /// <summary>
        /// Connect to the specified COM port
        /// </summary>
        /// <param name="comPortName">COM port name to connect to</param>
        /// <exception cref="Exception">Specified COM port does not exist</exception>
        public void Connect(string comPortName)
        {
            // Make sure specified COM port name exists
            if (SerialPort.GetPortNames().All(portName => portName != comPortName))
            {
                throw new Exception($"The specified COM port \"{comPortName}\" does not exist.");
            }

            // Reconnect when already connected
            if (buttonBoxPort.IsOpen)
            {
                Disconnect();
            }

            // Assign COM port and open connection to button box
            buttonBoxPort.PortName = comPortName;
            buttonBoxPort.Open();
        }

        /// <summary>
        /// Disconnect from the serial port
        /// </summary>
        public void Disconnect() => buttonBoxPort.Close();

        /// <summary>
        /// Send updated LED colors to the button box arduino
        /// </summary>
        /// <param name="ledsUpdate">Dictionary of LED indices and their corresponding color (Color.Black for off)</param>
        public void SendLedsUpdate(Dictionary<byte, Color> ledsUpdate)
        {
            // Buffer sized for magic number, length and LED indices and RGB values
            byte[] buffer = new byte[ledsUpdate.Count * 4 + 2];
            buffer[0] = 1; // Magic number for LED states
            buffer[1] = (byte)ledsUpdate.Count; // Amount of changed LEDs

            int ledUpdateIndex = 0;
            foreach (KeyValuePair<byte, Color> led in ledsUpdate)
            {
                // Set LED index and RGB values
                buffer[2 + (4 * ledUpdateIndex) + 0] = led.Key;
                buffer[2 + (4 * ledUpdateIndex) + 1] = led.Value.R;
                buffer[2 + (4 * ledUpdateIndex) + 2] = led.Value.G;
                buffer[2 + (4 * ledUpdateIndex) + 3] = led.Value.B;
            }

            // Send LED updates to button box arduino
            buttonBoxPort.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Set the global brightness of all LEDs
        /// </summary>
        /// <param name="brightness">Brightness value between 0 (off) and 255 (burn your eyes out)</param>
        public void SendBrightness(byte brightness) => buttonBoxPort.Write(new byte[] { 2 /* Magic number Global brightness */, brightness }, 0, 2);

        /// <summary>
        /// Send a handshake to the button box arduino to initialise connection
        /// </summary>
        public void Handshake()
        {
            buttonBoxPort.Write(handshakeBuffer, 0, handshakeBuffer.Length);
        }
    }
}