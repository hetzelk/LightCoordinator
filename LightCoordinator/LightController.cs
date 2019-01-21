using LightCoordinator.Extensions;
using LightCoordinator.Model;
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

namespace LightCoordinator
{
    public class LightController
    {
        delegate bool SendLightDelegate(string url_ex, string json);

        public LightController()
        {

        }

        public void PairLights()
        {

        }

        public Nanoleaf SelectCorrectLight(Nanoleaf nanoleaf)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            WebRequest request = WebRequest.Create(nanoleaf.url + "/new");
            request.ContentType = "application/json";
            request.Method = "POST";

            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        Log.Error("Sync response failed");
                        return null;
                    }
                    else
                    {
                        WebHeaderCollection header = response.Headers;

                        var encoding = ASCIIEncoding.ASCII;
                        string body = "";
                        using (var reader = new StreamReader(response.GetResponseStream(), encoding))
                        {
                            body = reader.ReadToEnd();
                            nanoleaf.auth_token = JObject.Parse(body)["auth_token"].ToString();
                            return nanoleaf;
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Log.Error(e.ToString());
                return null;
            }
        }

        public Dictionary<string, string> Get(Nanoleaf nanoleaf, string url_ex)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            WebRequest request = WebRequest.Create(nanoleaf.url + url_ex);
            request.ContentType = "application/json";
            request.Method = "GET";

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception();
                }
                else
                {
                    WebHeaderCollection header = response.Headers;

                    var encoding = ASCIIEncoding.ASCII;
                    string body = "";
                    using (var reader = new StreamReader(response.GetResponseStream(), encoding))
                    {
                        body = reader.ReadToEnd();
                    }

                    watch.Stop();
                    long ms = watch.ElapsedMilliseconds;
                    Dictionary<string, string> get = new Dictionary<string, string>(){
                        { "body", body },
                        { "ms", ms.ToString() }
                    };
                    return get;
                }
            }
        }

        public Dictionary<string, string> Put(Nanoleaf nanoleaf, string url_ex, string json)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            WebRequest request = WebRequest.Create(nanoleaf.url + url_ex);
            request.ContentType = "application/json";
            request.Method = "PUT";

            MemoryStream jsonContent = new MemoryStream(Encoding.ASCII.GetBytes(json));

            using (var content = request.GetRequestStream())
            {
                jsonContent.Seek(0, SeekOrigin.Begin);
                jsonContent.WriteTo(content);
            }

            Dictionary<string, string> put = new Dictionary<string, string>();

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    WebHeaderCollection header = response.Headers;

                    var encoding = ASCIIEncoding.ASCII;
                    string body = "";
                    using (var reader = new StreamReader(response.GetResponseStream(), encoding))
                    {
                        body = reader.ReadToEnd();
                    }

                    put.Add("body", body);
                }
                else if (response.StatusCode != HttpStatusCode.NoContent)
                {
                    throw new Exception();
                }

                put.Add("json", json);
            }

            watch.Stop();
            long ms = watch.ElapsedMilliseconds;
            put.Add("ms", ms.ToString());
            return put;
        }

        public void FireAndForget(Nanoleaf nanoleaf, Delegate d, params object[] args)
        {
            ThreadPool.QueueUserWorkItem(callback => PutAndForget(nanoleaf, args[0].ToString(), args[1].ToString()));
        }

        public bool PutAndForget(Nanoleaf nanoleaf, string url_ex, string json)
        {
            WebRequest request = WebRequest.Create(nanoleaf.url + url_ex);
            request.ContentType = "application/json";
            request.Method = "PUT";

            MemoryStream jsonContent = new MemoryStream(Encoding.ASCII.GetBytes(json));

            using (var content = request.GetRequestStream())
            {
                jsonContent.Seek(0, SeekOrigin.Begin);
                jsonContent.WriteTo(content);
            }

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
                else if (response.StatusCode != HttpStatusCode.NoContent)
                {
                    return false;
                }

                return true;
            }
        }

        public Dictionary<string, string> SetState(Nanoleaf nanoleaf, Dictionary<string, int> settings, int delay)
        {
            dynamic jsonObject = new JObject();
            foreach (KeyValuePair<string, int> pair in settings)
            {
                switch (pair.Key)
                {
                    case "on":
                        jsonObject.on = pair.Value == 1;
                        break;
                    case "hue":
                        jsonObject.hue = pair.Value;
                        break;
                    case "sat":
                        jsonObject.sat = pair.Value;
                        break;
                    case "saturation":
                        jsonObject.sat = pair.Value;
                        break;
                    case "brightness":
                        jsonObject.brightness = pair.Value;
                        break;
                    case "ct":
                        jsonObject.ct = pair.Value;
                        break;
                    case "probability":
                        jsonObject.duration = pair.Value;
                        break;
                    default:
                        jsonObject.on = true;
                        break;
                }
            }

            string url_ex = "/state";

            return Put(nanoleaf, url_ex, jsonObject.ToString(Newtonsoft.Json.Formatting.None));
        }

        public Dictionary<string, string> StartExistingEffect(Nanoleaf nanoleaf, string effect_name)
        {
            dynamic jsonObject = new JObject();
            jsonObject.select = effect_name;
            string url_ex = "/effects";

            return Put(nanoleaf, url_ex, jsonObject.ToString(Formatting.None));
        }

        public Dictionary<string, string> On(Nanoleaf nanoleaf)
        {
            Dictionary<string, int> settings = new Dictionary<string, int>(){
                { "on", 1 }
            };
            return SetState(nanoleaf, settings, 1000);
        }

        public Dictionary<string, string> Off(Nanoleaf nanoleaf)
        {
            Dictionary<string, int> settings = new Dictionary<string, int>(){
                { "on", 0 }
            };
            return SetState(nanoleaf, settings, 1000);
        }

        public Dictionary<string, string> RandomStatic(Nanoleaf nanoleaf)
        {
            Dictionary<string, int> settings = PaletteController.Random().HSBtoDict();
            return SetState(nanoleaf, settings, 1000);
        }

        public Tuple<string, string> CreateCustom(Nanoleaf nanoleaf, JArray palette, int transMin=5, int transMax=5, string animType = "flow")
        {
            dynamic write = new JObject();
            write.command = "display";
            write.animType = animType;
            write.colorType = "HSB";
            write.animData = null;
            write.loop = true;
            write.palette = palette;
            dynamic transTime = new JObject();
            transTime.maxValue = transMax;
            transTime.minValue = transMin;
            write.transTime = transTime;

            dynamic jsonObject = new JObject();
            jsonObject.write = write;

            string url_ex = "/effects";

            return PutAndForget(nanoleaf, url_ex, jsonObject.ToString(Formatting.None));
        }

        public Tuple<string, string> CreateTest(Nanoleaf nanoleaf, JArray palette, int transMin, int transMax, string animType = "flow")
        {
            dynamic write = new JObject();
            write.command = "display";
            write.animType = animType;
            write.colorType = "HSB";
            write.animData = null;
            write.loop = true;
            write.palette = palette;
            dynamic transTime = new JObject();
            transTime.maxValue = transMax;
            transTime.minValue = transMin;
            write.transTime = transTime;

            dynamic jsonObject = new JObject();
            jsonObject.write = write;

            string url_ex = "/effects";

            //SendLightDelegate putUpdate = PutAndForget;
            //FireAndForget(putUpdate, url_ex, jsonObject.ToString(Formatting.None)); Thread.Sleep(300);
            PutAndForget(nanoleaf, url_ex, jsonObject.ToString(Formatting.None));
            return new Tuple<string, string>(url_ex, jsonObject.ToString(Formatting.None));
        }

        public Tuple<string, string> CreateTemp(Nanoleaf nanoleaf, List<Panel> panels, List<LCColor> colors)
        {
            dynamic write = new JObject();
            write.command = "display";
            write.animType = "static";

            string animData = String.Format("{0} ", panels.Count);
            int indexer = 0;

            foreach (LCColor color in colors)
            {
                panels[indexer].Color = color;
                //string format is the panelId, frame, rgbwt string
                animData += String.Format("{0} {1} {2} ", panels[indexer].panelId, 10, color.RGBWT);
                indexer += 1;
            }

            write.animData = animData;

            dynamic jsonObject = new JObject();
            jsonObject.write = write;

            string url_ex = "/effects";
            
            PutAndForget(nanoleaf, url_ex, jsonObject.ToString(Formatting.None));
            return new Tuple<string, string>(url_ex, jsonObject.ToString(Formatting.None));
        }

        public Dictionary<string, string> CreateCustomRhythm(Nanoleaf nanoleaf, JArray palette, int brightness)
        {
            dynamic pluginRequest = new JObject();
            pluginRequest.command = "request";
            pluginRequest.animName = "Paint Splatter";
            dynamic jsonObject = new JObject();
            jsonObject.write = pluginRequest;
            JObject effect = JObject.FromObject(Put(nanoleaf, "/effects", jsonObject.ToString(Formatting.None)));
            JToken body = JToken.Parse(effect["body"].ToString());
            string pluginUuid = body["pluginUuid"].ToString();

            //get the rhythm/plugin uuid of the effect to overwrite, do not need to send the 'animName' in this update
            dynamic write = new JObject();
            write.command = "display";
            write.pluginType = "rhythm";
            write.pluginUuid = pluginUuid;
            write.animType = "plugin";
            write.colorType = "HSB";
            write.animData = null;
            write.loop = true;
            write.flowFactor = 0;
            write.explodeFactor = 0.5;
            write.palette = palette;

            jsonObject = new JObject();
            jsonObject.write = write;

            string url_ex = "/effects";

            return Put(nanoleaf, url_ex, jsonObject.ToString(Formatting.None));
        }

        public void RainbowShift(Nanoleaf nanoleaf, int delay, int gap, int brightness)
        {
            Dictionary<string, int> settings = new Dictionary<string, int>(){
                    { "hue", 360 },
                    { "sat", 100 },
                    { "brightness", brightness }
                };
            SetState(nanoleaf, settings, delay);

            int value = 360;
            while (true)
            {
                settings = new Dictionary<string, int>(){
                    { "hue", value }
                };
                SetState(nanoleaf, settings, delay);
                value -= gap;
                Thread.Sleep(delay);
                if (value <= 0) value = 360 + value;
            }
        }
    }
}
