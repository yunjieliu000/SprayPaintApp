using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace SprayPaintApp
{
    public class SprayPoint
    {
        public double X {  get; set; }
        public double Y { get; set; }
        public Point Coordinates {  get; set; }
        public String PointColor { get; set; }

        public SprayPoint(double x, double y, String pointColor)
        {
            X = x;
            Y = y;
            Coordinates = new Point(X, Y);
            PointColor = pointColor;
        }
    }
}
