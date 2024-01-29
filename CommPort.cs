using System;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace UniversaLIS
{
     
     public class CommPort : IPortAdapter
     {
          private readonly SerialPort serialPort = new SerialPort();
          private readonly CommFacilitator facilitator;
          public CommPort(Serial serial, CommFacilitator facilitator)
          {
               serialPort.PortName = serial.Portname!;
               serialPort.BaudRate = serial.Baud;
               serialPort.DataBits = serial.Databits;
               serialPort.Parity = serial.Parity;
               serialPort.StopBits = serial.Stopbits;
               serialPort.Handshake = serial.Handshake;
               serialPort.DataReceived += OnSerialDataReceived;
               serialPort.ReadTimeout = 50;
               this.facilitator = facilitator;
          }
          public void Send(string messageText)
          {
               // AppendToLog($"Out: \t{messageText}");                                //GD 27 11 2023 Its better to log the message after sending it
               byte[] bytes = serialPort.Encoding.GetBytes(messageText);
               for (int i = 0; i < bytes.Length; i++)
               {
                    serialPort.Write(bytes, i, 1);
               }
               AppendToLog($"Out: \t{messageText}");                                  //GD 27 11 2023 Its better to log the message after sending it
        }
          public static readonly EventWaitHandle logOpen = new EventWaitHandle(true, EventResetMode.AutoReset);

          string IPortAdapter.PortName
          {
               get
               {
                    return serialPort.PortName;
               }
          }

          public void AppendToLog(string txt)
          {
               string? publicFolder = Environment.GetEnvironmentVariable("AllUsersProfile");
               var date = DateTime.Now;
               string txtFile = $"{publicFolder}\\UniversaLIS\\Serial_Logs\\SerialLog-{serialPort.PortName}_{date.Year}-{date.Month}-{date.Day}.txt";
               if (!Directory.Exists($"{publicFolder}\\UniversaLIS\\Serial_Logs\\"))
               {
                    Directory.CreateDirectory($"{publicFolder}\\UniversaLIS\\Serial_Logs\\");
               }
               string txtWrite = $"{date.ToLocalTime()} \t{txt}\r\n";
               _ = logOpen.WaitOne();
               File.AppendAllText(txtFile, txtWrite);
               _ = logOpen.Set();
          }

          public event EventHandler? PortDataReceived;

          protected void OnSerialDataReceived(object sender, SerialDataReceivedEventArgs eventArgs)
          {
               EventHandler? handler = PortDataReceived;
               handler?.Invoke(this, eventArgs);
          }
          public int Open()
          // public void Open()                 //GD: 19 11 2023 In case of exception int can return an error
          {
               try                              //GD: 19 11 2023
               {                                //GD: 19 11 2023
                   serialPort.Open();
                AppendToLog("Port: " + serialPort.PortName + " open.");        //GD: 27 11 2023
                Console.WriteLine(" serial COM: port " + serialPort.PortName + " open"); // GD 28 01 2024
                return 0;
               }                                //GD: 19 11 2023
               catch                            //GD: 19 11 2023
               {                                //GD: 19 11 2023
                   Console.WriteLine("Unable to open COM: port " + serialPort.PortName + " does not exit exist or already used");  //GD 19 11 2023
                                                //GD: TODO write a entry in the log file.
                   AppendToLog("Unable to open: " + serialPort.PortName + ".");
                                                //GD: TODO manage this error in the caller and in all heap.
                                                //GD: Perhaps send a throw but here the Open is a void procedure.
                   return -1;                   //GD: 19 11 2023  
               }                                //GD: 19 11 2023 
          }                                     
        public void Close()
          {
               serialPort.Close();
               AppendToLog("Port: " + serialPort.PortName + " closed.");       //GD: 27 11 2023
        }

          string GetCharString()
          {
               char readChar = (char)serialPort.ReadChar();
               return $"{readChar}";
          }

          string IPortAdapter.ReadChars()
          {
               System.Text.StringBuilder buffer = new System.Text.StringBuilder();
               bool timedOut = false;
               try
               {
                    /* There are a few messages that won't end in a NewLine,
                     * so we have to read one character at a time until we run out of them.
                     */
                    do
                    { // Read one char at a time until the ReadChar times out.
                         try
                         {
                              buffer.Append(GetCharString());
                         }
                         catch (Exception)
                         {
                              timedOut = true;
                         }
                    } while (!timedOut);
               }
               catch (Exception ex)
               {
                    facilitator.service.HandleEx(ex);
                    throw;
               }
               return buffer.ToString();
          }
          string IPortAdapter.PortType()
          {
               return "serial";
          }
     }
}