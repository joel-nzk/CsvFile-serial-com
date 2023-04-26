using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;

namespace SerialPortCSVReader
{

    internal class Program
    {
        private static SerialPort comPort = new SerialPort();
        private static string COM = "";
        private static bool firstLine_flag = true;
        private static readonly string signature = "cf";
        private static string filePath = "";

        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Type 'help' to display avalaible commands");
            ReadMode();
            Close();
        }

        private static void ReadMode()
        {
            Console.Write("> ");
            string cmd = Console.ReadLine();
          

            CmdManager(cmd);

        }

        private static void CmdManager(string cmd)
        {
            string[] read_cmd = cmd.Split(' ');

            if(read_cmd.Length == 2)
            {
                if(read_cmd[0] == "write")
                {

                    COM = read_cmd[1].ToUpper();
                    CheckPortValidity();
                }
            }
            else
            {
                switch (cmd.ToLower())
                {
                    case "exit":
                        Close();
                        break;
                    case "write":
                        SelectPort();
                        break;
                    case "help":
                        Console.WriteLine("  exit   -  exit the programm\n  help   -  display all commands\n  write  -  select and write on a serial port\n  write <port name>\n  clear  -  clear the console\n");
                        ReadMode();
                        break;
                    case "clear":
                        Console.Clear();
                        ReadMode();
                        break;
                    default:
                        Console.WriteLine("Unknown command");
                        ReadMode();
                        break;

                }
            }

            
        }

        private static void SelectPort()
        {
            Console.Write(" Write the serial port name : ");
            string port = Console.ReadLine().ToUpper();
            Console.WriteLine();
            COM = port;
            CheckPortValidity();

        }

        private static void CheckPortValidity()
        {
           
            ConnectoToPort();

            if (comPort.IsOpen)
            {
                SelectFile();

                if (filePath.Length == 0)
                {
                    Console.WriteLine(" No file selected\n");
                    ReadMode();
                }


                while (true)
                    ReadFile();
            }
            else
            {
                Console.WriteLine(" This port is not avalaible\n");
                ReadMode();
            }
        }

        private static void Close()
        {
            comPort.Close();
            comPort.Dispose();
        }

        private static void ConnectoToPort()
        {
            Close();

            try
            {
                comPort = new SerialPort(COM, 115200, Parity.None, 8, StopBits.One);
                comPort.Open();
            }catch(Exception ex)
            {
               
            }
            
        }

        private static void SelectFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.InitialDirectory = "c:\\";
            ofd.Multiselect = false;
            ofd.Filter = "txt files (*.csv)|*.csv|All files (*.*)|*.*";
            ofd.ShowDialog();
            filePath = ofd.FileName;
        }

        private static void ReadFile()
        {
            using (var reader = new StreamReader(@$"{filePath}"))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();

                    if (firstLine_flag)
                    {
                        firstLine_flag = false;
                        continue;
                    }

                    line = line.Replace(";", ",");
                    
                    line = $"\x02{signature}{line}\x03";
                    Console.WriteLine(line);


                    while (!comPort.IsOpen)
                    {
                        Thread.Sleep(1000);
                        Console.WriteLine($"Connection to port {COM} lost, reconnecting attempt...");
                        ReconnectComPort();
                    }



                    comPort.Write(line);
                    Thread.Sleep(100);
                }


                Console.WriteLine("---END---");
            }

        }

        private static void ReconnectComPort()
        {
            comPort.Close();
            comPort.Dispose();
            ConnectoToPort();
        }
    }
}
