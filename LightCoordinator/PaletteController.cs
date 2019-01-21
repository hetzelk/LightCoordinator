using LightCoordinator.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Drawing;
using LightCoordinator.Model;

namespace LightCoordinator
{
    public class PaletteController
    {
        public static JObject Random()
        {
            Random random = new Random();
            int hue = random.Next(0, 360);
            int saturation = random.Next(0, 100);
            int brightness = random.Next(2, 100);

            JObject color = new LCColor("hsb", hue, saturation, brightness).HSBJO;
            return color;
        }

        public static JArray ToJArray(List<LCColor> palette)
        {
            JArray jarray = new JArray();
            foreach (LCColor color in palette)
            {
                jarray.Add(color.HSBJO);
            }

            return jarray;
        }

        public static JArray RandomPalette(int count,
            int minHue = 1000,
            int maxHue = 1000,
            int minSaturation = 1000,
            int maxSaturation = 1000,
            int minBrightness = 1000,
            int maxBrightness = 1000)
        {
            if (count > 50)
            {
                //20 is the max allowed to edit on the app
                count = 50;
            }

            //if maintaining the values of HSB, check that they are valid
            minHue = (minHue != 1000 && minHue < 360) ? minHue : minHue = 0;
            maxHue = (maxHue != 1000 && maxHue < 360) ? maxHue : maxHue = 360;
            minSaturation = (minSaturation != 1000 && minSaturation < 100) ? minSaturation : minSaturation = 0;
            maxSaturation = (maxSaturation != 1000 && maxSaturation < 100) ? maxSaturation : maxSaturation = 100;
            minBrightness = (minBrightness != 1000 && minBrightness < 100) ? minBrightness : minBrightness = 2 /*min brightness needs to be 2*/;
            maxBrightness = (maxBrightness != 1000 && maxBrightness < 100) ? maxBrightness : maxBrightness = 100;

            JArray colors = new JArray();
            Random random = new Random();

            while (count > 0)
            {
                int hue = random.Next(minHue, maxHue);
                int saturation = random.Next(minSaturation, minSaturation);
                int brightness = random.Next(minBrightness, minBrightness);

                JObject color = new LCColor("hsb", hue, saturation, brightness).HSBJO;
                colors.Add(color);
                count--;
            }

            return colors;
        }
    }
}
