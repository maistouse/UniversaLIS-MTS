﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
// TODO: Add internal database while keeping external database option.
// TODO: Add UI?
namespace UniversaLIS
{
     public partial class UniversaLIService : BackgroundService
    {
        public readonly ILogger<UniversaLIService> EventLogger;
        private static readonly List<CommFacilitator> s_commFacilitators = new List<CommFacilitator>();

        public static bool ListenHL7 { get; set; }
        public static int HL7Port { get; set; }
        public static string? ExternalDbConnString { get; set; }
        public static bool UseExtDB { get; set; }
        public static int DbPollInterval { get; set; }
        public static YamlSettings? YamlSettings { get; set; }

        public UniversaLIService(ILogger<UniversaLIService> logger)
          {
               EventLogger = logger;
          }

        public void HandleEx(Exception ex)
        {
            if (ex is null)
            {
                return;
            }
            string? message = ex.Source + " - Error: " + ex.Message + "\n" + ex.TargetSite + "\n" + ex.StackTrace;
            EventLogger.LogError(message);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
               OnStart();
               while (!stoppingToken.IsCancellationRequested) {
                    await Task.Delay(1000, stoppingToken);
               }
               OnStop();
        }

        protected void OnStart()
        {
            try
            {
                using (var reader = new StreamReader("Properties/config.yml"))
                {
                    var yamlText = reader.ReadToEnd();
                    var deserializer = new DeserializerBuilder()
                         .Build();

                    YamlSettings = deserializer.Deserialize<YamlSettings>(yamlText);
                    if (YamlSettings.ServiceConfig?.ListenHl7 == true)
                    {
                        // TODO: Actually set up the HL7 listener.
                    }
                    if (YamlSettings.ServiceConfig?.UseExternalDb == true)
                    {
                        UseExtDB = true;
                        ExternalDbConnString = YamlSettings.ServiceConfig.ConnectionString;
                    }
                    else
                    {
                        UseExtDB = false;
                    }
                    foreach (var serialPort in YamlSettings?.Interfaces?.Serial ?? Enumerable.Empty<Serial>())
                    {
                        s_commFacilitators.Add(new CommFacilitator(serialPort, this));
                    }
                    foreach (var tcpPort in YamlSettings?.Interfaces?.Tcp ?? Enumerable.Empty<Tcp>())
                    {
                        s_commFacilitators.Add(new CommFacilitator(tcpPort, this));
                    }
                }
            }
            catch (Exception ex)
            {
                HandleEx(ex);
                throw;
            }
        }
        protected void OnStop()
        {

            try
            {
                AppendToLog("Service stopping.");
                foreach (var comm in s_commFacilitators)
                {
                    comm.Close();
                }
            }
            catch (Exception ex)
            {
                HandleEx(ex);
                throw;
            }
        }

        public static void AppendToLog(string txt)
        {
            string? publicFolder = Environment.GetEnvironmentVariable("AllUsersProfile");
            var date = DateTime.Now;
            string txtFile = $"{publicFolder}\\UniversaLIS\\Service_Logs\\Log_{date.Year}-{date.Month}-{date.Day}.txt";
            if (!Directory.Exists($"{publicFolder}\\UniversaLIS\\Service_Logs\\"))
            {
                Directory.CreateDirectory($"{publicFolder}\\UniversaLIS\\Service_Logs\\");
            }
            string txtWrite = $"{date.ToLocalTime()} \t{txt}\r\n";
            File.AppendAllText(txtFile, txtWrite);
        }


        public static string CHKSum(string message)
        {
            // This function returns the checksum for the data string passed to it.
            // If I've done it right, the checksum is calculated by binary 8-bit addition of all included characters
            // with the 8th or parity bit assumed zero. Carries beyond the 8th bit are lost. The 8-bit result is
            // converted into two printable ASCII Hex characters ranging from 00 to FF, which are then inserted into
            // the data stream. Hex alpha characters are always uppercase.

            string? checkSum;
            int ascSum, modVal;
            ascSum = 0;
            foreach (char c in message)
            {
                if ((int)c != 2)
                {    // Don't count any STX.
                    ascSum += (int)c;
                }
            }
            modVal = ascSum % 256;
            checkSum = modVal.ToString("X");
            return checkSum.PadLeft(2, '0');
        }

        // <summary>Method invoked when service is started from a debugging console.</summary>
        internal void DebuggingRoutine()
        {
            OnStart();
            while (Console.ReadLine() is null)
            {
                // Not sure why it doesn't work without looping anymore. 
                Console.ReadLine();
            }
            OnStop();
        }

    }

}