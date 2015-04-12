using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;

using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UnityFramework.Network;
using UnityEngine;
using System.IO;
using System.Xml;
using UnityFramework.Misc.Pool;
namespace FrameworkTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

		private void Form1_Load(object sender, EventArgs e)
		{

			

		}

        private void connected()
        {
            Console.WriteLine("connected");
        }

		private void button1_Click(object sender, EventArgs e)
		{
			HTTPNetwork.Instance.Get("http://www.163.com", "GET", (NetworkStatusEnum Status, System.Object Data) =>
			{
				string V = System.Text.Encoding.Default.GetString((byte[])Data);

				Console.WriteLine(V);
			});
		}

		private void button2_Click(object sender, EventArgs e)
		{
			Console.WriteLine(ObjectPool.Instance.DumpPoolStatus<HTTPRequest>());
		}
    }

}
