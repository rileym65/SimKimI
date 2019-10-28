using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace SimKimI
{
    public class SevenSegmentDisplay : PictureBox
    {
        protected int onSegments;
        protected int lastSegments;
        protected int segmentWidth;
        protected int segmentHeight;

        public SevenSegmentDisplay()
        {
            onSegments = 0x00;
            lastSegments = 0x00;
            Image = new Bitmap(this.Width, this.Height);
            segmentWidth = Width / 10;
            segmentHeight = Height / 20;
            SizeChanged += new EventHandler(sizeChangedHandler);
            Draw();
        }

        protected void sizeChangedHandler(object sender,EventArgs e)
        {
            Image = new Bitmap(this.Width, this.Height);
            segmentWidth = (int)(Width / 10.0);
            segmentHeight = (int)(Height / 16.0);
            SizeChanged += new EventHandler(sizeChangedHandler);
            Draw();
        }

        public void Draw()
        {
            Graphics gc;
            Pen pen;
            Brush brush;
            Brush on;
            Brush off;
            gc = Graphics.FromImage(Image);
            brush = Brushes.Black;
            on = Brushes.Red;
            off = new SolidBrush(Color.FromArgb(70, 0, 0));
            gc.FillRectangle(brush, 0, 0, Width, Height);
            gc.FillRectangle((((onSegments & 1) == 1) ? on : off), segmentWidth * 2, segmentHeight, segmentWidth * 6, segmentHeight);
            gc.FillRectangle((((onSegments & 2) == 2) ? on : off), segmentWidth * 8, segmentHeight * 2, segmentWidth, segmentHeight * 6);
            gc.FillRectangle((((onSegments & 4) == 4) ? on : off), segmentWidth * 8, segmentHeight * 9, segmentWidth, segmentHeight * 6);
            gc.FillRectangle((((onSegments & 8) == 8) ? on : off), segmentWidth * 2, segmentHeight * 15, segmentWidth * 6, segmentHeight);
            gc.FillRectangle((((onSegments & 16) == 16) ? on : off), segmentWidth * 1, segmentHeight * 9, segmentWidth, segmentHeight * 6);
            gc.FillRectangle((((onSegments & 32) == 32) ? on : off), segmentWidth * 1, segmentHeight * 2, segmentWidth, segmentHeight * 6);
            gc.FillRectangle((((onSegments & 64) == 64) ? on : off), segmentWidth * 2, segmentHeight * 8, segmentWidth * 6, segmentHeight);
            gc.Dispose();
            off.Dispose();
            lastSegments = onSegments;
        }

        public void Value(int segments)
        {
            onSegments = segments;
            if (onSegments != lastSegments) Draw();
            this.Invalidate();
        }

    }
}
