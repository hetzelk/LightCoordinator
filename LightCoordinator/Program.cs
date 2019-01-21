using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LightCoordinator.Model;
using LightCoordinator.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LightCoordinator
{
    class Program
    {
        private static List<long> times = new List<long>();

        private static List<Nanoleaf> lights = new List<Nanoleaf>();

        static void Main(string[] args)
        {
            string data = "";
            //if there is a configured light saved, do not go back though the sync process
            using (StreamReader sr = new StreamReader("connected.txt"))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    if (line != "")
                    {
                        string[] values = line.Split(':');
                        lights.Add(new Nanoleaf(values[0], values[1], values[2]));
                    }
                }
            }

            if (lights.Count == 0)
            {
                AddLight();
            }
            
            if (lights.Count == 0)
            {
                Log.Write("Please restart the app and try again.");
            }
            else
            {
                Parallel.ForEach(lights, (light) =>
                {
                    int mode = 5;
                    new Mode(light, mode);
                });
            }
        }

        static void AddLight()
        {
            Ping_all();

            Nanoleaf nanoleaf = null;
            int attempts = 1;

            while (nanoleaf == null && attempts < 10)
            {
                attempts++;
                Log.Write("Hold the on-off button down for 5-7 seconds until the LED starts flashing in a pattern");//pairing stays active for about 30s
                Thread.Sleep(/*5000*/100);//sleep for 5 seconds for the user to press and hold the button
                Log.Write("waited");

                Parallel.ForEach(lights, (light) =>
                {
                    Log.Write("attempt: " + attempts);
                    nanoleaf = new LightController().SelectCorrectLight(light);
                });

                if (nanoleaf == null)
                {
                    Thread.Sleep(1000);
                    Log.Write("slept");
                }
            }
        }

        static string GetIP()
        {
            string ip = null;

            foreach (NetworkInterface f in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (f.OperationalStatus == OperationalStatus.Up)
                {
                    var properties = f.GetIPProperties();
                    foreach (GatewayIPAddressInformation d in properties.GatewayAddresses)
                    {
                        ip = d.Address.ToString();
                    }
                }
            }
            return ip;
        }

        static void Ping_all()
        {
            string gate_ip = GetIP();

            string[] array = gate_ip.Split('.');

            for (int i = 2; i <= 255; i++)
            {
                string ping_var = array[0] + "." + array[1] + "." + array[2] + "." + i;

                Ping(ping_var, 4, 4000);
            }

        }

        static void Ping(string host, int attempts, int timeout)
        {
            for (int i = 0; i < attempts; i++)
            {
                try
                {
                    Ping ping = new Ping();
                    ping.PingCompleted += new PingCompletedEventHandler(PingCompleted);
                    ping.SendAsync(host, timeout, host);
                }
                catch
                {
                    // Do nothing
                }
            }
        }

        static void PingCompleted(object sender, PingCompletedEventArgs e)
        {
            string ip = (string)e.UserState;
            if (e.Reply != null && e.Reply.Status == IPStatus.Success)
            {
                string macaddress = GetMacAddress(ip);
                if (macaddress == "NOTHING")
                {
                    return;
                }

                foreach (Nanoleaf l in lights)
                {
                    if (l.ip == ip)
                    {
                        return;
                    }
                }
                lights.Add(new Nanoleaf(ip, "16021"));
            }
        }

        static string GetMacAddress(string ipAddress)
        {
            string macAddress = string.Empty;
            System.Diagnostics.Process Process = new System.Diagnostics.Process();
            Process.StartInfo.FileName = "arp";
            Process.StartInfo.Arguments = "-a " + ipAddress;
            Process.StartInfo.UseShellExecute = false;
            Process.StartInfo.RedirectStandardOutput = true;
            Process.StartInfo.CreateNoWindow = true;
            Process.Start();
            string strOutput = Process.StandardOutput.ReadToEnd();
            string[] substrings = strOutput.Split('-');
            if (substrings.Length >= 8)
            {
                macAddress = substrings[3].Substring(Math.Max(0, substrings[3].Length - 2))
                         + "-" + substrings[4] + "-" + substrings[5] + "-" + substrings[6]
                         + "-" + substrings[7] + "-"
                         + substrings[8].Substring(0, 2);

                if (macAddress.StartsWith("00-55-da") && ipAddress == "192.168.1.6")
                {
                    return macAddress;
                }
                else
                {
                    return "NOTHING";
                }
            }
            else
            {
                return "NOTHING";
            }
        }
    }
}
