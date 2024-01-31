# MODIFICATION AFTER DOWNLAODING THE PROJET FROM GITHUB
=========================================================
 ## 19 11 2023
--------------
  ### GD

>1. CommFacilitator.cs public CommFacilitator(Serial serialSettings, UniversaLIService LIService) 
and
   CommFacilitator.cs public CommFacilitator(Tcp tcpSettings, UniversaLIService LIService)
I added a error handler to manage incase of no port COM or if it is already opened
for CommFacilitator.cs public CommFacilitator(Serial serialSettings, UniversaLIService LIService)
I replace 
    public CommFacilitator(Serial serialSettings, UniversaLIService LIService)
                    ...
                    ComPort.Open();                         
                    ...
                    idleTimer.Start();
                }
                                }
               catch (Exception ex)
               ...
by
                    if (ComPort.Open() == 0)                        //GD: 19 11 2023 
                    {                                               //GD: 19 11 2023
                        ...
                        idleTimer.Start();
                    }                                               //GD: 19 11 2023
                    else                                            //GD: 19 11 2023
                    {                                               //GD: 19 11 2023
                    AppendToLog("Unable to open: " + serialSettings.Portname + ".");
                    }                                               //GD: 19 11 2023
                }
               catch (Exception ex)
               ...
for CommFacilitator.cs public CommFacilitator(Tcp tcpSettings, UniversaLIService LIService)
I replace
          public CommFacilitator(Tcp tcpSettings, UniversaLIService LIService)
          {
                    ...
                    ComPort.Open();                               
                    AppendToLog($"Socket opened: {tcpSettings.Socket}");
                    idleTimer.AutoReset = true;
                    idleTimer.Elapsed += new System.Timers.ElapsedEventHandler(IdleTime);
                    if (tcpSettings.AutoSendOrders > 0)
                    {
                         idleTimer.Elapsed += WorklistTimedEvent;
                    }
                    idleTimer.Start();
               }
               catch (Exception ex)
               ...
          }
by
          public CommFacilitator(Tcp tcpSettings, UniversaLIService LIService)
          {
                    ...
                    //ComPort.Open();                              //GD: 19 11 2023
                    if (ComPort.Open() == 0)                       //GD: 19 11 2023
                    {                                              //GD: 19 11 2023
                        AppendToLog($"Socket opened: {tcpSettings.Socket}");
                        idleTimer.AutoReset = true;
                        idleTimer.Elapsed += new System.Timers.ElapsedEventHandler(IdleTime);
                        if (tcpSettings.AutoSendOrders > 0)
                        {
                            idleTimer.Elapsed += WorklistTimedEvent;
                        }
                        idleTimer.Start();
                    }    
               }
               catch (Exception ex)
               ...
         }
------------------------------------
>2. CommPort.cs public void Open()
I modified the type of return of this method. In case of no port COM present in the Pc or already opened the system doesn't see the issue.
I replace public void Open() by public int Open(). 
And I manage the error inside the function.
According to this modification i had to modify the same for TCP. See Below
I replace
          public void Open()                 
          {
               serialPort.Open();
          }   
by 
          public int Open()
          {
               try                              //GD: 19 11 2023
               {                                //GD: 19 11 2023
                   serialPort.Open();
                return 0;
               }                                //GD: 19 11 2023
               catch                            //GD: 19 11 2023
               {                                //GD: 19 11 2023
                   Console.WriteLine("Unable to open COM: port " + serialPort.PortName + " does not exit exist or already used");  //GD 19 11 2023
                                                //GD: TODO write a entry in the log file.
                   AppendToLog("Unable to open: " + serialPort.PortName + ".");
                                                //GD: TODO manage this error in the caller and in all heap.
                                                //GD: Perhaps send a throw but here the Open is a void procedure.
                   return -1;                   //GD 19 11 2023  
               }                                //GD 19 11 2023 
          }   
Due this modification, TcpPort.cs needs a modification
          void IPortAdapter.Open()
          {
               ...
               portTimer.Start();
         }
by
          int IPortAdapter.Open()
          // void IPortAdapter.Open()                         //GD: 19 11 2023
          {
               ...
               portTimer.Start();
               return 0;                                      //GD: 19 11 2023
Due this modification, IPortAdapter.cs needs the same
I replace
          public void Open();                               //GD: 19 11 2023
by
          public int Open();
------------------------------------

>3. I modified RcvWaitState.cs private bool CheckChecksum(string InputString)
 Perhaps because windows is in french, it doesn't work on my PC.
 I cannot found  the character <ETX> "/x03" ou "\u0003" then I use the function string.IndexOf overloaded
 I replace
 private bool CheckChecksum(string InputString)
          { ...
               int position = message.IndexOf(Constants.ETX);
                ...
          }
by
              int position = message.IndexOf(Constants.ETX, System.StringComparison.Ordinal);
------------------------------------
>4 in RcvWaitState.cs  I accepted the password verification in case is null or empty=''
I replace
          private bool CheckPassword(string inputString)
          {
               if (inputString.Substring(0, 3) == $"{Constants.STX}1H")
               {
                   ...
                    //TODO Thre is no reading of the entry config.yml
                    String[] fieldArray = inputString.Split('|');
                    if (fieldArray[3] != comm.password)
                    {
                        return false;
                    }
                    ...
by
          private bool CheckPassword(string inputString)
          {
                   ...
                    //TODO Thre is no reading of the entry config.yml
                    String[] fieldArray = inputString.Split('|');
                    if (fieldArray[3] == "" && comm.password == null)               //GD: 29 11 2023
                    {                                                               //GD: 29 11 2023
                    return true;                                                    //GD: 29 11 2023
                    }                                                               //GD: 29 11 2023
                    if (fieldArray[3] != comm.password)
                    {
                        return false;
                    }
                    ...
------------------------------
>5 in CommPort.cs I move the append to the log after sending the message in the port COM 
I replace
          public void Send(string messageText)
          {
               AppendToLog($"Out: \t{messageText}"); 
               byte[] bytes = serialPort.Encoding.GetBytes(messageText);
               for (int i = 0; i < bytes.Length; i++)
               {
                    serialPort.Write(bytes, i, 1);
               }
        }
by 
          public void Send(string messageText)
          {
               byte[] bytes = serialPort.Encoding.GetBytes(messageText);
               for (int i = 0; i < bytes.Length; i++)
               {
                    serialPort.Write(bytes, i, 1);
               }
               AppendToLog($"Out: \t{messageText}");                                  //GD 27 11 2023 Its better to log the message after sending it
        }
------------------------------
>6 In CommPort.cs i add an log when the serial port is open or closed
I replace
          public int Open()
          // public void Open()                 
          {
               try                              
               {                                
                   serialPort.Open();
                return 0;
               } 
...
by 
          public int Open()
          // public void Open()                 
          {
               try                              
               {                                
                   serialPort.Open();
                AppendToLog("Port: " + serialPort.PortName + " opened.");        //GD: 27 11 2023
                return 0;
               } 
...
and
I replace
        public void Close()
          {
               serialPort.Close();
        }
by
        public void Close()
          {
               serialPort.Close();
               AppendToLog("Port: " + serialPort.PortName + " closed.");       //GD: 27 11 2023
        }
------------------------------
>7 
It missed the class comment.cs. In ASTM the comment are associated with the Result frame or the Order frame.
------------------------------
>8 In Constants.cs 
I added the code used in HL7
namespace UniversaLIS
{
     public static class Constants
     {
          public const string VT = "\x0B";                           //GD 28 01 2024  Used in HL7
          public const string FS = "\x1C";                           //GD 28 01 2024  Used in HL7
    }
}

------------------------------
>9 In CommFacilitator.cs 
Because the in the service log I cannot see the <CR>, <ACK> etc then i replace those ascii by their texts. the <CR> was a true carriage return and not a data to debug the frame
example
28/01/2024 16:55:14 	In: 	
28/01/2024 16:55:16 	In: 	1H|\^&|||MHR1^001^1.0.0|||||||P|LIS2-A2|20170323162715
D9
CR 
28/01/2024 16:55:17 	In: 	2P|1|||||||||||||||||||||||||||||||||||
33
 CR
28/01/2024 16:55:18 	In: 	3O|1|NUMSID^01^006947^7||^^^DIF||||||||||||||||AP^STANDARD(m)|||||F|||||
38
CR
28/01/2024 16:55:19 	In: 	4M|1|QC|EXTQC\TIME2QC\WESTGARD\XB\XM|INDETERMINED\INDETERMINED\INDETERMINED\INDETERMINED\INDETERMINED
98
CR 
28/01/2024 16:55:20 	In: 	5M|2|SETTING|RUO\5DIFF|TRUE\FALSE
79
became
28/01/2024 17:54:03 	In: 	<ENQ>
28/01/2024 17:54:04 	In: 	<STX>1H|\^&|||MHR1^001^1.0.0|||||||P|LIS2-A2|20170323162715<CR ><ETX>D9<CR ><LF>
28/01/2024 17:54:06 	In: 	<STX>2P|1|||||||||||||||||||||||||||||||||||<CR ><ETX>33<CR ><LF>
28/01/2024 17:54:07 	In: 	<STX>3O|1|NUMSID^01^006947^7||^^^DIF||||||||||||||||AP^STANDARD(m)|||||F|||||<CR ><ETX>38<CR ><LF>
28/01/2024 17:54:08 	In: 	<STX>4M|1|QC|EXTQC\TIME2QC\WESTGARD\XB\XM|INDETERMINED\INDETERMINED\INDETERMINED\INDETERMINED\INDETERMINED<CR ><ETX>98<CR ><LF>
28/01/2024 17:54:09 	In: 	<STX>5M|2|SETTING|RUO\5DIFF|TRUE\FALSE<CR ><ETX>79<CR ><LF>
I added in file CommFacilitator. Perhpas it's not the best place. Because when it sends message it's from an another class.
          public string AsciiToText(string txt)
            // when It recieve data with ascii below 31. It's not possible to see the character. then I transform those character
            //GD 28 01 2024
          {
               string output = txt.Replace(Constants.ACK, "<ACK>");
               output = output.Replace(Constants.CR, "<CR>");
               output = output.Replace(Constants.ENQ, "<ENQ>");
               output = output.Replace(Constants.EOT, "<EOT>");
               output = output.Replace(Constants.ETB, "<ETB>");
               output = output.Replace(Constants.ETX, "<ETX>");
               output = output.Replace(Constants.LF, "<LF>");
               output = output.Replace(Constants.NAK, "<NAK>");
               output = output.Replace(Constants.STX, "<STX>");
               output = output.Replace(Constants.VT, "<VT>");
               output = output.Replace(Constants.FS, "<FS>");
               return output; 
          }
and I replace
     public class CommFacilitator
     {
          void CommPortDataReceived(object sender, EventArgs e)
          {
           ...
                    UniversaLIService.AppendToLog($"In: \t{buffer}"); 
           ...
          }
      ...
      }
by
     public class CommFacilitator
     {
          void CommPortDataReceived(object sender, EventArgs e)
          {
           ...
                    UniversaLIService.AppendToLog($"In: \t{AsciiToText(buffer.ToString())}");      //GD 28 01 2024
           ...
          }
      ...
      }
------------------------------
>10 the service log file doesn't show the output only the input
In IdleState.cs 
I replace
          public void RcvENQ()
          {
               comm.Send(Constants.ACK);
          }              
by
          public void RcvENQ()
          {
               comm.Send(Constants.ACK);
               UniversaLIService.AppendToLog($"out: \t<ACK>");     //GD 28 01 2024
          }              
And
          public void HaveData()
          {
               // If there's data to send, check the timers before sending.
               if (comm.ContentTimer.RemainingDuration <= 0 && comm.BusyTimer.RemainingDuration <= 0)
               {
                    // Send ENQ
                    comm.Send(Constants.ENQ);
                    ...
                }
                ...
          }
by
          public void HaveData()
          {
               // If there's data to send, check the timers before sending.
               if (comm.ContentTimer.RemainingDuration <= 0 && comm.BusyTimer.RemainingDuration <= 0)
               {
                    // Send ENQ
                    comm.Send(Constants.ENQ);
                    UniversaLIService.AppendToLog($"out: \t<ENQ>");     //GD 28 01 2024
                    ...
                }
                ...
          }
in LISCommState.cs
I replace
     public void TransTimeout()
          {
               if (CommState is TransEnqState || CommState is TransWaitState)
               {
                    // Send EOT and return to idle state.
                    comm.Send(Constants.EOT);
                    ...
               }
          }
by
     public void TransTimeout()
          {
               if (CommState is TransEnqState || CommState is TransWaitState)
               {
                    // Send EOT and return to idle state.
                    comm.Send(Constants.EOT);
                    UniversaLIService.AppendToLog($"out: \t<EOT>");     //GD 28 01 2024
                    ...
               }
          }
in RcvWaitState.cs
I replace
          public void RcvData(string InputString)
          {
               ...
               if (isFrameGood)
               {
                    // Send ACK 
                    comm.Send(Constants.ACK);
                // Reset rcvTimer to 30 seconds.
                comm.RcvTimer.Reset(30);
                    // Increment frame number.
                    ExpectedFrame = ++ExpectedFrame % 8;
                    // Actually handle the frame.
                    comm.ParseMessageLine(InputString);
               }
               else
               {
                    // Send NAK
                    comm.Send(Constants.NAK);
                    // Reset rcvTimer to 30 seconds.
                comm.RcvTimer.Reset(30);
               }
          ...
          }
by 
          public void RcvData(string InputString)
          {
               ...
               if (isFrameGood)
               {
                    // Send ACK 
                    comm.Send(Constants.ACK);
                    UniversaLIService.AppendToLog($"out: \t<ACK>");     //GD 28 01 2024
                // Reset rcvTimer to 30 seconds.
                comm.RcvTimer.Reset(30);
                    // Increment frame number.
                    ExpectedFrame = ++ExpectedFrame % 8;
                    // Actually handle the frame.
                    comm.ParseMessageLine(InputString);
               }
               else
               {
                    // Send NAK
                    comm.Send(Constants.NAK);
                UniversaLIService.AppendToLog($"out: \t<NAK>");     //GD 28 01 2024
                // Reset rcvTimer to 30 seconds.
                comm.RcvTimer.Reset(30);
               }
          ...
          }
in TransENQState.cs
It misses the transformation of the character below 31 to text. that's where the point 9 shouldn't be in CommFacilitator.cs
I replace
          public void RcvACK()
          {
               // Send next frame.
               comm.CurrentMessage = comm.OutboundMessageQueue.Dequeue();
               comm.CurrentMessage.PrepareToSend();
               comm.Send(comm.CurrentMessage.FrameList[comm.CurrentFrameCounter]);
               comm.CurrentFrameCounter++;
               // Reset the NAK count to 0.
               comm.NumNAK = 0;
               // Reset the transaction timer to 15 seconds.
               comm.TransTimer.Reset(15);
          }
by
          public void RcvACK()
          {
               // Send next frame.
               comm.CurrentMessage = comm.OutboundMessageQueue.Dequeue();
               comm.CurrentMessage.PrepareToSend();
               comm.Send(comm.CurrentMessage.FrameList[comm.CurrentFrameCounter]);
               UniversaLIService.AppendToLog($"out: \t" + comm.CurrentMessage.FrameList[comm.CurrentFrameCounter]);     //GD 28 01 2024
               comm.CurrentFrameCounter++;
               // Reset the NAK count to 0.
               comm.NumNAK = 0;
               // Reset the transaction timer to 15 seconds.
               comm.TransTimer.Reset(15);
          }
in TransWaitState.cs
It misses the transformation of the character below 31 to text. that's where the point 9 shouldn't be in CommFacilitator.cs
I replace
          public void RcvACK()
          {
#if DEBUG
               AppendToLog("CurrentMessage.FrameList.Count: " + comm.CurrentMessage.FrameList.Count);
               AppendToLog("CurrentFrameCounter: " + comm.CurrentFrameCounter);
#endif
               // If all frames have been sent, end the transmission.
               if (comm.CurrentMessage.FrameList.Count == comm.CurrentFrameCounter)
               {
                    comm.Send(Constants.EOT);
                    comm.CurrentMessage = new Message(comm);
               }
               else
               {
                    // Otherwise, send next frame.
                    comm.Send(comm.CurrentMessage.FrameList[comm.CurrentFrameCounter]);
                comm.CurrentFrameCounter++;
                    // Reset the NAK count to 0.
                    comm.NumNAK = 0;
                    // Reset the transaction timer to 15 seconds.
                    comm.TransTimer.Reset(15);
               }
          }
by
          public void RcvACK()
          {
#if DEBUG
               AppendToLog("CurrentMessage.FrameList.Count: " + comm.CurrentMessage.FrameList.Count);
               AppendToLog("CurrentFrameCounter: " + comm.CurrentFrameCounter);
#endif
               // If all frames have been sent, end the transmission.
               if (comm.CurrentMessage.FrameList.Count == comm.CurrentFrameCounter)
               {
                    comm.Send(Constants.EOT);
                    UniversaLIService.AppendToLog($"out: \t" + "<EOT>");     //GD 28 01 2024
                    comm.CurrentMessage = new Message(comm);
               }
               else
               {
                    // Otherwise, send next frame.
                    comm.Send(comm.CurrentMessage.FrameList[comm.CurrentFrameCounter]);
                    UniversaLIService.AppendToLog($"out: \t" + comm.CurrentMessage.FrameList[comm.CurrentFrameCounter]);     //GD 28 01 2024
                comm.CurrentFrameCounter++;
                    // Reset the NAK count to 0.
                    comm.NumNAK = 0;
                    // Reset the transaction timer to 15 seconds.
                    comm.TransTimer.Reset(15);
               }
          }
in TransWaitState.cs
I replace
          public void RcvData(string InputString)
          {
               // Data frames should always be preceded by other signals, so for now, just log it.
               // Signal the instrument to interrupt the transmission with an EOT.
               AppendToLog("Data received in TransWait state: " + InputString);
               comm.Send(Constants.EOT);
          }
by
          public void RcvData(string InputString)
          {
               // Data frames should always be preceded by other signals, so for now, just log it.
               // Signal the instrument to interrupt the transmission with an EOT.
               AppendToLog("Data received in TransWait state: " + InputString);
               comm.Send(Constants.EOT);
               UniversaLIService.AppendToLog($"out: \t" + "<EOT>");     //GD 28 01 2024
          }
in TransWaitState.cs
It misses the transformation of the character below 31 to text. that's where the point 9 shouldn't be in CommFacilitator.cs
I replace
          public void RcvNAK()
          {
               // Send old frame.
               comm.Send(comm.CurrentMessage.FrameList[comm.CurrentFrameCounter - 1]);
               // Increment NAK count.
               comm.NumNAK++;
               if (comm.NumNAK == 6)
               {
                    // Too many NAKs. Something's wrong. Send an EOT and go back to Idle.
                    // Maybe stick the message back in the queue to try again later?
                    comm.Send(Constants.EOT);
                    comm.OutboundMessageQueue.Enqueue(comm.CurrentMessage);
                    comm.CurrentMessage = new Message(comm);
               }
          }
by
          public void RcvNAK()
          {
               // Send old frame.
               comm.Send(comm.CurrentMessage.FrameList[comm.CurrentFrameCounter - 1]);
               UniversaLIService.AppendToLog($"out: \t" + comm.CurrentMessage.FrameList[comm.CurrentFrameCounter - 1]);     //GD 28 01 2024
               // Increment NAK count.
               comm.NumNAK++;
               if (comm.NumNAK == 6)
               {
                    // Too many NAKs. Something's wrong. Send an EOT and go back to Idle.
                    // Maybe stick the message back in the queue to try again later?
                    comm.Send(Constants.EOT);
                    UniversaLIService.AppendToLog($"out: \t" + "<EOT>");     //GD 28 01 2024
                    comm.OutboundMessageQueue.Enqueue(comm.CurrentMessage);
                    comm.CurrentMessage = new Message(comm);
               }
          }

------------------------------
>11
i moved the method asciitotext from CommFacilitator.cs  to UniversaLIService.cs
    public class CommFacilitator
         public string AsciiToText(string txt)
         {...
         }
became
public partial class UniversaLIService : BackgroundService
    public class CommFacilitator
         public static string AsciiToText(string txt)
         {...
         }

>12 In TransENQState.cs     
Now i moved the method asciitotext from CommFacilitator.cs  to UniversaLIService.cs
Then I can use this method anywhere
I replace
          public void RcvACK()
          {
               // Send next frame.
               comm.CurrentMessage = comm.OutboundMessageQueue.Dequeue();
               comm.CurrentMessage.PrepareToSend();
               comm.Send(comm.CurrentMessage.FrameList[comm.CurrentFrameCounter]);
               UniversaLIService.AppendToLog($"out: \t" + comm.CurrentMessage.FrameList[comm.CurrentFrameCounter]);                            //GD 28 01 2024
               ...
          }
by 
          public void RcvACK()
          {
               // Send next frame.
               comm.CurrentMessage = comm.OutboundMessageQueue.Dequeue();
               comm.CurrentMessage.PrepareToSend();
               comm.Send(comm.CurrentMessage.FrameList[comm.CurrentFrameCounter]);
               UniversaLIService.AppendToLog($"In: \t{UniversaLIService.AsciiToText(comm.CurrentMessage.FrameList[comm.CurrentFrameCounter])}");  //GD 31 01 2024
               ...
          }
               

