using System;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;

namespace Neon3D
{
    public partial class FpgaUpload : Form
    {
        public SerialPort comPort = new SerialPort();
        public string data = "";
        public bool received = false;
        public bool isUploading = false;
        public bool abortUpload = false;


        public FpgaUpload(string data)
        {
            InitializeComponent();
            StartUpload.Enabled = false;
            this.data = data;
            FpgaUploadInformation.AppendText("Not connected to FPGA! \n");
            FpgaUploadInformation.AppendText("Data to be send over:  \n");
            FpgaUploadInformation.AppendText(data + "\n");
        }

        public FpgaUpload(SerialPort comPort, string data)
        {
            InitializeComponent();
            this.comPort = comPort;
            this.data = data;
            StartUpload.Enabled = true;
            FpgaUploadInformation.AppendText("Connected to FPGA! \n");
            FpgaUploadInformation.AppendText("Data to be send over:  \n");
            FpgaUploadInformation.AppendText(data + "\n");
        }

        private void setProgressBar(int maxValue, int startValue)
        {
            FPGAProgress.Maximum = maxValue;
            FPGAProgress.Value = startValue;
        }

        private void updateProgressBar(int currentValue)
        {
           
            try
            {
                if (InvokeRequired)
                {
                    this.Invoke(new Action<int>(updateProgressBar), new object[] { currentValue });
                    return;
                }
                else
                {
                    FPGAProgress.Value = currentValue;
                }
            }
            catch
            {

            }
        }

        private void StartUpload_Click(object sender, EventArgs e)
        {
            Thread uploadThread = new Thread(new ThreadStart(() => uploadThreadProgramm()));
            uploadThread.Start();
        }

        private void uploadThreadProgramm()
        {
            isUploading = true;
            StartUpload.Enabled = false;
            string fpgaArray = data;
            string amountOfChar = fpgaArray.Length.ToString();

            char[] FPGA = fpgaArray.ToCharArray(0, fpgaArray.Length - 1);
            char[] amountOfCharArray = amountOfChar.ToCharArray(0, amountOfChar.Length);

            int i;

            comPort.Write("0");
            setProgressBar(FPGA.Length + 10, 0);
            for (i = 0; i < FPGA.Length + 10; i++)
            {
                while (!received && i <= (FPGA.Length + 10) && !abortUpload) ;
                if (abortUpload)
                {
                    break;
                }
                if (i < 4)
                {
                    comPort.Write("1");
                }
                else if (i == 4)
                {
                    comPort.Write("t");
                }
                else if (i >= 5 && i <= 8)
                {

                    try
                    {
                        comPort.Write(amountOfCharArray[i - 5].ToString());
                    }
                    catch (Exception derp)
                    {
                        comPort.Write("t");
                    }
                }
                else if (i == 9)
                {
                    comPort.Write("n");
                }
                else
                {
                    updateProgressBar(i);
                    comPort.Write(FPGA[i - 10].ToString());
                }
                received = false;
            }
            Thread.Sleep(100);

            for (int x = 0; x < 1; x++)
            {
                while (!received) ;
                comPort.Write("d");
                received = false;
            }

            if (abortUpload)
            {

                isUploading = false;
                abortUpload = false;
                updateProgressBar(0);
                this.Close();
            }

            updateProgressBar(i);
            StartUpload.Enabled = true;
            isUploading = false;
        }

        private void AbortUpload_Click(object sender, EventArgs e)
        {
            if (isUploading)
            {
                abortUpload = true;
            } else
            {
                this.Close();
            }
        }
    }
}
