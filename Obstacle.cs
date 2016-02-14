using System;
using System.Drawing;

namespace Sailing
{
    struct Shape
    {
        public Point[] p;
        public SolidBrush b;
        public Shape(Point[] points) {
            p = points;
            b = new SolidBrush(Color.White);
        }
        public void Draw(Graphics g) { g.FillPolygon(b, p); }
    }

    abstract class Obstacle : Content
    {
        public static readonly Color shoalColor = Color.FromArgb(127,185,194);
        public Shape s;
        public Obstacle(ContentManager m, Point[] points): base(m) {
            s = new Shape(points);
        }
        public override void Draw(Graphics g, float t) { s.Draw(g); }
    }

    class Land : Obstacle
    {
        static readonly Pen shoalPen = new Pen(Obstacle.shoalColor, 5f);
        public Land(ContentManager m, Point[] p): base(m, p) {
            s.b.Color = Color.FromArgb(170, 150, 80);
        }
        public override void Draw(Graphics g, float t) {
            base.Draw(g, t);
            g.DrawPolygon(shoalPen, s.p);
        }
    }

    class Shoal : Obstacle
    {
        public Shoal(ContentManager m, Point[] p): base(m, p) {
            s.b.Color = shoalColor;
        }
    }
}