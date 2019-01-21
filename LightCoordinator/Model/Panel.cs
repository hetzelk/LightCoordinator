using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightCoordinator.Model
{
    public class Panel
    {
        public int panelId { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public int o { get; set; }
        private LCColor color;
        public LCColor previousColor { get; set; }
        public int[] partnerIds { get; set; }

        public LCColor Color
        {
            get
            {
                return color;
            }
            set
            {
                previousColor = color;
                color = value;
            }
        }

        public Panel()
        {

        }

        public Panel(int panelId, int x, int y, int o)
        {
            this.panelId = panelId;
            this.x = x;
            this.y = y;
            this.o = o;
        }

        public static List<Panel> GetPanels(string body)
        {
            JToken panelData = JObject.Parse(body)["positionData"];
            List<Panel> panels = new List<Panel>();
            foreach (JToken p in panelData)
            {
                Panel panel = new Panel();
                panel.panelId = Int32.Parse(p["panelId"].ToString());
                panel.x = Int32.Parse(p["x"].ToString());
                panel.y = Int32.Parse(p["y"].ToString());
                panel.o = Int32.Parse(p["o"].ToString());

                panels.Add(panel);
            }

            return panels;
        }

        public static List<Panel> Shuffle(List<Panel> panels)
        {
            List<Panel> newOrder = new List<Panel>();
            Random random = new Random();

            while (newOrder.Count != panels.Count)
            {
                int i = random.Next(0, panels.Count);
                if (!newOrder.Contains(panels[i]))
                {
                    newOrder.Add(panels[i]);
                }
            }

            return newOrder;
        }
    }
}
