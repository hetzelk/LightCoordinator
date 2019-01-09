using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LightCoordinator.Model;

namespace LightCoordinator
{
    public class Mode
    {
        private LightController lightController;
        private static List<long> times = new List<long>();

        public Mode(Nanoleaf nanoleaf, int mode)
        {
            this.lightController = new LightController();
            //get the number of light panels and double it
            switch (mode)
            {
                case 1:
                    Testing(nanoleaf);
                    break;
                case 2:
                    MimickAverage(nanoleaf);
                    break;
                case 3:
                    MimickMostUsed(nanoleaf);
                    break;
                case 4:
                    MimickPosition(nanoleaf);
                    break;
                case 5:
                    StreamingMode(nanoleaf);
                    break;
                default:
                    Testing(nanoleaf);
                    break;
            }
        }

        public void Testing(Nanoleaf nanoleaf)
        {

        }

        public long ConvertIP(string addr)
        {
            // careful of sign extension: convert to uint first;
            // unsigned NetworkToHostOrder ought to be provided.
            return (long)(uint)IPAddress.NetworkToHostOrder(
                 (int)IPAddress.Parse(addr).Address);
        }

        public void StreamingMode(Nanoleaf nanoleaf)
        {
            //UdpClient udp = new UdpClient();
            ////udp.EnableBroadcast = true;  //This was suggested in a now deleted answer
            //IPEndPoint groupEP = new IPEndPoint(ConvertIP("192.168.1.6"), 60221);
            //string str4 = "2 55 1 255 0 0 0 1 10 1 0 255 0 0 1 ";
            //byte[] sendBytes4 = Encoding.ASCII.GetBytes(str4);
            //udp.Send(sendBytes4, sendBytes4.Length, groupEP);
            //udp.Close();

            //get panel layout
            bool first = true;
            string body = lightController.Get(nanoleaf, "/panelLayout/layout")["body"];
            List<Panel> panels = Panel.GetPanels(body);

            List<Color> palette = null;
            Stopwatch timer = null;
            Bitmap bitmap = null;

            while (true)
            {
                timer = new Stopwatch();
                timer.Start();

                palette = new List<Color>();
                bitmap = ImageAnalysis.CaptureBitmap(1);

                if (bitmap == null)
                {
                    palette.Add(Color.Black);
                }
                else
                {
                    int paletteCount = panels.Count;
                    palette = ImageAnalysis.ReduceColors(ImageAnalysis.GetAverageColors(bitmap), true, paletteCount);
                }

                timer.Stop();
                long ms = timer.ElapsedMilliseconds;
                if (ms < 100)
                {
                    Thread.Sleep(50);
                }
                timer.Start();

                lightController.CreateTemp(nanoleaf, panels, palette);

                timer.Stop();
                ms = timer.ElapsedMilliseconds;
                
                if (!first)
                {
                    CalcAverage(ms);
                }
                first = false;
            }
        }

        public void MimickAverage(Nanoleaf nanoleaf)
        {
            JArray palette = null;
            Stopwatch timer = null;
            Bitmap bitmap = null;
            while (true)
            {
                palette = new JArray();
                timer = new Stopwatch();
                timer.Start();
                bitmap = ImageAnalysis.CaptureBitmap(1);

                if (bitmap == null)
                {
                    palette = PaletteController.RandomPalette(1, maxBrightness: 0);
                }
                else
                {
                    int paletteCount = 12;
                    var averages = ImageAnalysis.ReduceColors(ImageAnalysis.GetAverageColors(bitmap), true, paletteCount);
                    
                    foreach (Color color in averages)
                    {
                        palette.Add(new LCColor("rgb", color.R, color.G, color.B).RGBJO);
                    }
                }

                lightController.CreateCustom(nanoleaf, palette, 5, 50, animType: "random");

                timer.Stop();
                CalcAverage(timer.ElapsedMilliseconds);
            }
        }

        public void MimickMostUsed(Nanoleaf nanoleaf)
        {
            Stopwatch timer = null;
            Bitmap bitmap = null;
            while (true)
            {
                timer = new Stopwatch();
                timer.Start();
                bitmap = ImageAnalysis.CaptureBitmap(1);

                if (bitmap == null)
                {
                    JArray colors = PaletteController.RandomPalette(1, maxBrightness: 0);
                    lightController.CreateCustom(nanoleaf, colors, 10, 10);
                }
                else
                {
                    int paletteCount = 12;
                    var mostUsed = ImageAnalysis.GetColorsFromImage(bitmap, paletteCount);

                    JArray palette = new JArray();
                    foreach (Color color in mostUsed)
                    {
                        LCColor lcc = new LCColor("rgb", color.R, color.G, color.B);
                        palette.Add(lcc);
                    }

                    lightController.CreateCustom(nanoleaf, palette, 5, 50, animType: "random");
                }

                timer.Stop();
                CalcAverage(timer.ElapsedMilliseconds);
            }
        }

        public void MimickPosition(Nanoleaf nanoleaf)
        {

        }

        public static void CalcAverage(long ms)
        {
            Console.Clear();
            List<long> averages = times;
            if (averages.Count > 0)
            {
                double average = averages.Average();

                Console.WriteLine("Updates: {0}", times.Count);
                Console.WriteLine("Latest: {0}", ms);
                Console.WriteLine("Average: {0}", average);
            }

            times.Add(ms);
        }
    }
}
