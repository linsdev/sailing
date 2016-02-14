using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace Sailing
{
    public partial class FormMain : Form
    {
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
        }
        ContentManager m;
        Land[] lands;
        Shoal shoal;
        Sailer boat;
        public FormMain() {
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            LoadMap("Map.txt");
            boat = new Sailer(m, 100f, 450f);
            boat.AddObstacle(shoal, lands);
            m.StartRender();
        }
        bool LoadMap(string fileName) {
            if (!File.Exists(fileName)) {
                MessageBox.Show("File not found or access denied\n" + fileName,
                            "File access error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            StreamReader f = new StreamReader(fileName);
            string l = f.ReadLine();
            uint count, i;
            ClientSize = new Size(int.Parse(l.Split()[0]), int.Parse(l.Split()[1]));
            m = new ContentManager(this, Color.FromArgb(25, 75, 90), true);
            while ((l = f.ReadLine()) != null) {
                switch (l) {
                    case "lands":
                        count = uint.Parse(f.ReadLine());
                        lands = new Land[count];
                        for (i = 0; i < count; i++) lands[i] = new Land(m, ReadPoints(f));
                        break;
                    case "shoal":
                        shoal = new Shoal(m, ReadPoints(f));
                        break;
                    default:
                        MessageBox.Show("Error parsing file\n" + fileName,
                            "Parse error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        f.Close();
                        return false;
                }
            }
            f.Close();
            return true;
        }
        Point[] ReadPoints(StreamReader f) {
            uint count = uint.Parse(f.ReadLine());
            Point[] r = new Point[count];
            string l;
            for (uint i = 0; i < count; i++) {
                l = f.ReadLine();
                string[] c = l.Split();
                r[i].X = int.Parse(c[0]);
                r[i].Y = int.Parse(c[1]);
            }
            return r;
        }
    }
}