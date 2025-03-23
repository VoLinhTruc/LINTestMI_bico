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
        int Port_Number = 0;     // Define the number of scanned serial ports as a global variable
        String PortName = "";    // Define the scanned serial port number as a global variable

        public static Byte[] Send_Data = new Byte[64];       // Define the send array
        public static Byte[] Read_Data = new Byte[64];       // Define the receive array 
        public static int RX_count = 0;

        public static int Rur_Mode = 0;                             // Define the upper computer running mode variable: 0 standby, 1 single machine, 2 list machine, 3 slave, 4 monitor, 5 offline, 6 BOOT
        public static int Baud_rate = 0;                            // Define the baud rate variable

        public static String SI_ID = "";                             // Define the display ID character variable
        public static String SI_Dir = "";                            // Define the display direction character variable
        public static String SI_Ch = "";                             // Define the display channel character variable
        public static String SI_Str = "";                            // Define the display data character variable
        public static String SI_State = "";                          // Define the display status character variable
        public static String SI_Check = "";                          // Define the display checksum character variable
        public static int SI_Length = 0;                             // Define the display data length variable

        public static String Channel1_str = "";                      // Define the slave channel 1 data variable
        public static String Channel2_str = "";                      // Define the slave channel 2 data variable
        public static String Channel3_str = "";                      // Define the slave channel 3 data variable

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.Enabled = true;    // Serial port number
            comboBox2.Enabled = true;    // Baud rate
            radioButton1.Enabled = true; // Enhanced
            radioButton2.Enabled = true; // Standard

            button1.Enabled = true;      // Open serial port
            button2.Enabled = false;     // Close serial port

            button3.Enabled = false;     // Single machine send
            button4.Enabled = true;      // Single machine receive

            button5.Enabled = false;     // Monitor start
            button6.Enabled = false;     // Monitor stop

            timer1.Enabled = true;       // Enable timer to check if new serial port settings are connected
            timer2.Enabled = false;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;      // Disable timer with 400ms period

            String[] Getports = System.IO.Ports.SerialPort.GetPortNames();  // Use SerialPort to get the valid serial ports of the computer
            Byte timer_i = 0;          // Define a temporary variable to read the number of serial ports
            int timer_ii = 0;          // Define a temporary variable to read the number of serial ports

            for (timer_i = 0; timer_i < Getports.Length; ++timer_i)
            {
                timer_ii = timer_ii + 1;
            }

            if (timer_ii != Port_Number)       // Check if the number of read serial ports is consistent with the last read
            {
                Application.DoEvents();       // Use DoEvents() function to achieve process synchronization for receiving data
                comboBox1.Items.Clear();      // Clear the options each time scanning
                Port_Number = 0;              // Clear the serial port number variable each time scanning

                for (timer_i = 0; timer_i < Getports.Length; ++timer_i)
                {
                    comboBox1.Items.Add(Getports[timer_i]);     // Add the scanned serial port number to ComboBox1
                    Port_Number = Port_Number + 1;              // Increment the serial port number
                    comboBox1.SelectedIndex = Port_Number - 1;  // Select the last scanned serial port number
                }
                PortName = comboBox1.Text.ToString();           // Read the selected serial port number
            }
            timer1.Enabled = true;       // Enable timer
        }

        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            this.Invoke(new EventHandler(delegate
            {
                Sp_Receiving(sender, e); // Delegate the code to call the receive data function
            }
                ));
        }

        // Event receive function
        private void Sp_Receiving(object sender, EventArgs e)
        {
            String Rxstr = "";      // Define the receive string variable
            int SI_i = 0;

            Array.Clear(Read_Data, 0, Read_Data.Length);   // Clear the receive buffer array
            RX_count = serialPort1.BytesToRead;            // Define a variable and read the size of the serial port receive data
            if (RX_count <= 0)
            {
                return;
            }

            serialPort1.Read(Read_Data, 0, RX_count);  // Read data and store it in the array

            // Check if a complete frame is received
            if (RX_count == 16)
            {
                SI_ID = "";                                                   // Clear the display ID character variable
                SI_Dir = "";                                                  // Clear the display direction character variable
                SI_Ch = "";                                                   // Clear the display channel character variable
                SI_Str = "";                                                  // Clear the display data character variable
                SI_State = "";                                                // Clear the display status character variable
                SI_Check = "";                                                // Clear the display checksum character variable
                SI_Length = 0;                                                // Clear the display data length variable

                if (Rur_Mode == 1)
                {
                    if (Read_Data[0] == 0x33)
                    {
                        SI_Ch = Read_Data[1].ToString();                       // Channel
                        SI_ID = Read_Data[2].ToString("X");                    // Read ID
                        SI_Dir = "Receive";
                        if (Read_Data[4] == 0)
                        {
                            SI_State = "Checksum error";
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
                            SI_State = "Frame header";
                        }
                        SI_Length = Read_Data[5];          // Read data length
                        for (SI_i = 6; SI_i < (SI_Length + 6); SI_i++)
                        {
                            SI_Str = SI_Str + Read_Data[SI_i].ToString("X") + " ";
                        }
                        SI_Check = Read_Data[14].ToString("X");

                        if (SI_State == "V1")
                        {
                            Rxstr = richTextBox1.Text + "\r\n";
                            Rxstr = Rxstr + "ID:" + SI_ID + "    " + SI_Dir + "    " + "Data:" + SI_Str + "    " + "Length:" + SI_Length.ToString() + "    " + "V1";
                        }
                        else if (SI_State == "V2")
                        {
                            Rxstr = richTextBox1.Text + "\r\n";
                            Rxstr = Rxstr + "ID:" + SI_ID + "    " + SI_Dir + "    " + "Data:" + SI_Str + "    " + "Length:" + SI_Length.ToString() + "    " + "V2";
                        }
                        else if (SI_State == "Checksum error")
                        {
                            Rxstr = richTextBox1.Text + "\r\n";
                            Rxstr = Rxstr + "ID:" + SI_ID + "    " + SI_Dir + "    " + "Data:" + SI_Str + "    " + "Length:" + SI_Length.ToString() + "    " + "Checksum error";
                        }
                        else { }
                        richTextBox1.Text = Rxstr;                     // Display data

                    }
                }
                else if (Rur_Mode == 3)
                {
                    if (Read_Data[0] == 0x44)
                    {
                        SI_ID = Read_Data[2].ToString("X");     // Read ID
                        SI_Dir = "Receive";
                        if (Read_Data[4] == 0)
                        {
                            SI_State = "Checksum error";
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
                            SI_State = "Frame header";
                        }
                        SI_Length = Read_Data[5];          // Read data length
                        for (SI_i = 6; SI_i < (SI_Length + 6); SI_i++)
                        {
                            SI_Str = SI_Str + Read_Data[SI_i].ToString("X") + " ";
                        }
                        SI_Check = Read_Data[14].ToString("X");     // Checksum value

                        if (SI_State == "V1")
                        {
                            Rxstr = richTextBox2.Text + "\r\n";
                            Rxstr = Rxstr + "ID:" + SI_ID + "    " + SI_Dir + "    " + "Data:" + SI_Str + "    " + "Length:" + SI_Length.ToString() + "    " + "V1";
                        }
                        else if (SI_State == "V2")
                        {
                            Rxstr = richTextBox2.Text + "\r\n";
                            Rxstr = Rxstr + "ID:" + SI_ID + "    " + SI_Dir + "    " + "Data:" + SI_Str + "    " + "Length:" + SI_Length.ToString() + "    " + "V2";
                        }
                        else if (SI_State == "Checksum error")
                        {
                            Rxstr = richTextBox2.Text + "\r\n";
                            Rxstr = Rxstr + "ID:" + SI_ID + "    " + SI_Dir + "    " + "Data:" + SI_Str + "    " + "Length:" + SI_Length.ToString() + "    " + "Checksum error";
                        }
                        else
                        {
                            Rxstr = richTextBox2.Text + "\r\n";
                            Rxstr = Rxstr + "ID:" + SI_ID + "    " + SI_Dir + "    " + "Data:" + "    " + "Length:" + SI_Length.ToString() + "    " + "Frame header";
                        }
                        richTextBox2.Text = Rxstr;                     // Display data

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

            Send_Data[0] = 0x11;                               // Mode command header
            Send_Data[1] = (Byte)mode;                         // Mode
            Send_Data[2] = (Byte)((Baud_value & 0xFF00) >> 8); // Baud rate high 8 bits
            Send_Data[3] = (Byte)(Baud_value & 0x00FF);        // Baud rate low 8 bits

            for (i = 0; i < 11; i++)
            {
                Send_Data[(i + 4)] = 0;
            }

            Send_Data[15] = Check_Sum(Send_Data, 15);    // Calculate checksum
        }

        public static void Host_Send_Data(Byte Send_ID, String Send_str, int Length, String check_type)
        {
            int i = 0, si = 0;
            Byte[] Host_DATA = new Byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 };

            Array.Clear(Send_Data, 0, Send_Data.Length);    // Clear array
            Send_Data[0] = 0x22;                               // Host send command header
            Send_Data[1] = 0;                                  // Channel 0
            Send_Data[2] = Send_ID;                            // ID
            Send_Data[3] = 0;                                  // Transmission direction 0
            if (check_type == "V1")
            {
                Send_Data[4] = 1;                              // Standard checksum
            }
            else
            {
                Send_Data[4] = 2;                              // Enhanced checksum
            }

            Send_Data[5] = (Byte)Length;                       // Data length  
            si = 0;
            for (i = 0; i < Length; i++)
            {
                Send_Data[(i + 6)] = (Byte)Convert.ToUInt32((Send_str.Substring(si, 2)).ToString(), 16);
                Host_DATA[i] = Send_Data[(i + 6)];
                si = si + 3;
            }

            Send_Data[15] = Check_Sum(Send_Data, 15);    // Calculate checksum
        }

        public static void Read_Slave_Data(Byte Read_ID, int Length, String check_type)
        {
            Array.Clear(Send_Data, 0, Send_Data.Length);    // Clear array
            Send_Data[0] = 0x33;                               // Read slave command header
            Send_Data[1] = 1;                                  // Channel 1
            Send_Data[2] = Read_ID;                            // ID
            Send_Data[3] = 1;                                  // Transmission direction 1
            if (check_type == "V1")
            {
                Send_Data[4] = 1;                              // Standard checksum
            }
            else
            {
                Send_Data[4] = 2;                              // Enhanced checksum
            }
            Send_Data[5] = (Byte)Length;                       // Data length

            Send_Data[15] = Check_Sum(Send_Data, 15);    // Calculate checksum
        }


        // Open serial port
        private void button1_Click(object sender, EventArgs e)
        {
            PortName = comboBox1.Text;       // Read the selected serial port number
            if (PortName == "")
            {
                // If the serial port cannot be opened, prompt a warning
                MessageBox.Show("Failed to open the serial port or no available port", "OK", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;   // Exit the program
            }

            if (comboBox2.Text == "")
            {
                MessageBox.Show("Please select any LIN baud rate!!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;   // Exit the program
            }

            serialPort1.PortName = PortName;                  // Set the computer serial port X, which is the serial port number "COM1"
            serialPort1.BaudRate = 460800;                    // Set the baud rate
            serialPort1.DataBits = 8;                         // Set the data bits
            serialPort1.Parity = System.IO.Ports.Parity.None; // No parity bit
            serialPort1.StopBits = System.IO.Ports.StopBits.One; // 1 stop bit

            serialPort1.Encoding = System.Text.Encoding.Default;
            serialPort1.DtrEnable = true;
            serialPort1.ReadTimeout = 500;
            serialPort1.ReceivedBytesThreshold = 16;

            serialPort1.Open();                               // Open the serial port
            if (serialPort1.IsOpen == true)
            {
                if (comboBox2.Text == "19200")
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

                // Note: When switching between modes, you need to enter standby mode first, and then switch to other modes, such as entering standby first and then entering the host, or entering standby first and then entering the monitor mode
                Rur_Mode = 0;                                // Upper computer running mode variable: 0 standby, 1 host, 2 slave, 3 monitor 
                Send_Mode_Command(Rur_Mode, Baud_rate);
                serialPort1.Write(Send_Data, 0, 16);         // Send data

                timer1.Enabled = false;         // Disable timer

                comboBox1.Enabled = false;    // Serial port number
                comboBox2.Enabled = false;    // Baud rate
                radioButton1.Enabled = false; // Enhanced
                radioButton2.Enabled = false; // Standard

                button1.Enabled = false;      // Open serial port
                button2.Enabled = true;       // Close serial port

                button3.Enabled = true;       // Single machine send
                button4.Enabled = true;       // Single machine receive

                button5.Enabled = true;       // Monitor start
                button6.Enabled = false;      // Monitor stop
            }
            else
            {
                MessageBox.Show("Failed to open the serial port or the port is in use", "OK", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Close serial port
        private void button2_Click(object sender, EventArgs e)
        {
            serialPort1.Close();                     // Close the serial port
            if (serialPort1.IsOpen == false)          // Check if the serial port is closed
            {
                comboBox1.Enabled = true;    // Serial port number
                comboBox2.Enabled = true;    // Baud rate
                radioButton1.Enabled = true; // Enhanced
                radioButton2.Enabled = true; // Standard

                button1.Enabled = true;      // Open serial port
                button2.Enabled = false;     // Close serial port

                button3.Enabled = false;     // Single machine send
                button4.Enabled = false;     // Single machine receive

                button5.Enabled = false;     // Monitor start
                button6.Enabled = false;     // Monitor stop

                timer1.Enabled = true;       // Enable timer to check if new serial port settings are connected
            }
            else
            {
                MessageBox.Show("Failed to close the serial port", "OK", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Single machine mode: Host sends data
        private void button3_Click(object sender, EventArgs e)
        {
            Byte im3_ID = 0, im3_Length = 0;
            String im3_str = "";
            String display_str = "";

            if (Rur_Mode != 1)    // Check if the current mode is host mode
            {
                if (Rur_Mode != 0)
                {
                    Rur_Mode = 0;
                    Send_Mode_Command(Rur_Mode, Baud_rate);         // Set to standby mode: 0 standby, 1 host, 2 slave, 3 monitor 
                    serialPort1.Write(Send_Data, 0, 16);            // Send data 
                    System.Threading.Thread.Sleep(100);             // Suspend the thread for a specified time
                }

                Rur_Mode = 1;                                // Upper computer running mode variable: 0 standby, 1 host, 2 slave, 3 monitor 
                Send_Mode_Command(Rur_Mode, Baud_rate);
                serialPort1.Write(Send_Data, 0, 16);         // Send data
            }

            im3_ID = (Byte)Convert.ToUInt32(textBox1.Text, 16);     // Read ID
            im3_str = textBox2.Text;                                // Read data
            im3_Length = (Byte)Convert.ToUInt32(textBox3.Text, 16); // Read length

            if (radioButton1.Checked == true)
            {
                Host_Send_Data(im3_ID, im3_str, im3_Length, "V2");    // Enhanced 
                serialPort1.Write(Send_Data, 0, 16);                  // Send data

                display_str = richTextBox1.Text + "\r\n";
                display_str = display_str + "ID:" + im3_ID.ToString("X") + "    " + "Send" + "    " + "Data:" + im3_str + "    " + "Length:" + im3_Length.ToString() + "    " + "V2";
                richTextBox1.Text = display_str;                     // Display data
            }
            else
            {
                Host_Send_Data(im3_ID, im3_str, im3_Length, "V1");    // Standard
                serialPort1.Write(Send_Data, 0, 16);                 // Send data

                display_str = richTextBox1.Text + "\r\n";
                display_str = display_str + "ID:" + im3_ID.ToString("X") + "    " + "Send" + "    " + "Data:" + im3_str + "    " + "Length:" + im3_Length.ToString() + "    " + "V1";
                richTextBox1.Text = display_str;                     // Display data
            }

        }

        // Single machine mode: Host reads slave data
        private void button4_Click(object sender, EventArgs e)
        {
            Byte im4_ID = 0, im4_Length = 0;

            if (Rur_Mode != 1)    // Check if the current mode is host mode
            {
                if (Rur_Mode != 0)
                {
                    Rur_Mode = 0;
                    Send_Mode_Command(Rur_Mode, Baud_rate);         // Set to standby mode: 0 standby, 1 host, 2 slave, 3 monitor 
                    serialPort1.Write(Send_Data, 0, 16);            // Send data 
                    System.Threading.Thread.Sleep(100);             // Suspend the thread for a specified time
                }

                Rur_Mode = 1;                                // Upper computer running mode variable: 0 standby, 1 host, 2 slave, 3 monitor 
                Send_Mode_Command(Rur_Mode, Baud_rate);
                serialPort1.Write(Send_Data, 0, 16);         // Send data
            }

            im4_ID = Convert.ToByte(textBox5.Text, 16);
            if (im4_ID > 0x3F)
            {
                MessageBox.Show("ID out of range!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;   // Exit the program
            }

            im4_Length = Convert.ToByte(textBox4.Text);
            if ((im4_Length == 0) || (im4_Length > 8))
            {
                MessageBox.Show("Data length exceeds the range!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;   // Exit the program
            }

            if (radioButton1.Checked == true)
            {
                Read_Slave_Data(im4_ID, im4_Length, "V2");            // Enhanced
                serialPort1.Write(Send_Data, 0, 16);                  // Send data
            }
            else
            {
                Read_Slave_Data(im4_ID, im4_Length, "V1");            // Standard
                serialPort1.Write(Send_Data, 0, 16);                  // Send data
            }
        }

        // Monitor mode: Start
        private void button5_Click(object sender, EventArgs e)
        {
            if (Rur_Mode != 3)    // Check if the current mode is monitor mode
            {
                if (Rur_Mode != 0)
                {
                    Rur_Mode = 0;
                    Send_Mode_Command(Rur_Mode, Baud_rate);         // Set to standby mode: 0 standby, 1 host, 2 slave, 3 monitor 
                    serialPort1.Write(Send_Data, 0, 16);            // Send data 
                    System.Threading.Thread.Sleep(100);             // Suspend the thread for a specified time
                }

                Rur_Mode = 3;                                // Upper computer running mode variable: 0 standby, 1 host, 2 slave, 3 monitor 
                Send_Mode_Command(Rur_Mode, Baud_rate);
                serialPort1.Write(Send_Data, 0, 16);         // Send data
            }

            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            button6.Enabled = true;

        }

        // Monitor mode: Stop
        private void button6_Click(object sender, EventArgs e)
        {
            Rur_Mode = 0;
            Send_Mode_Command(Rur_Mode, Baud_rate);         // Set to standby mode: 0 standby, 1 host, 2 slave, 3 monitor 
            serialPort1.Write(Send_Data, 0, 16);            // Send data 
            System.Threading.Thread.Sleep(100);             // Suspend the thread for a specified time

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

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}