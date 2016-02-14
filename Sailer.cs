using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections;

namespace Sailing
{
    class Sailer : Content
    {   // Visual parameter
        const int width = 18;
        const int height = 40;
        const float hWidth = width / 2f;
        const float hHeight = height / 2f;
        const float backWidth = hWidth / 2f;
        const float fronIndent = height / 3.2f;
        const float backIndent = height - fronIndent;
        const float backBulgeIndent = backIndent * (1 - 1/2.3f);
        const float frontBulgeIndent = backWidth * 1.7f;
        const float sailLength = backIndent * 0.9f;
        const float sailWidth = width * 0.1f;
        const float mastsD = sailWidth * 1.3f;
        const float backBulge = backIndent * 1.075f;
        const float rudderLength = 10f;
        const float arrowSize = 10f;
        const float lateralResistance = 0.025f; // Physical parameter
        double simulationSpeed = 50;
        double stepAngleChange = 0.0872664626;
        const double rudderForce = 0.01;
        static readonly Pen darkGray = new Pen(Color.FromArgb(90, 90, 90));
        static readonly Pen rudderPen = new Pen(Color.White, mastsD / 2);
        ContentManager m;
        GraphicsPath boatPath, sailPath, arrowPath;
        Vector boat, boatDirection, rudder, sail, wind;
        double prevBoatDirection;
        Obstacle[] obstacle;
        public Sailer(ContentManager m, float X,float Y): base(m) {
            boat = new Vector(X, Y);
            this.m = m;
            m.KeyDown += onKeyDown;
            boatPath = new GraphicsPath();
            boatPath.AddClosedCurve(new PointF[] {
                new PointF(-backIndent, -backWidth),
                new PointF(-backBulgeIndent, -hWidth),
                new PointF(0, -frontBulgeIndent), new PointF(fronIndent, 0),
                new PointF(0, frontBulgeIndent),
                new PointF(-backBulgeIndent, hWidth),
                new PointF(-backIndent, backWidth), new PointF(-backBulge, 0)
            });
            sailPath = new GraphicsPath();
            sailPath.AddClosedCurve(new PointF[] {
                new PointF(0, 0),
                new PointF(sailWidth*0.75f, sailLength*0.15f),
                new PointF(sailWidth, sailLength/2),
                new PointF(sailWidth*0.75f, sailLength*0.85f),
                new PointF(0, sailLength)
            });
            arrowPath = new GraphicsPath();
            arrowPath.AddLines(new PointF[] {
                new PointF(-4*arrowSize,0),         new PointF(4*arrowSize,0),
                new PointF(2*arrowSize,-arrowSize), new PointF(4*arrowSize,0),
                new PointF(2*arrowSize,arrowSize),  new PointF(4*arrowSize,0)
            });
            boatDirection = new Vector(0, -1);
            boatDirection.calcAngle();
            prevBoatDirection = boatDirection.angle;
            Random r = new Random();
            wind.length = 1;
            wind.angle = r.NextDouble() * 2*Math.PI;
            sail = new Vector(1, 0);
            rudder = new Vector(-rudderLength, 0);
            rudder.calcLength();
            rudder.calcAngle();
        }
        public void AddObstacle(Shoal s, Land[] l) {
            obstacle = new Obstacle[l.Length+1];
            Array.Copy(l, obstacle, l.Length);
            obstacle[l.Length] = s;
        }
        void onKeyDown(KeyEventArgs e) {
            if (m.rendering) { switch (e.KeyCode) {
                case Keys.Q:        sail.angle += stepAngleChange;   break;
                case Keys.W:        sail.angle -= stepAngleChange;   break;
                case Keys.R:        Reset();                         break;
                case Keys.Add:      simulationSpeed *= 1.1;          break;
                case Keys.Subtract: simulationSpeed *= 0.91;         break;
                case Keys.Up:       rudder.angle = Math.PI;          break;
                case Keys.Right:    if (rudder.angle > 0.75*Math.PI)
                                    rudder.angle -= stepAngleChange; break;
                case Keys.Left:     if (rudder.angle < 1.25*Math.PI)
                                    rudder.angle += stepAngleChange; break;
                case Keys.Back:     wind.angle += Math.PI;           break;
            } }
            if (e.KeyCode==Keys.Space) m.rendering = !m.rendering;
        }
        public override void Draw(Graphics g, float t) {
            Vector f = wind.Projection(sail);
            Vector v = f.Projection(boatDirection);
            Vector d = lateralResistance * (f - v);
            boat += simulationSpeed * t * (v + d);
            v.calcLength();
            boatDirection.angle += Vector.SignCos(f, boatDirection) *
                rudderForce * v.length * rudder.y/rudderLength;
            sail.angle += boatDirection.angle - prevBoatDirection;
            g.TranslateTransform((float)boat.x, (float)boat.y);
            g.RotateTransform(boatDirection.angleDegree);
            g.FillPath(Brushes.LightGray, boatPath);
            g.DrawLine(rudderPen, -backBulge,0, (float)rudder.x-backBulge,(float)rudder.y);
            g.RotateTransform(sail.angleDegree-boatDirection.angleDegree);
            g.FillPath(Brushes.White, sailPath);
            g.DrawPath(darkGray, sailPath);
            g.FillEllipse(Brushes.Gray, -mastsD/2,-mastsD/2, mastsD,mastsD);
            g.ResetTransform();
            g.TranslateTransform((float)boat.x, (float)boat.y);
            g.RotateTransform(wind.angleDegree);
            g.DrawPath(Pens.White, arrowPath);
            g.ResetTransform();
            prevBoatDirection = boatDirection.angle;
            if (InsideObstacle()) OnCrash();
            if (boat.x<0 || boat.x>m.width || boat.y<0 || boat.y>m.height) OnCrash();
        }
        void OnCrash() {
            m.rendering = false;
            MessageBox.Show("The boat has crashed", "Game over!",
                MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            Reset();
        }
        void Reset() {
            m.rendering = false;
            do {
                Random r = new Random();
                boat.x = r.Next(m.width);
                boat.y = r.Next(m.height);
                wind.angle = r.NextDouble() * 2 * Math.PI;
            } while (InsideObstacle());
            m.rendering = true;
        }
        bool InsideObstacle() {
            for (int i = 0; i < obstacle.Length; i++)
                if (PointInsidePoligon(new Point((int)boat.x, (int)boat.y),
                    obstacle[i].s.p)) return true;
            return false;
        }
        static bool PointInsidePoligon(Point a, Point[] p) {
            int l = p.Length-1, f = Check(p[l], p[0], a);
            for (int i=0; i<l; i++) f *= Check(p[i], p[i+1], a);
            return f==1 ? false : true;
        }
        static int Check(Point a, Point b, Point m) {
            int ax = a.X-m.X, ay = a.Y-m.Y, bx = b.X-m.X, by = b.Y-m.Y;
            if ((ax | ay) == 0 || (bx | by) == 0) return 0;
            if ((ay | by) == 0) return ((ax ^ bx) >> 31) + 1;
            if ((ay ^ by) >= 0) return 1;
            long n = (long)ax * by - (long)ay * bx;
            return n==0 ? 0 : (((int)(n >> 32) ^ by) >> 30) | 1;
        }
    }
}