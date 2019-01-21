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
using LightCoordinator.Extensions;

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
                    Screenshot(nanoleaf);
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
                case 6:
                    FrameMode(nanoleaf);
                    break;
                default:
                    Screenshot(nanoleaf);
                    break;
            }
        }

        public void Screenshot(Nanoleaf nanoleaf)
        {
            List<LCColor> palette = null;
            Bitmap bitmap = null;
            string body = lightController.Get(nanoleaf, "/panelLayout/layout")["body"];
            List<Panel> panels = Panel.GetPanels(body);
            int paletteCount = panels.Count;
            Tuple<int, int> gcd = ImageAnalysis.DivideScreen(paletteCount);

            palette = new List<LCColor>();
            bitmap = ImageAnalysis.CaptureBitmap(0);

            if (bitmap == null)
            {
                palette.Add(new LCColor(Color.Black));
            }
            else
            {
                //palette = ImageAnalysis.ReduceColors(ImageAnalysis.GetAverageColors(bitmap, gcd.Item1, gcd.Item2), true, paletteCount);
                palette = ImageAnalysis.GetAverageColors(bitmap, gcd.Item1, gcd.Item2, borderReduction: 300);
            }

            lightController.CreateCustom(nanoleaf, PaletteController.ToJArray(palette));
        }

        public long ConvertIP(string addr)
        {
            // careful of sign extension: convert to uint first;
            // unsigned NetworkToHostOrder ought to be provided.
            return (long)(uint)IPAddress.NetworkToHostOrder(
                 (int)IPAddress.Parse(addr).Address);
        }

        public void FrameMode(Nanoleaf nanoleaf)
        {
            string body = lightController.Get(nanoleaf, "/panelLayout/layout")["body"];
            List<Panel> panels = Panel.GetPanels(body);
            int paletteCount = panels.Count;
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
            
            List<LCColor> palette = null;
            Stopwatch timer = null;
            Bitmap bitmap = null;

            int paletteCount = panels.Count;
            Tuple<int, int> gcd = ImageAnalysis.DivideScreen(paletteCount);

            while (true)
            {
                timer = new Stopwatch();
                timer.Start();

                palette = new List<LCColor>();
                bitmap = ImageAnalysis.CaptureBitmap(0);

                if (bitmap == null)
                {
                    palette.Add(new LCColor(Color.Black));
                }
                else
                {
                    //palette = ImageAnalysis.ReduceColors(ImageAnalysis.GetAverageColors(bitmap, gcd.Item1, gcd.Item2), true, paletteCount);
                    palette = ImageAnalysis.GetAverageColors(bitmap, gcd.Item1, gcd.Item2, borderReduction: 300);
                }
                
                if (timer.ElapsedMilliseconds < 100)
                {
                    if (100 - timer.ElapsedMilliseconds > 0)
                    {
                        Thread.Sleep(100 - Convert.ToInt32(timer.ElapsedMilliseconds));
                    }
                }

                //TODO: change the position of some of the panels on shuffle, not all
                bool shuffle = true;
                if (shuffle && times.Count % 100 == 0)
                {
                    panels = Panel.Shuffle(panels);
                }

                lightController.CreateTemp(nanoleaf, panels, palette);

                timer.Stop();
                long ms = timer.ElapsedMilliseconds;

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
                    var averages = ImageAnalysis.ReduceColors(ImageAnalysis.GetAverageColors(bitmap, 1, 1), true, paletteCount);
                    throw new Exception();

                    foreach (LCColor color in averages)
                    {
                        palette.Add(new LCColor("rgb", color.r, color.g, color.b).RGBJO);
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
                    foreach (LCColor color in mostUsed)
                    {
                        LCColor lcc = new LCColor("rgb", color.r, color.g, color.b);
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

                Log.Write(String.Format("Updates: {0}", times.Count));
                Log.Write(String.Format("Latest: {0}", ms));
                Log.Write(String.Format("Average: {0}", average));
            }

            times.Add(ms);
        }
    }
}
