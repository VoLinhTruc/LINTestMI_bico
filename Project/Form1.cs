using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Project
{
	public partial class Form1 : Form
	{
		int Port_Number = 0;     //定义 扫描到的 串口数目 全局变量
		String PortName = "";    //定义 扫描到的 串口号   全局变量

		public static Byte[] Send_Data = new Byte[64];       //定义发送数组
		public static Byte[] Read_Data = new Byte[64];       //定义接收数组 
		public static int RX_count = 0;

		public static int Rur_Mode = 0;                             //定义 上位机运行模式变量 0待机 1单机 2列表机 3从机 4监听 5离线 6BOOT
		public static int Baud_rate = 0;                            //定义 波特率变量

		public static String SI_ID = "";                             //定义  显示ID字符变量
		public static String SI_Dir = "";                            //定义  显示方向字符变量
		public static String SI_Ch = "";                             //定义  显示通道字符变量
		public static String SI_Str = "";                            //定义  显示数据字符变量
		public static String SI_State = "";                          //定义  显示状态字符变量
		public static String SI_Check = "";                          //定义  显示校验和字符变量
		public static int SI_Length = 0;                             //定义  显示数据长度变量

		public static String Channel1_str = "";                      //定义 从机通道1数据变量
		public static String Channel2_str = "";                      //定义 从机通道2数据变量
		public static String Channel3_str = "";                      //定义 从机通道3数据变量

		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{

		}

		private void Form1_Load(object sender, EventArgs e)
		{
			comboBox1.Enabled = true;    //串口号
			comboBox2.Enabled = true;    //波特率
			radioButton1.Enabled = true; //增强
			radioButton2.Enabled = true; //标准

			button1.Enabled = true;      //打开串口
			button2.Enabled = false;      //关闭串口

			button3.Enabled = false;      //单机 发送
			button4.Enabled = true;      //单机 接收

			button5.Enabled = false;      //监听 启动
			button6.Enabled = false;      //监听 停止

			timer1.Enabled = true;         //启用定时器，用于判断是否有新串口设置接入
			timer2.Enabled = false;
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			timer1.Enabled = false;      //关闭定时器 400ms周期

			String[] Getports = System.IO.Ports.SerialPort.GetPortNames();  //必须用命名空间，用SerialPort,获取计算机的有效串口
																			//String port_i = "";        //定义端口临时变量
			Byte timer_i = 0;          //定义 读取串口数目 临时变量
			int timer_ii = 0;          //定义 读取串口数目 临时变量

			for (timer_i = 0; timer_i < Getports.Length; ++timer_i)
			{
				timer_ii = timer_ii + 1;
			}

			if (timer_ii != Port_Number)       //判断 读取到的串口数目 与上一次读取的是否一致
			{
				Application.DoEvents();       //使用DoEvents()函数，使接收数据可以实现进程同步
				comboBox1.Items.Clear();      //每次扫描都清空一次选项
				Port_Number = 0;              //每次扫描都清空一次 串口数目变量

				for (timer_i = 0; timer_i < Getports.Length; ++timer_i)
				{
					comboBox1.Items.Add(Getports[timer_i]);     //向ComboBox1中添加 扫描出来的串口号
					Port_Number = Port_Number + 1;              //串口数目加1
					comboBox1.SelectedIndex = Port_Number - 1;  //选中最后一个扫描出来的串口号
				}
				PortName = comboBox1.Text.ToString();           //读取 被选中的 串口号
			}
			timer1.Enabled = true;       //启用定时器
		}

		private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
		{
			this.Invoke(new EventHandler(delegate
			{
				Sp_Receiving(sender, e); //要委托的代码  调用接收数据函数
				}
				));
		}

		//事件接收函数
		private void Sp_Receiving(object sender, EventArgs e)
		{
			String Rxstr = "";      //定义接收 字符串变量
			int SI_i = 0;

			Array.Clear(Read_Data, 0, Read_Data.Length);   //清空 接收缓冲数组
			RX_count = serialPort1.BytesToRead;            //定义一个变量，并读取串口接收数据大小
			if (RX_count <= 0)
			{
				return;
			}

			serialPort1.Read(Read_Data, 0, RX_count);  //读取数据并存于数组中

			//判断是否接收到完整的帧 
			if (RX_count==16)
			{
				SI_ID = "";                                                   //清0 显示ID字符变量
				SI_Dir = "";                                                  //清0 显示方向字符变量
				SI_Ch = "";                                                   //清0 显示通道字符变量
				SI_Str = "";                                                  //清0 显示数据字符变量
				SI_State = "";                                                //清0 显示状态字符变量
				SI_Check = "";                                                //清0 显示校验和字符变量
				SI_Length = 0;                                                //清0 显示数据长度变量

				if (Rur_Mode == 1)
				{
					if (Read_Data[0]==0x33)
					{
						SI_Ch = Read_Data[1].ToString();                       //通道
						SI_ID = Read_Data[2].ToString("X");                    //读ID
						SI_Dir = "接收";
						if (Read_Data[4] == 0)
						{
							SI_State = "校验和错误";
						}
						else if (Read_Data[4] == 1)
						{
							SI_State = "V1";
						}
						else if (Read_Data[4] == 2)
						{
							SI_State = "V2";
						}
						else
						{
							SI_State = "帧头";
						}
						SI_Length = Read_Data[5];          //读数据长度
						for (SI_i = 6; SI_i < (SI_Length + 6); SI_i++)
						{
							SI_Str = SI_Str + Read_Data[SI_i].ToString("X") + " ";
						}
	 				    SI_Check = Read_Data[14].ToString("X");

						if (SI_State == "V1")
						{
							Rxstr = richTextBox1.Text + "\r\n";
							Rxstr = Rxstr + "ID:" + SI_ID + "    " + SI_Dir + "    " + "数据:" + SI_Str + "    " + "长度:" + SI_Length.ToString() + "    " + "V1";
						}
						else if (SI_State == "V2")
						{
							Rxstr = richTextBox1.Text + "\r\n";
							Rxstr = Rxstr + "ID:" + SI_ID + "    " + SI_Dir + "    " + "数据:" + SI_Str + "    " + "长度:" + SI_Length.ToString() + "    " + "V2";
						}
						else if (SI_State == "校验和错误")
						{
							Rxstr = richTextBox1.Text + "\r\n";
							Rxstr = Rxstr + "ID:" + SI_ID + "    " + SI_Dir + "    " + "数据:" + SI_Str + "    " + "长度:" + SI_Length.ToString() + "    " + "校验和错误";
						}
						else { }
						richTextBox1.Text = Rxstr;                     //显示数据

					}
				}
				else if (Rur_Mode == 3)
				{
					if (Read_Data[0] == 0x44)
					{
						SI_ID = Read_Data[2].ToString("X");     //读ID
						SI_Dir = "接收";
						if (Read_Data[4] == 0)
						{
							SI_State = "校验和错误";
						}
						else if (Read_Data[4] == 1)
						{
							SI_State = "V1";
						}
						else if (Read_Data[4] == 2)
						{
							SI_State = "V2";
						}
						else
						{
							SI_State = "帧头";
						}
						SI_Length = Read_Data[5];          //读数据长度
						for (SI_i = 6; SI_i < (SI_Length + 6); SI_i++)
						{
							SI_Str = SI_Str + Read_Data[SI_i].ToString("X") + " ";
						}
						SI_Check = Read_Data[14].ToString("X");     //校验和值

						if (SI_State == "V1")
						{
							Rxstr = richTextBox2.Text + "\r\n";
							Rxstr = Rxstr + "ID:" + SI_ID + "    " + SI_Dir + "    " + "数据:" + SI_Str + "    " + "长度:" + SI_Length.ToString() + "    " + "V1";
						}
						else if (SI_State == "V2")
						{
							Rxstr = richTextBox2.Text + "\r\n";
							Rxstr = Rxstr + "ID:" + SI_ID + "    " + SI_Dir + "    " + "数据:" + SI_Str + "    " + "长度:" + SI_Length.ToString() + "    " + "V2";
						}
						else if (SI_State == "校验和错误")
						{
							Rxstr = richTextBox2.Text + "\r\n";
							Rxstr = Rxstr + "ID:" + SI_ID + "    " + SI_Dir + "    " + "数据:" + SI_Str + "    " + "长度:" + SI_Length.ToString() + "    " + "校验和错误";
						}
						else
						{
							Rxstr = richTextBox2.Text + "\r\n";
							Rxstr = Rxstr + "ID:" + SI_ID + "    " + SI_Dir + "    " + "数据:"  + "    " + "长度:" + SI_Length.ToString() + "    " + "帧头";
						}
						richTextBox2.Text = Rxstr;                     //显示数据

					}
				}
				else { }

			}
		}

		public static Byte Check_Sum(Byte[] Data, int length)
		{
			int cout_i = 0;
			int Sum = 0;

			for (cout_i = 0; cout_i < length; cout_i++)
			{
				Sum = Sum + Data[cout_i];
			}
			Sum = (((~Sum) & 0x000000FF) + 1);

			return (Byte)Sum;
		}

		public static void Send_Mode_Command(int mode, int Baud_value)
		{
			int i = 0;

			Send_Data[0] = 0x11;                               //模式指令头
			Send_Data[1] = (Byte)mode;                         //模式
			Send_Data[2] = (Byte)((Baud_value & 0xFF00) >> 8); //波特率高8位
			Send_Data[3] = (Byte)(Baud_value & 0x00FF);        //波特率低8位

			for (i = 0; i < 11; i++)
			{
				Send_Data[(i + 4)] = 0;
			}

			Send_Data[15] = Check_Sum(Send_Data, 15);    //计算校验和
		}

		public static void Host_Send_Data(Byte Send_ID, String Send_str, int Length, String check_type)
		{
			int i = 0, si = 0;
			Byte[] Host_DATA = new Byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 };

			Array.Clear(Send_Data, 0, Send_Data.Length);    //清0 数组
			Send_Data[0] = 0x22;                               //主机发送指令头
			Send_Data[1] = 0;                                  //通道0
			Send_Data[2] = Send_ID;                            //ID
			Send_Data[3] = 0;                                  //传输方向0
			if (check_type == "V1")
			{
				Send_Data[4] = 1;                              //标准型校验和
			}
			else
			{
				Send_Data[4] = 2;                              //增强型校验和
			}

			Send_Data[5] = (Byte)Length;                       //数据长度  
			si = 0;
			for (i = 0; i < Length; i++)
			{
				Send_Data[(i + 6)] = (Byte)Convert.ToUInt32((Send_str.Substring(si, 2)).ToString(), 16);
				Host_DATA[i] = Send_Data[(i + 6)];
				si = si + 3;
			}

			Send_Data[15] = Check_Sum(Send_Data, 15);    //计算校验和
		}

		public static void Read_Slave_Data(Byte Read_ID, int Length, String check_type)
		{
			Array.Clear(Send_Data, 0, Send_Data.Length);    //清0 数组
			Send_Data[0] = 0x33;                               //读取从机指令头
			Send_Data[1] = 1;                                  //通道1
			Send_Data[2] = Read_ID;                            //ID
			Send_Data[3] = 1;                                  //传输方向1
			if (check_type == "V1")
			{
				Send_Data[4] = 1;                              //标准型校验和
			}
			else
			{
				Send_Data[4] = 2;                              //增强型校验和
			}
			Send_Data[5] = (Byte)Length;                       //数据长度

			Send_Data[15] = Check_Sum(Send_Data, 15);    //计算校验和
		}


		//打开 串口
		private void button1_Click(object sender, EventArgs e)
		{
			PortName = comboBox1.Text;       //读取 被选中的 串口号
			if (PortName == "")
			{
				//如果，无法打开串口，则提示 警告
				MessageBox.Show("Failed to open the serial port or no available port", "OK", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;   //退出 程序
			}

			if (comboBox2.Text=="")
			{
				MessageBox.Show("Please select any LIN baud rate!!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;   //退出 程序
			}

			serialPort1.PortName = PortName;                  //计算机串口设置 X，是串口号。"COM1"
			serialPort1.BaudRate = 460800;                    //波特率设置
			serialPort1.DataBits = 8;                         //数据位设置
			serialPort1.Parity = System.IO.Ports.Parity.None; //无校验位
			serialPort1.StopBits = System.IO.Ports.StopBits.One;//1位停止位

			serialPort1.Encoding = System.Text.Encoding.Default;               
			serialPort1.DtrEnable = true;                         
			serialPort1.ReadTimeout = 500;                      
			serialPort1.ReceivedBytesThreshold = 16;              
																               
			serialPort1.Open();                               //打开串口
			if (serialPort1.IsOpen == true)
			{
				if (comboBox2.Text=="19200")
				{
					Baud_rate = 19200;
				}
				else if (comboBox2.Text == "9600")
				{
					Baud_rate = 9600;
				}
				else
				{
					Baud_rate = 10400;
				}

				//注：模式之间切换时，需先进入待机模式，再切换其他模式，如，先进入待机再进入主机，或先进入待机再进入监听模式
				Rur_Mode = 0;                                //上位机运行模式变量 0待机 1主机 2从机 3监听 
				Send_Mode_Command(Rur_Mode, Baud_rate);      
				serialPort1.Write(Send_Data, 0, 16);         //发送数据

				timer1.Enabled = false;         //关闭定时器

				comboBox1.Enabled = false;    //串口号
				comboBox2.Enabled = false;    //波特率
				radioButton1.Enabled = false; //增强
				radioButton2.Enabled = false; //标准

				button1.Enabled = false;      //打开串口
				button2.Enabled = true;      //关闭串口

				button3.Enabled = true;      //单机 发送
				button4.Enabled = true;      //单机 接收

				button5.Enabled = true;      //监听 启动
				button6.Enabled = false;      //监听 停止
			}
			else
			{
				MessageBox.Show("Failed to open the serial port or the port is in use", "OK", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		//关闭 串口
		private void button2_Click(object sender, EventArgs e)
		{
			serialPort1.Close();                     //关闭串口
			if (serialPort1.IsOpen == false)          //判断 串口是否已关闭
			{
				comboBox1.Enabled = true;    //串口号
				comboBox2.Enabled = true;    //波特率
				radioButton1.Enabled = true; //增强
				radioButton2.Enabled = true; //标准

				button1.Enabled = true;      //打开串口
				button2.Enabled = false;      //关闭串口

				button3.Enabled = false;      //单机 发送
				button4.Enabled = false;      //单机 接收

				button5.Enabled = false;      //监听 启动
				button6.Enabled = false;      //监听 停止


				timer1.Enabled = true;         //启用定时器，用于判断是否有新串口设置接入
			}
			else
			{
				MessageBox.Show("Failed to close the serial port", "OK", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		//单机模式 主机发送数据
		private void button3_Click(object sender, EventArgs e)
		{
			Byte im3_ID = 0, im3_Length = 0;
			String im3_str = "";
			String display_str = "";

			if (Rur_Mode!=1)    //判断当前是否是主机模式
			{
				if (Rur_Mode != 0)
				{
					Rur_Mode = 0;                                   
					Send_Mode_Command(Rur_Mode, Baud_rate);         //设置为监听0待机 1主机 2从机 3监听 
					serialPort1.Write(Send_Data, 0, 16);            //发送数据 
					System.Threading.Thread.Sleep(100);             //挂起线程指定时间Timing
				}

				Rur_Mode = 1;                                //上位机运行模式变量 0待机 1主机 2从机 3监听 
				Send_Mode_Command(Rur_Mode, Baud_rate);
				serialPort1.Write(Send_Data, 0, 16);         //发送数据
			}

			im3_ID =(Byte) Convert.ToUInt32(textBox1.Text, 16);     //读取ID
			im3_str = textBox2.Text;                                //读取数据
			im3_Length= (Byte)Convert.ToUInt32(textBox3.Text, 16);  //读取长度

			if (radioButton1.Checked == true)
			{
				Host_Send_Data(im3_ID, im3_str, im3_Length, "V2");    //增强型 
				serialPort1.Write(Send_Data, 0, 16);                  //发送数据

				display_str = richTextBox1.Text+ "\r\n";
				display_str = display_str + "ID:"+ im3_ID.ToString("X") + "    " +"发送"+ "    " + "数据:"+im3_str + "    "+"长度:"+ im3_Length.ToString()+"    " + "V2";
				richTextBox1.Text = display_str;                     //显示数据
			}
			else
			{
				Host_Send_Data(im3_ID, im3_str, im3_Length, "V1");    //标准型
				serialPort1.Write(Send_Data, 0, 16);                 //发送数据

				display_str = richTextBox1.Text + "\r\n";
				display_str = display_str + "ID:" + im3_ID.ToString("X") + "    " + "发送" + "    " + "数据:" + im3_str + "    " + "长度:" + im3_Length.ToString() + "    " + "V1";
				richTextBox1.Text = display_str;                     //显示数据
			}

		}

		//单机模式 主机读取从机数据
		private void button4_Click(object sender, EventArgs e)
		{
			Byte im4_ID = 0, im4_Length = 0;

			if (Rur_Mode != 1)    //判断当前是否是主机模式
			{
				if (Rur_Mode != 0)
				{
					Rur_Mode = 0;
					Send_Mode_Command(Rur_Mode, Baud_rate);         //设置为监听0待机 1主机 2从机 3监听 
					serialPort1.Write(Send_Data, 0, 16);            //发送数据 
					System.Threading.Thread.Sleep(100);             //挂起线程指定时间Timing
				}

				Rur_Mode = 1;                                //上位机运行模式变量 0待机 1主机 2从机 3监听 
				Send_Mode_Command(Rur_Mode, Baud_rate);
				serialPort1.Write(Send_Data, 0, 16);         //发送数据
			}

			im4_ID = Convert.ToByte(textBox5.Text,16);
			if (im4_ID > 0x3F)
			{
				MessageBox.Show("ID out of range!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;   //退出 程序
			}

			im4_Length = Convert.ToByte(textBox4.Text);
			if ((im4_Length == 0) || (im4_Length > 8))
			{
				MessageBox.Show("Data length exceeds the range!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;   //退出 程序
			}

			if (radioButton1.Checked == true)
			{
				Read_Slave_Data(im4_ID, im4_Length, "V2");            //增强型
				serialPort1.Write(Send_Data, 0, 16);            //发送数据
			}
			else
			{
				Read_Slave_Data(im4_ID, im4_Length, "V1");            //标准型
				serialPort1.Write(Send_Data, 0, 16);            //发送数据
			}
		}

		//监听模式 启动
		private void button5_Click(object sender, EventArgs e)
		{
			if (Rur_Mode != 3)    //判断当前是否是监听模式
			{
				if (Rur_Mode != 0)
				{
					Rur_Mode = 0;
					Send_Mode_Command(Rur_Mode, Baud_rate);         //设置为  0待机 1主机 2从机 3监听 
					serialPort1.Write(Send_Data, 0, 16);            //发送数据 
					System.Threading.Thread.Sleep(100);             //挂起线程指定时间Timing
				}

				Rur_Mode = 3;                                //上位机运行模式变量 0待机 1主机 2从机 3监听 
				Send_Mode_Command(Rur_Mode, Baud_rate);
				serialPort1.Write(Send_Data, 0, 16);         //发送数据
			}

			button3.Enabled = false;
			button4.Enabled = false;
			button5.Enabled = false;
			button6.Enabled = true;

		}

		//监听模式 停止
		private void button6_Click(object sender, EventArgs e)
		{
			Rur_Mode = 0;
			Send_Mode_Command(Rur_Mode, Baud_rate);         //设置为  0待机 1主机 2从机 3监听 
			serialPort1.Write(Send_Data, 0, 16);            //发送数据 
			System.Threading.Thread.Sleep(100);             //挂起线程指定时间Timing

			button3.Enabled = true;
			button4.Enabled = true;
			button5.Enabled = true;
			button6.Enabled = false;
		}

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {

        }
    }
}
