using System;
using System.Drawing;

namespace Sailing
{
    struct Vector
    {
        double a, l;
        public double x, y;
        public float angleDegree { get { return (float)(180*a/Math.PI); } }
        public double angle { get { return a; }
            set { a = value; if (a<0) a += 2*Math.PI;
                else if (a>2*Math.PI) a -= 2*Math.PI;
                x = Math.Cos(a)*l; y = Math.Sin(a)*l; } }
        public double length { get { return l; }
            set { x *= value / l; y *= value / l; l = value; } }
        public Vector(double X, double Y) { x = X; y = Y; a = 0; l = 1; }
        public void calcLength() { l = Math.Sqrt(x * x + y * y); }
        public void calcAngle() { a = Math.Atan2(y, x); }
        public static double ScalarMult(Vector a, Vector b) {
            return a.x * b.x + a.y * b.y;
        }
        public static int SignCos(Vector a, Vector b) {
            return ScalarMult(a, b) > 0 ? 1 : -1;
        }
        public Vector Projection(Vector to) {
            Vector r;
            r = ScalarMult(this, to) * to.length * to.length * to;
            return r;
        }
        public static Vector operator *(double a, Vector v) {
            return new Vector(a * v.x, a * v.y);
        }
        public static Vector operator +(Vector a, Vector b) {
            return new Vector(a.x + b.x, a.y + b.y);
        }
        public static Vector operator -(Vector a, Vector b) {
            return new Vector(a.x - b.x, a.y - b.y);
        }
    }
}