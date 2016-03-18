using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WCFRawTcpTest_libss;

namespace WCFRawTcpTest_sswinform
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var proxy = new ProxyServer(
                "127.0.0.1",
                int.Parse(textBox5.Text),
                textBox1.Text,
                int.Parse(textBox2.Text),
                textBox3.Text,
                textBox4.Text);
            proxy.Open();

            button1.Text = "Running";
        }
    }
}
