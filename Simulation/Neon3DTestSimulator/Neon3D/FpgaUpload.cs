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
            FpgaUploadInformation.AppendText(data.Split('~')[1] + "\n");
            FpgaUploadInformation.AppendText("Amount of values:  \n");
            FpgaUploadInformation.AppendText(data.Split('~')[1].Split(new string[] { " , " }, StringSplitOptions.None).Length + "\n");
            radioAllObjects.Checked = true;
        }

        public FpgaUpload(SerialPort comPort, string data)
        {
            InitializeComponent();
            this.comPort = comPort;
            this.data = data;
            StartUpload.Enabled = true;
            FpgaUploadInformation.AppendText("Connected to FPGA! \n");
            FpgaUploadInformation.AppendText("Data to be send over:  \n");
            FpgaUploadInformation.AppendText(data.Split('~')[1] + "\n");
            FpgaUploadInformation.AppendText("Amount of values:  \n");
            FpgaUploadInformation.AppendText(data.Split('~')[1].Split(new string[] { " , " }, StringSplitOptions.None).Length + "\n");
            radioAllObjects.Checked = true;
        }

        private void setProgressBar(int maxValue)
        {
            try
            {
                if (InvokeRequired)
                {
                    
                    
                    this.Invoke(new Action<int>(setProgressBar), new object[] { maxValue });
                    return;
                }
                else
                {
                    FPGAProgress.Maximum = maxValue;
                    FPGAProgress.Value = 0;
                }
            }
            catch { }
            
            
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
            comPort.Close();

            string[] names = SerialPort.GetPortNames();
            //checks if comport is available (if so it opens the comport else it shows a message)
            if (names.Length != 0 && !comPort.IsOpen)
            {
                comPort.PortName = names[0];
                comPort.BaudRate = 115000;
                comPort.Handshake = 0;
                comPort.Open();
                comPort.RtsEnable = true;
                comPort.DtrEnable = true;
                comPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            }
            else
            {
                MessageBox.Show("FPGA not found");
            }
            Thread uploadThread = new Thread(new ThreadStart(() => uploadThreadProgramm()));
            uploadThread.Start();
        }

        private void uploadThreadProgramm()
        {
            isUploading = true;
            StartUpload.Enabled = true;
            string fpgaArray = data.Split('~')[0];
            string amountOfChar = fpgaArray.Length.ToString();

            char[] FPGA = fpgaArray.ToCharArray(0, fpgaArray.Length - 1);
            char[] amountOfCharArray = amountOfChar.ToCharArray(0, amountOfChar.Length);

            int i;

            comPort.Write("0");
            setProgressBar(FPGA.Length + 13);
            for (i = 0; i < FPGA.Length + 13; i++)
            {
                while (!received && i <= (FPGA.Length + 12) && !abortUpload) ;
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
                }else if(i == 5)
                {
                    if (radioAllObjects.Checked)
                    {
                        comPort.Write("1");
                    }else if (radioObject1.Checked)
                    {
                        comPort.Write("2");
                    }else if (radioObject2.Checked)
                    {
                        comPort.Write("3");
                    }
                }else if(i == 6)
                {
                    comPort.Write("i");
                }
                else if (i >= 7 && i <= 11)
                {

                    try
                    {
                        comPort.Write(amountOfCharArray[i - 7].ToString());
                    }
                    catch (Exception derp)
                    {
                        comPort.Write("t");
                    }
                }
                else if (i == 12)
                {
                    comPort.Write("n");
                }
                else
                {
                    updateProgressBar(i);
                    comPort.Write(FPGA[i - 13].ToString());
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
                try
                {
                    this.Close();
                }
                catch { }
                
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
        //event method for receving serial information
        private void DataReceivedHandler(
                         object sender,
                         SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadExisting();
            received = true;
        }
    }
}
