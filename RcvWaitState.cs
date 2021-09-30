﻿using System;
using static UniversaLIS.ServiceMain;

namespace UniversaLIS
{
     class RcvWaitState : ILISState
     {
          // Track the current frame number to ensure that the received frame is correct.
          public int ExpectedFrame = 1;
          protected internal CommFacilitator comm;
          public void RcvInput(string InputString)
          {
               switch (InputString)
               {
                    case Constants.ACK:
                         RcvACK();
                         break;
                    case Constants.NAK:
                         RcvNAK();
                         break;
                    case Constants.ENQ:
                         RcvENQ();
                         break;
                    case Constants.EOT:
                         RcvEOT();
                         break;
                    default:
                         RcvData(InputString);
                         break;
               }
          }
          public void RcvACK()
          {
               // This shouldn't happen. Ignore it, I guess, and hope the instrument gets its act together.
               AppendToLog("ACK received in RcvWait state.");
          }

          public void RcvData(string InputString)
          {
               // Compare the frame number and checksum to see whether the frame is good or bad.
               bool isFrameGood;
               // Compare frame numbers.
               if (InputString.TrimStart('\x02').StartsWith(ExpectedFrame.ToString()))
               {
                    isFrameGood = true;
               }
               else
               {
                    AppendToLog("Frame number is incorrect!");
                    isFrameGood = false;
               }
               // Check checksum.
               if (isFrameGood)
               {
                    isFrameGood = CheckChecksum(InputString);
               }
               // If it's a header message, check the password.
               if (isFrameGood)
               {
                    isFrameGood = CheckPassword(InputString);
               }
               // If the frame is good, act accordingly.
               if (isFrameGood)
               {
                    // Send ACK 
                    comm.ComPort.Send(Constants.ACK);
                    // Reset rcvTimer to 30 seconds.
                    comm.rcvTimer.Reset(30);
                    // Increment frame number.
                    ExpectedFrame = ++ExpectedFrame % 8;
                    // Actually handle the frame.
                    comm.ParseMessageLine(InputString);
               }
               else
               {
                    // Send NAK
                    comm.ComPort.Send(Constants.NAK);
                    // Reset rcvTimer to 30 seconds.
                    comm.rcvTimer.Reset(30);
               }

          }

          private bool CheckPassword(string inputString)
          {
               if (inputString.Substring(0, 3)==$"{Constants.STX}1H")
               {
                    // 1H|\\^&||{password}|
                    String[] fieldArray = inputString.Split('|');
                    if (fieldArray[3] != comm.password)
                    {
                         return false;
                    }
               };
               return true;
          }

          private bool CheckChecksum(string InputString)
          {
               string message = InputString;
               // There should be a message ending in a <CR><ETX>, then a checksum, and then a <CR><LF> at the end of the line.
               // Find the <ETX>. Any message that reaches this part of the code should have one.
               int position = message.IndexOf(Constants.ETX);
               if (position < 0)
               {
                    // If no <ETX>, maybe it's an intermediate frame. Check for <ETB>.
                    position = message.IndexOf(Constants.ETB);
                    if (position < 0)
                    {
                         return false;
                    }
               }
               string mainMessage = message.Substring(1, position);
               // The checksum is generated by passing everything between the <STX> and the checksum to the CHKSum function below,
               // but for some reason the "Result Message" examples in the documentation don't match. 
               // The other messages do, though, so it's probably fine.
               string checkSum = message.Substring(position + 1, 2);
               // If the checksum doesn't match, write a <NAK> to the sender.
               if (checkSum != CHKSum(mainMessage))
               {
                    return false;
               }
               // Otherwise, it's good.
               return true;
          }

          public void RcvENQ()
          {
               // This shouldn't happen since we already received the ENQ that brought us to this state.
               // Ignore it, I guess, and hope the instrument finds what it's looking for.
               AppendToLog("ENQ received in RcvWait state.");
          }

          public void RcvEOT()
          {
               // Discard last incomplete message (if applicable).
               if (comm.CurrentMessage.Terminator < 'E')
               {
                    comm.CurrentMessage = new Message(comm);
               }
               else
               {
                    comm.ProcessMessage(comm.CurrentMessage);
               }
          }

          public void RcvNAK()
          {
               // This shouldn't happen. Ignore it, I guess, and hope the instrument feels better soon.
               AppendToLog("NAK received in RcvWait state.");
          }

          void ILISState.HaveData()
          {
               // It doesn't matter if we have data to send. We're receiving right now.
               AppendToLog("HaveData called in RcvWait state.");
          }
     }
}
