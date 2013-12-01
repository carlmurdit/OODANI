using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows;
using System.ComponentModel;
using System.Windows.Forms;
using DANI_Server.Word_Processing;
using DANI_Server.File_Parsing;

namespace DANI_Server
{
    public partial class frmMain : Form
    {
        
        private cWordProcessor WordProcessor = new cWordProcessor();
        SocketServer.SocketServer server = null;

        public frmMain()
        {
            InitializeComponent();
            this.richText.ReadOnly = true;
            WordProcessor = new cWordProcessor();
            WordProcessor.Alert += WordProcessor_Alert;
            pictureBox1.Image = imageList1.Images[0]; //start icon
            mnuStartServer.Image = imageList1.Images[0]; 
            lblServerStatus.Text = "Socket Server not running. Click to start.";
        }

        private bool StopImport = false ;
        private void mnuImport_Click_1(object sender, EventArgs e)
        {
            frmImport frmImport = new frmImport();
            if (!(frmImport.ShowDialog() == System.Windows.Forms.DialogResult.Cancel))
            {
                cFileParser fp = new DANI_Server.File_Parsing.cFileParser();
                List<string> Sentences = fp.Parse(frmImport.FileName, -1, frmImport.MaxSentences);
                ProgressBarImport.Value = 0;
                ProgressBarImport.Maximum = Sentences.Count * 2;
                pnlImport.BringToFront();
                foreach (string sentence in Sentences)
                {
                    Application.DoEvents();
                    if (StopImport) break;
                    AppendMessage(sentence, Color.Black); //show all sentences together
                    ProgressBarImport.Value += 1; 
                }
                foreach (string sentence in Sentences)
                {
                    Application.DoEvents();
                    if (StopImport) break;
                    WordProcessor.Process(sentence, false);
                    ProgressBarImport.Value += 1;
                }
                pnlStatus.BringToFront();
                lblKnownWords.Text = "Known Words: " + WordProcessor.KnownWords();
                StopImport = false; //reset flag
            }
            txtInput.Text = "";
            txtInput.Focus();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            StopImport = true;
        }

        private void mnuStartServer_Click(object sender, EventArgs e)
        {
            if (server == null)
            {
                //start it
                frmSocketServer frm = new frmSocketServer();
                if (frm.ShowDialog() == DialogResult.Cancel)
                    return;
                server = new SocketServer.SocketServer();
                server.Start(frm.IPAddress, WordProcessor, frm.Port);
                pictureBox1.Image = imageList1.Images[1]; //stop icon
                mnuStartServer.Image = imageList1.Images[1];
                lblServerStatus.Text = string.Format("Server listening on {0}, port {1}.", frm.IPAddress, frm.Port);
                mnuStartServer.Text = "Stop Socket Server";
            }
            else
            {
                //stop it
                if (MessageBox.Show("Server is running. Stop it?", "Socket Server", MessageBoxButtons.YesNo) == DialogResult.No)
                    return;
                server.Stop();
                server = null;
                pictureBox1.Image = imageList1.Images[0]; //start icon
                mnuStartServer.Image = imageList1.Images[0];
                lblServerStatus.Text = "Socket Server not running. Click to start.";
                mnuStartServer.Text = "Start Socket Server";
            }
        }

        private void tsbErase_Click(object sender, EventArgs e)
        {
            richText.Text = "";
        }

        private void WordProcessor_Alert(string msg, cWordProcessor.eProcessType ProcessType) //display messages from word processor
        {
            if (ProcessType == cWordProcessor.eProcessType.Responded)
                UpdateWordCount();
            else
            {
                if (ProcessType == cWordProcessor.eProcessType.Learning)
                    AppendMessage(msg, Color.Green);
                else
                    AppendMessage(msg, Color.Blue);
            }
        }

        private void UpdateWordCount()
        {
            if (lblKnownWords .InvokeRequired == true)
            {
                this.Invoke((Action)(() => UpdateWordCount()));
                return;
            }
            lblKnownWords.Text = "Known Words: " + WordProcessor.KnownWords();
        }

        public void AppendMessage(string msg, System.Drawing.Color color)
        {

            if (richText.InvokeRequired == true)
            {
                this.Invoke((Action)(() => AppendMessage(msg, color)));
                return;
            }

            richText.SelectionStart = richText.Text.Length;
            richText.SelectionColor = color;
            richText.AppendText(msg + "\n");
            richText.ScrollToCaret();
        }

        private void txtInput_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter & txtInput.Text.Trim().Length > 0)
            {
                WordProcessor.Process(txtInput.Text, true);
                txtInput.Text = "";
                txtInput.Focus();
            }
        }

    }
}
