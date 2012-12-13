using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ProjectXenix
{
    public partial class XenixMain : Form
    {
        private KinectController kinectController;
        private AIModule aim;

        public XenixMain()
        {
            InitializeComponent();

            this.aim = new AIModule();
            this.kinectController = new KinectController(aim);
            this.Resize += new EventHandler(XenixMain_Resize);
        }

        private void XenixMain_Resize(object sender, System.EventArgs e)
        {
            if (FormWindowState.Minimized == WindowState)
            {
                Hide();
                notifyIcon1.Visible = true;
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }
    }
}
