using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Dyd.BusinessMQ.Test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        int count = 0;
        private void btnSend_Click(object sender, EventArgs e)
        {
            BusinessMQSdkDemo demo = new BusinessMQSdkDemo();
            demo.SendMessageDemo(count + "");
            this.rtbsend.AppendText(count+""+"\n");
            this.rtbReceive.ScrollToCaret();
            count++;
        }

        BusinessMQSdkDemo demo2 = new BusinessMQSdkDemo();
        private void btnReceive_Click(object sender, EventArgs e)
        {
            demo2.ReceiveMessageDemo((msg) =>
            {
                this.rtbReceive.BeginInvoke(new Action(() =>
                {
                    this.rtbReceive.AppendText(msg + "\n");
                    this.rtbReceive.ScrollToCaret();
                }));
            });
            this.btnReceive.Enabled = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            demo2.CloseReceiveMessage();
        }
    }
}
