using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Xml;

namespace LightCoordinator.Extensions
{
    public static class JsonEx
    {
        public static string ToString1(this JObject json)
        {
            return JsonConvert.SerializeObject(json, Newtonsoft.Json.Formatting.None);
        }

        public static Dictionary<string, int> HSBtoDict(this JObject json)
        {
            int hue = Int16.Parse(json["hue"].ToString());
            Dictionary<string, int> palette = new Dictionary<string, int>(){

                { "hue", Int16.Parse(json["hue"].ToString()) },
                { "sat", Int16.Parse(json["saturation"].ToString()) },
                { "brightness", Int16.Parse(json["brightness"].ToString()) }
            };
            return palette;
        }
    }
}