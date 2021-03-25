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

namespace maimaiDX_BT_Test
{
    public partial class Form1 : Form
    {
        private SerialPort mySerial = new SerialPort();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //comboBox1.DataSource = SerialPort.GetPortNames();
        }

        /*private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!mySerial.IsOpen)  //시리얼포트가 닫혀있을 때만
            {
                //mySerial.PortName = comboBox1.Text;  // 선택된 combobox 의 이름으로 포트명을 지정하자
                mySerial.PortName = "COM5";
                mySerial.BaudRate = 9600;  //아두이노에서 사용할 전송률를 지정하자
                mySerial.DataBits = 8;
                mySerial.StopBits = StopBits.One;
                mySerial.Parity = Parity.None;

                mySerial.Open();  //시리얼포트 열기
            }
            else
            {
                MessageBox.Show("해당포트가 이미 열려 있습니다.");
            }
        }*/

        private void button1_Click(object sender, EventArgs e)
        {
            byte[] datas = StringToByte("1\n"); //'1' 전송 뒤에 개행문자 \n을 필수로 붙혀야 함
            mySerial.Write(datas, 0, datas.Length);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                mySerial.PortName = comboBox1.SelectedItem.ToString();   // 컴포트명
                mySerial.BaudRate = Convert.ToInt32(comboBox1.Text);   // 보드레이트
                mySerial.Open();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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

        private void mySerial_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            byte[] datas = new byte[100];
            mySerial.Read(datas, 0, datas.Length);
            string str = ByteToString(datas);
            Console.WriteLine(str);
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
    }
}