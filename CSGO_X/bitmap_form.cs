using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CSGO_X
{
    public partial class bitmap_form : Form
    {
        public bitmap_form()
        {
            InitializeComponent();
        }


        private void Draw_Load(object sender, EventArgs e)
        {
            //屏幕大小
            Width = 1920;
            Height = 1080;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                int WS_EX_TOOLWINDOW = 0x80;
                CreateParams CP = base.CreateParams;

                CP.ExStyle = CP.ExStyle | WS_EX_TOOLWINDOW;
                return CP;
            }
        }

        public void Drawing(List<(Rectangle rectangle,Color line_color)> Rectangles) {
            try
            {
                var MyBuff = new BufferedGraphicsContext().Allocate(CreateGraphics(), new Rectangle(Location.X, Location.Y, Width, Height));
                var e = MyBuff.Graphics;
                e.Clear(BackColor);
                
                foreach (var rectangles in Rectangles) {
                    var pen = new Pen(rectangles.line_color, 3);
                    e.DrawRectangle(pen, rectangles.rectangle);
                }
                MyBuff.Render();
                MyBuff.Dispose();
            }
            catch { 
            
            }
        }

    }
}
