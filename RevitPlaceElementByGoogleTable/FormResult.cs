using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RevitPlaceElementByGoogleTable
{
    public partial class FormResult : Form
    {
        public FormResult(List<string> info)
        {
            InitializeComponent();

            foreach(string line in info)
            {
                richTextBox1.Text += line + System.Environment.NewLine;
            }
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
