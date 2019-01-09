using System;
using System.Collections.Generic;
using LightCoordinator.Model;

namespace LightCoordinator
{
    class Program
    {
        private static List<long> times = new List<long>();

        static void Main(string[] args)
        {
            Nanoleaf nanoleaf = new Nanoleaf("192.168.1.6", "16021", "jSmYnP4H8gXooZ4IK87nbJR1vUna5oBX");
            int mode = 5;
            new Mode(nanoleaf, mode);
        }

        public static void RunTest()
        {
            //Nanoleaf nanoleaf = new Nanoleaf(ip, port, auth_token);

            //string[] files = { "1.jpg", "2.jpg", "3.jpg", "4.jpg", "5.jpg", "6.jpg", "7.jpg" };
            //foreach (string file in files)
            //{
            //    var bitmap = ImageAnalysis.createBitMap(file);
            //    var mostUsed = ImageAnalysis.GetColorsFromImage(bitmap, 15);

            //    JArray palette = new JArray();
            //    foreach (Color color in mostUsed)
            //    {
            //        palette.Add(Palette.ConvertToHSB(color.R, color.G, color.B));
            //    }

            //    nanoleaf.CreateCustom(palette, 10, 10, animType: "random");
            //    Thread.Sleep(1);
            //}
            //return;
            //Random random = new Random();

            //Dictionary<string, string> layout = nanoleaf.Get("/panelLayout/layout");
            //Dictionary<string, string> existingEffects = nanoleaf.Get("/effects/effectsList");

            //JArray colors = Palette.RandomPalette(15, minSaturation: 40, maxSaturation: 50, minBrightness: 40, maxBrightness: 50);
            //nanoleaf.CreateCustom(colors, 10, 10);
            //nanoleaf.CreateCustomRhythm(colors, 50);
            //nanoleaf.RainbowShift(1000, 7, 100);
            //nanoleaf.RandomStatic();
            //nanoleaf.On();
            //nanoleaf.SetState(Palette.HSB(11, 11, 100).HSBtoDict(), 1000);
        }
    }
}
