using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSGO_X
{

    public partial class Form1 : Form
    {
        public static string PName = "csgo";
        public static string ModuleName = "client_panorama.dll";
        X_Ray x_ray;
        public Form1()
        {
            InitializeComponent();
        }

        public static bitmap_form bitmap_form;
        private void Form1_Load(object sender, EventArgs e)
        {
            bitmap_form = new bitmap_form();
            bitmap_form.Show();
            x_ray = new X_Ray(PName);         
        }     
    }
}
