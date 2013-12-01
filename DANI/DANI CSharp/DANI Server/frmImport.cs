using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DANI_Server
{
    public partial class frmImport : Form
    {
        public String FileName ;
        public int MaxSentences;

        public frmImport()
        {
            InitializeComponent();
        }

        private void OK_Button_Click(object sender, EventArgs e)
        {
            txtFileName.Text = txtFileName.Text.Trim();
            if (!System.IO.File.Exists(txtFileName.Text))
            {
                MessageBox.Show("Invalid File");
                return;
            }
            FileName = txtFileName.Text;
            if (radAll.Checked)
                MaxSentences = -1;
            else
                MaxSentences = Convert.ToInt32(NumericUpDown1.Value);
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void Cancel_Button_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                return;
            this.txtFileName.Text = ofd.FileName;
        }

    }
}
