﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SerialPortListener.Serial;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;



namespace SerialPortListener
{
    public partial class MainForm : Form
    {
        SerialPortManager _spManager;
        Socket SClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        string str = "a";
        public MainForm()
        {
            InitializeComponent();

            UserInitialization();

            CheckForIllegalCrossThreadCalls = false;
        }

        List<string> listGetByte = new List<string>();
        List<string> listSendByte = new List<string>();
        List<string> listStore = new List<string>();

        public void getMsg()
        {

            try
            {
                while (true)
                {
                    byte[] b = new byte[1024];
                    int R = SClient.Receive(b);
                    if (R > 0)
                    {

                        //!   ListShowmsg.Items.Add(ascint);Decimal(Encoding.Default.GetString(b, 0, R)
                        ListShowmsg.Items.Add(Encoding.Default.GetString(b, 0, R));
                        //!! ListShowmsg.Items.Add(",");
                        listGetByte.Add(Encoding.ASCII.GetString(b, 0, R));
                        // ListShowmsg.Items.Add("Server :" + Decimal(Encoding.ASCII.GetString(b, 0, R)));

                        //  SoundPlayer simpleSound = new SoundPlayer(strAudioFilePath);
                        //  simpleSound.Play();
                    }
                }
            }
            catch
            {
                ;
            }

        }

        private void UserInitialization()
        {
            _spManager = new SerialPortManager();
            SerialSettings mySerialSettings = _spManager.CurrentSerialSettings;
            serialSettingsBindingSource.DataSource = mySerialSettings;
            portNameComboBox.DataSource = mySerialSettings.PortNameCollection;
            baudRateComboBox.DataSource = mySerialSettings.BaudRateCollection;
            dataBitsComboBox.DataSource = mySerialSettings.DataBitsCollection;
            parityComboBox.DataSource = Enum.GetValues(typeof(System.IO.Ports.Parity));
            stopBitsComboBox.DataSource = Enum.GetValues(typeof(System.IO.Ports.StopBits));

            _spManager.NewSerialDataRecieved += new EventHandler<SerialDataEventArgs>(_spManager_NewSerialDataRecieved);
            this.FormClosing += new FormClosingEventHandler(MainForm_FormClosing);
        }


        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _spManager.Dispose();
        }

        void _spManager_NewSerialDataRecieved(object sender, SerialDataEventArgs e)
        {
            if (this.InvokeRequired)
            {
                // Using this.Invoke causes deadlock when closing serial port, and BeginInvoke is good practice anyway.
                this.BeginInvoke(new EventHandler<SerialDataEventArgs>(_spManager_NewSerialDataRecieved), new object[] { sender, e });
                return;
            }

            int maxTextLength = 1000; // maximum text length in text box
            if (tbData.TextLength > maxTextLength)
                tbData.Text = tbData.Text.Remove(0, tbData.TextLength - maxTextLength);

            // This application is connected to a GPS sending ASCCI characters, so data is converted to text
            str = Encoding.ASCII.GetString(e.Data);
            listStore.Add(str);
          //  tbData.AppendText(str);
            // tbData.ScrollToCaret();

        }

        // Handles the "Start Listening"-buttom click event
        private void btnStart_Click(object sender, EventArgs e)
        {
            _spManager.StartListening();
        }

        // Handles the "Stop Listening"-buttom click event
        private void btnStop_Click(object sender, EventArgs e)
        {
            _spManager.StopListening();
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            IPEndPoint IpServe = new IPEndPoint(IPAddress.Parse(txtIp.Text), int.Parse(txtPort.Text));
            try
            {
                SClient.Connect(IpServe);
                MessageBox.Show("Connect ");

                Thread tr = new Thread(new ThreadStart(getMsg));
                tr.Start();


                Thread ts = new Thread(new ThreadStart(sendMsg));
                ts.Start();

            }
            catch
            {
                MessageBox.Show("Server  not Start");
            }
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            if (SClient != null)
            {
                SClient.Shutdown(SocketShutdown.Both);
                Environment.Exit(Environment.ExitCode);
                Application.Exit();
            }
            else
            {
                Application.Exit();
            }
        }

        private void BtnWave_Click(object sender, EventArgs e)
        {

            System.IO.File.WriteAllLines("E:/danny/app/SavedRecLists.txt", listGetByte);
            System.IO.File.WriteAllLines("E:/danny/app/SavedSendLists.txt", listSendByte);//listStore
            System.IO.File.WriteAllLines("E:/danny/app/SavedStoreLists.txt", listStore);

        }

        private void sendMsg()
        {



            while (true)
            {

                string Nstr = threeNumber(str);

                byte[] barray = new byte[256];

           //     label3.Text = Nstr;

                barray = Encoding.ASCII.GetBytes(Nstr);

                SClient.Send(barray);
                //listSendByte.Add(barray.ToString());


                listSendByte.Add(Nstr);

                //barray = Encoding.ASCII.GetBytes(newVal.ToString());
                //SClient.Send(barray);
                //listSendByte.Add(newVal.ToString());

                //for (int i = 0; i < pcmHunter.Length; i++)
                // {
                // //    byte[] dataBytes = ConvertDoubleToByteArray(pcmmux[i]);
                // barray = Encoding.ASCII.GetBytes();
                //  SClient.Send(audioBytes);
                //  listSendByte.Add(audioBytes.ToString());
                //     listSendByte.Add(pcmHunter[i].ToString());
                // }

                // not imparance


                //     listStore.Add(listSendByte.Last());
            }
        }


        private void BtnSend_Click(object sender, EventArgs e)
        {
            byte[] barray = new byte[1024];
            barray = Encoding.ASCII.GetBytes(Txtmsg.Text);
            //listSendByte.Add(Encoding.ASCII.GetString(b, 0, R));audioValueLast
            //barray = Encoding.Unicode.GetBytes(Txtmsg.Text);
            SClient.Send(barray);
        }

        private string threeNumber(string strs)
        {
            int strInteger = 0;
            if (Int32.TryParse(strs, out int numValue))
            {
                Int32.TryParse(strs, out strInteger);
            }
            else
            {
                strs = "0";
            }


            if (strInteger > 999)
            {
                strs = "999";
            }
            if ((strInteger <= 99) & (strInteger >= 10))
            {
                strs = "0" + strs;
            }
            if (strInteger <= 9)
            {
                strs = "00" + strs;
            }
            return strs;
        }

    }
}
