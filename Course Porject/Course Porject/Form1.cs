using Microsoft.VisualBasic.Logging;
using System.Net.Sockets;

namespace Course_Porject
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        TcpClient client;
        NetworkStream stream;
        byte[] selectedFileBytes;
        string selectedFileName;
        private async void button1_Click(object sender, EventArgs e)
        {
            client = new TcpClient();
            await client.ConnectAsync("127.0.0.1", 5000);
            stream = client.GetStream();
            listBox1.Items.Add("Connected to server!");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                selectedFileBytes = File.ReadAllBytes(dialog.FileName);
                selectedFileName = Path.GetFileName(dialog.FileName);
                listBox1.Items.Add($"Selected: {selectedFileName} ({selectedFileBytes.Length} bytes)");
            }
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            byte[] sizeBytes = BitConverter.GetBytes(selectedFileBytes.Length);
            await stream.WriteAsync(sizeBytes, 0, 4);


            await stream.WriteAsync(selectedFileBytes, 0, selectedFileBytes.Length);
            listBox1.Items.Add("File sent! Waiting for compressed version...");
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            byte[] compSizeBuffer = new byte[4];
            await stream.ReadAsync(compSizeBuffer, 0, 4);
            int compSize = BitConverter.ToInt32(compSizeBuffer, 0);
            listBox1.Items.Add($"Receiving compressed file ({compSize} bytes)...");

            
            byte[] compBytes = new byte[compSize];
            int totalRead = 0;
            while (totalRead < compSize)
            {
                int read = await stream.ReadAsync(compBytes, totalRead, compSize - totalRead);
                if (read == 0) break;
                totalRead += read;
            }
        }
    }
}
