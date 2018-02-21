using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Threading;
using System.Drawing.Drawing2D;

namespace Screenshot
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            Thread t1 = new Thread(screenIt);
            t1.IsBackground = true;
            t1.Start();
        }

        public void screenIt()
        {
            while (true)
            {
                var bmpScreenshot = new Bitmap(300, 300, PixelFormat.Format32bppArgb);
                var gfxScreenshot = Graphics.FromImage(bmpScreenshot);
                gfxScreenshot.CopyFromScreen(Screen.AllScreens[1].Bounds.X, Screen.AllScreens[1].Bounds.Height - 300, 0, 0, bmpScreenshot.Size, CopyPixelOperation.SourceCopy);
                Bitmap b = (Bitmap)bmpScreenshot.Clone();
                Image endImg = ClipToCircle(bmpScreenshot);
                pictureBox1.Image = endImg;
                Tuple<int, int, int> avgs = calcRGB(b);
                int avgB = avgs.Item1;
                int avgG = avgs.Item2;
                int avgR = avgs.Item3;
                string txt = "R: " + avgR.ToString() + " G: " + avgG.ToString() + " B: " + avgB.ToString();
                UpdateLabel(txt);
            }
        }

        public Image ClipToCircle(Image srcImage)
        {
            Image dstImage = new Bitmap(srcImage.Width, srcImage.Height, srcImage.PixelFormat);
            using (Graphics g = Graphics.FromImage(dstImage))
            {
                RectangleF r = new RectangleF(0, 0, 300, 300);

                // enables smoothing of the edge of the circle (less pixelated)
                g.SmoothingMode = SmoothingMode.AntiAlias;

                Color background = Color.FromArgb(0, 0, 0, 0);
                // fills background color
                using (Brush br = new SolidBrush(background))
                {
                    g.FillRectangle(br, 0, 0, dstImage.Width, dstImage.Height);
                }

                // adds the new ellipse & draws the image again 
                GraphicsPath path = new GraphicsPath();
                path.AddEllipse(r);
                g.SetClip(path);
                g.DrawImage(srcImage, 0, 0);
                return dstImage;
            }
        }

        private Tuple<int, int, int> calcRGB(Bitmap bm)
        {
            
            BitmapData srcData = bm.LockBits(new Rectangle(0, 0, bm.Width, bm.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            int stride = srcData.Stride;

            IntPtr Scan0 = srcData.Scan0;

            long[] totals = new long[] { 0, 0, 0 };

            int width = bm.Width;
            int height = bm.Height;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        for (int color = 0; color < 3; color++)
                        {
                            int idx = (y * stride) + x * 4 + color;

                            totals[color] += p[idx];
                        }
                    }
                }
            }

            int avgB = Convert.ToInt32(totals[0] / (width * height));
            int avgG = Convert.ToInt32(totals[1] / (width * height));
            int avgR = Convert.ToInt32(totals[2] / (width * height));

            return Tuple.Create(avgB, avgG, avgR);
        }

        private void UpdateLabel(string text)
        {
            if(this.label1.InvokeRequired)
            {
                UpdateLabelCallback d = new UpdateLabelCallback(UpdateLabel);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.label1.Text = text;
            }
        }

        public delegate void UpdateLabelCallback (string text);
        
    }
}
