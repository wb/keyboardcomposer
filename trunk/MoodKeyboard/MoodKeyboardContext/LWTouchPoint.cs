using System;
using System.Runtime.Serialization;

namespace LWEvent
{
    public class LWTouchPoint
    {
        public double x;
        public double y;

        public LWTouchPoint(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public String Serialize()
        {
            String s = String.Format("{0:G},{1:G}", x, y);
            return s;
        }

        public static LWTouchPoint Deserialize(String s)
        {
            String[] parts = s.Split(',');
            return new LWTouchPoint(Double.Parse(parts[0]), Double.Parse(parts[1]));
        }
    };
}