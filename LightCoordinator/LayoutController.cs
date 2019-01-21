using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightCoordinator
{
    public class LayoutController
    {
        public LayoutController()
        {

        }

        public static void CreateLayout()
        {

        }

        //calling this RotatePoint()
        //List<Panel> newXY = new List<Panel>();
        //    foreach (Panel panel in panels)
        //    {
        //        Point center = new Point(0, 0);
        //Point panelPoint = new Point(panel.x, panel.y);
        //int rotation = -45;
        //Point rotated = LayoutController.RotatePoint(panelPoint, center, rotation);

        //newXY.Add(new Panel(panel.panelId, rotated.X, rotated.Y, rotation));
        //    }
        public static Point RotatePoint(Point pointToRotate, Point centerPoint, double angleInDegrees)
        {
            double angleInRadians = angleInDegrees * (Math.PI / 180);
            double cosTheta = Math.Cos(angleInRadians);
            double sinTheta = Math.Sin(angleInRadians);
            return new Point
            {
                X =
                    (int)
                    (cosTheta * (pointToRotate.X - centerPoint.X) -
                    sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X),
                Y =
                    (int)
                    (sinTheta * (pointToRotate.X - centerPoint.X) +
                    cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y)
            };
        }
    }
}
