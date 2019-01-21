using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightCoordinator.Model
{
    public class LCColor
    {
        public int h { get; set; }
        public int s { get; set; }
        public int v { get; set; }

        public int r { get; set; }
        public int g { get; set; }
        public int b { get; set; }

        public LCColor(Color color)
        {
            SetRGB(color.R, color.G, color.B);
        }

        public LCColor(string type, int v1, int v2, int v3)
        {
            if (type == "hsb")
            {
                SetHSB(v1, v2, v3);
            }
            else
            {
                SetRGB(v1, v2, v3);
            }
        }

        //i want to be able to get any color in any form I want at any time
        //i want to be able to set them from anywhere using any values as well

        //give all the return types in the way you want them and have setting the types, set all of the variables at once

        public JObject HSBJO
        {
            get
            {
                dynamic color = new JObject();
                color.hue = h;
                color.saturation = s;
                color.brightness = v;
                return color;
            }
        }

        public JObject RGBJO
        {
            get
            {
                dynamic color = new JObject();
                color.type = "rgb";
                color.red = r;
                color.green = g;
                color.blue = b;
                return color;
            }
        }

        public Dictionary<string, int> DictHSB
        {
            get
            {
                Dictionary<string, int> color = new Dictionary<string, int>(){
                    { "hue", h },
                    { "saturation", s },
                    { "brightness", b }
                };
                return color;
            }
        }

        public string RGBWT
        {
            get
            {
                int W = 0; //default value is 0 because nanoleaf use W to fade into the other colors
                int updateDuration = 3; //1 is equal to 100ms
                return String.Format("{0} {1} {2} {3} {4}", r, g, b, W, updateDuration);
            }
        }

        public JObject SetHSB(int h, int s, int b)
        {
            this.h = h;
            this.s = s;
            this.b = b;

            ConvertToRGB(h, s, b);

            return HSBJO;
        }

        public JObject SetRGB(int r, int g, int b)
        {
            this.r = r;
            this.g = g;
            this.b = b;

            ConvertToHSB(r, g, b);

            return RGBJO;
        }
        
        public void ConvertToRGB(JObject hsb)
        {
            ConvertToRGB(Int32.Parse(hsb["hue"].ToString()), Int32.Parse(hsb["hue"].ToString()), Int32.Parse(hsb["hue"].ToString()));
        }

        public void ConvertToRGB(int h, int s, int v)
        {
            var red = 0;
            var green = 0;
            var blue = 0;

            int i = Convert.ToInt32(h * 6);
            var f = h * 6 - i;
            var p = v * (1 - s);
            var q = v * (1 - f * s);
            var t = v * (1 - (1 - f) * s);

            switch (i % 6)
            {
                case 0: { red = v; green = t; blue = p; break; }
                case 1: { red = q; green = v; blue = p; break; };
                case 2: { red = p; green = v; blue = t; break; };
                case 3: { red = p; green = q; blue = v; break; };
                case 4: { red = t; green = p; blue = v; break; };
                case 5: { red = v; green = p; blue = q; break; };
            }

            red = red * 255;
            green = green * 255;
            blue = blue * 255;

            red = red > 255 ? 255 : red;
            green = green > 255 ? 255 : green;
            blue = blue > 255 ? 255 : blue;
            
            r = red;
            g = green;
            b = blue;
        }

        public void ConvertToHSB(decimal red, decimal green, decimal blue)
        {
            decimal hsv_red = red / 255;
            decimal hsv_green = green / 255;
            decimal hsv_blue = blue / 255;

            decimal[] colors = { hsv_red, hsv_green, hsv_blue };
            decimal max = colors.Max();
            decimal min = colors.Min();

            decimal h = 0;
            decimal s = 0;
            decimal b = max;
            decimal g = max - min;

            s = max == s ? 0 : g / max;

            if (max == min)
            {
                h = 0;
            }
            else
            {
                if (max == hsv_red)
                {
                    h = (hsv_green - hsv_blue) / g + (hsv_green < hsv_blue ? 6 : 0);
                }
                else if (hsv_green == max)
                {
                    h = (hsv_blue - hsv_red) / g + 2;
                }
                else if (hsv_blue == max)
                {
                    h = (hsv_red - hsv_green) / g + 4;
                }
                h /= 6;
            }

            int hue = Convert.ToInt32(Math.Round(h * 100, MidpointRounding.AwayFromZero));
            hue = (hue * 360) / 100;
            int sat = Convert.ToInt32(Math.Round(s * 100, MidpointRounding.AwayFromZero));
            int bri = Convert.ToInt32(Math.Round(b * 100, MidpointRounding.AwayFromZero));

            hue = hue > 360 ? 360 : hue;
            sat = sat > 100 ? 100 : sat;
            bri = bri > 100 ? 100 : bri;

            h = hue;
            s = sat;
            v = bri;
        }
    }
}
