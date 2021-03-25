using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Threading;

using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Types;

namespace maimaiDX_BT_Test_2
{
    public partial class Form1 : Form
    {
        protected OBSWebsocket _obs;

        String ppap = "";
        bool LEDstatus = false;

        bool LEDLamping = false;
        int BTNCoolTime = 0;

        public Form1()
        {
            InitializeComponent();

            _obs = new OBSWebsocket();

            _obs.Connected += onConnect;
            _obs.Disconnected += onDisconnect;

            _obs.RecordingStateChanged += onRecordingStateChange;
        }



        private void onConnect(object sender, EventArgs e)
        {
            BeginInvoke((MethodInvoker)(() => {

                var streamStatus = _obs.GetStreamingStatus();

                if (streamStatus.IsRecording)
                    onRecordingStateChange(_obs, OutputState.Started);
                else
                    onRecordingStateChange(_obs, OutputState.Stopped);
            }));
        }

        private void onDisconnect(object sender, EventArgs e)
        {
            BeginInvoke((MethodInvoker)(() => {

            }));
        }


        private void onRecordingStateChange(OBSWebsocket sender, OutputState newState)
        {
            string state = "";
            switch (newState)
            {
                case OutputState.Starting:
                    state = "Recording starting...";
                    break;

                case OutputState.Started:
                    state = "Stop recording";
                    break;

                case OutputState.Stopping:
                    state = "Recording stopping...";
                    break;

                case OutputState.Stopped:
                    state = "Start recording";
                    break;

                default:
                    state = "State unknown";
                    break;
            }

            BeginInvoke((MethodInvoker)delegate
            {
                if(newState == OutputState.Stopping || newState == OutputState.Starting)
                {
                    byte[] datas;
                    datas = StringToByte("LED_Ramping_ON\n");
                    serialPort1.Write(datas, 0, datas.Length);
                }
                
                if(state == "Stop recording")
                {
                    byte[] datas;
                    datas = StringToByte("LED_Ramping_OFF\n");
                    serialPort1.Write(datas, 0, datas.Length);

                    datas = null;

                    datas = StringToByte("LED_ON\n");
                    serialPort1.Write(datas, 0, datas.Length);

                    LEDstatus = true;
                }
                if (state == "Start recording")
                {
                    byte[] datas;
                    datas = StringToByte("LED_Ramping_OFF\n");
                    serialPort1.Write(datas, 0, datas.Length);

                    datas = null;

                    datas = StringToByte("LED_OFF\n");
                    serialPort1.Write(datas, 0, datas.Length);

                    LEDstatus = false;
                }
            });
        }



        private void SerialReceiveEvent()
        {
            //textBox2.Text += ppap;
            MessageBox.Show(ppap);
            ppap = "";
        }
        

        private void 연결버튼_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort1.PortName = comboBox1.SelectedItem.ToString();   // 컴포트명
                serialPort1.BaudRate = 9600;   // 보드레이트
                serialPort1.Open();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            try
            {
                _obs.Connect("ws://127.0.0.1:7849", "");
            }
            catch(System.Exception ex) { MessageBox.Show(ex.Message); }

            byte[] datas;
            datas = StringToByte("LED_Ramping_ON\n");
            serialPort1.Write(datas, 0, datas.Length);
        }

        protected override void WndProc(ref Message m)
        {//디바이스 추가/제거시 자동 감지
            UInt32 WM_DEVICECHANGE = 0x0219;
            UInt32 DBT_DEVTUP_VOLUME = 0x02;
            UInt32 DBT_DEVICEARRIVAL = 0x8000;
            UInt32 DBT_DEVICEREMOVECOMPLETE = 0x8004;

            if ((m.Msg == WM_DEVICECHANGE) && (m.WParam.ToInt32() == DBT_DEVICEARRIVAL))//디바이스 연결
            {
                //int m_Count = 0;
                int devType = Marshal.ReadInt32(m.LParam, 4); //파라메타 마샬링

                if (devType == DBT_DEVTUP_VOLUME)
                {
                    GetSerialPort();
                }
            }

            if ((m.Msg == WM_DEVICECHANGE) && (m.WParam.ToInt32() == DBT_DEVICEREMOVECOMPLETE))  //디바이스 연결 해제
            {
                int devType = Marshal.ReadInt32(m.LParam, 4);  //파라메타 마샬링
                if (devType == DBT_DEVTUP_VOLUME)
                {
                    GetSerialPort();
                }
            }

            base.WndProc(ref m);
        }

        private void GetSerialPort()
        {
            comboBox1.Items.Clear();

            try
            {
                foreach (string str in SerialPort.GetPortNames())
                {
                    comboBox1.Items.Add(str);
                }
                if (comboBox1.Items.Count <= 0)
                {
                    comboBox1.Items.Add("연결 장치 없음");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void 시리얼포트_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            byte[] datas = new byte[1000];
            //serialPort1.Read(datas, 0, datas.Length);
            string str = serialPort1.ReadExisting();
            //string str = ByteToString(datas);
            //textBox2.Text += str;

            ppap += str;
        }

        private string ByteToString(byte[] strByte)
        {
            string str = Encoding.Default.GetString(strByte);
            return str;
        }

        private byte[] StringToByte(string str)
        {
            byte[] StrByte = Encoding.UTF8.GetBytes(str);
            return StrByte;
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            GetSerialPort();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            byte[] datas;
            if (!LEDstatus)
            {
                datas = StringToByte("LED_ON\n"); //'1' 전송 뒤에 개행문자 \n을 필수로 붙혀야 함
                LEDstatus = true;
            }
            else
            {
                datas = StringToByte("LED_OFF\n"); //'1' 전송 뒤에 개행문자 \n을 필수로 붙혀야 함
                LEDstatus = false;
            }
            serialPort1.Write(datas, 0, datas.Length);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (ppap.Contains("BT_Read_RECORD\n") && BTNCoolTime == 0)
            {
                _obs.StartStopRecording();
                BTNCoolTime = 5;
                ppap = "";
            }

            else if (BTNCoolTime > 0) BTNCoolTime--;
            
            if (ppap.Contains("\n"))
            {
                ppap = ppap.Replace("\n", "");

                if (ppap.Contains("BT_Read_RECORD") )
                {
                    
                }
                else if (BTNCoolTime > 0) BTNCoolTime--;
                //else MessageBox.Show(ppap);
                ppap = "";
            }
        }

        void LED_Lamp()
        {
            while (LEDLamping)
            {
                if (!LEDLamping) break;

                if (!LEDstatus) { LED_Lamp_Status(true); Thread.Sleep(150); }
                else if (LEDstatus) { LED_Lamp_Status(false); Thread.Sleep(150); }
                //Thread.Sleep(50);
            }
        }

        void LED_Lamp_Status(bool tStatus)
        {
            if (tStatus)
            {
                byte[] datas;
                datas = StringToByte("LED_ON\n");
                serialPort1.Write(datas, 0, datas.Length);
                LEDstatus = true;
            }
            else if (!tStatus)
            {
                byte[] datas;
                datas = StringToByte("LED_OFF\n");
                serialPort1.Write(datas, 0, datas.Length);
                LEDstatus = false;
            }
        }
    }

}
