using System;

namespace UniversaLIS
{
     interface IPortAdapter
     {
          public string PortName { get; }
          public string PortType();
          public void Send(string messageText);
          // public void Open();                               //GD: 19 11 2023
          public int Open();
          public void Close();
          internal string ReadChars();
          public virtual event EventHandler PortDataReceived
          {
               add
               {
                    PortDataReceived += value;
               }
               remove
               {
                    PortDataReceived -= value;
               }
          }
     }
}
