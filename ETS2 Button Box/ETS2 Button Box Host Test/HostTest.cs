using ETS2_Button_Box_Library;
using System.IO.Ports;
using System.Text;

namespace ETS2_Button_Box_Host_Test
{
    public partial class HostTest : Form
    {
        private SerialController serialController;

        public HostTest()
        {
            InitializeComponent();

            serialController = new SerialController();
            serialController.ButtonChanged += SerialController_ButtonChanged;
            serialController.DebugMessage += this.SerialController_DebugMessage;
        }

        private void SerialController_DebugMessage(string message)
        {
            if (InvokeRequired)
            {
                Invoke(() => SerialController_DebugMessage(message));
                return;
            }

            textBox1.AppendText(message);
            textBox1.AppendText(Environment.NewLine);
        }

        private void SerialController_ButtonChanged(byte buttonIndex, bool buttonState)
        {
            if (InvokeRequired)
            {
                Invoke(() => SerialController_ButtonChanged(buttonIndex, buttonState));
                return;
            }

            if (txtButtonStates.Text.Length <= buttonIndex)
            {
                txtButtonStates.Text += "".PadRight((buttonIndex + 1) - txtButtonStates.Text.Length);
            }

            StringBuilder text = new StringBuilder(txtButtonStates.Text);
            text[buttonIndex] = buttonState ? '1' : '0';
            txtButtonStates.Text = text.ToString();
        }

        private void HostTest_Load(object sender, EventArgs e)
        {
            cbCOMPort.Items.AddRange(SerialPort.GetPortNames());
            cbCOMPort.SelectedIndex = cbCOMPort.Items.Count - 1;
        }

        private void btnSetLed_Click(object sender, EventArgs e)
        {
            if (colorDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            Color color = colorDialog.Color;
            serialController.SendLedsUpdate(new Dictionary<byte, Color>() { { (byte)nudLedIndex.Value, color } });
        }

        private void txtConnect_Click(object sender, EventArgs e) => serialController.Connect((string)cbCOMPort.SelectedItem);

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            lblBrightness.Text = trackBar1.Value.ToString();
            serialController.SendBrightness((byte)trackBar1.Value);
        }
    }
}
