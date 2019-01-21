using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightCoordinator.Model
{
    public class Nanoleaf
    {
        public string ip { get; set; }
        public string port { get; set; }
        public string auth_token { get; set; }
        public string url { get; set; }

        public Nanoleaf(string ip, string port)
        {
            this.ip = ip;
            this.port = port;
            this.url = String.Format("http://{0}:{1}/api/v1", ip, port);
        }

        public Nanoleaf(string ip, string port, string auth_token)
        {
            this.ip = ip;
            this.port = port;
            this.auth_token = auth_token;
            this.url = String.Format("http://{0}:{1}/api/v1/{2}", ip, port, auth_token);
        }
    }
}
