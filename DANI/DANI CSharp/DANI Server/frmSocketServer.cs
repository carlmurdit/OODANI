using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;


namespace DANI_Server
{
    public partial class frmSocketServer : Form
    {
        List<IPAddress> IPAddresses = new List<IPAddress>();
        public IPAddress IPAddress;
        public int Port = 11000;

        public frmSocketServer()
        {
            InitializeComponent();

            foreach (NetworkInterface adapter in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
            {
                IPInterfaceProperties ipProps = adapter.GetIPProperties();
                foreach (UnicastIPAddressInformation ipInfo in ipProps.UnicastAddresses)
                {
                    if ((adapter.OperationalStatus == OperationalStatus.Up) && (ipInfo.Address.AddressFamily == AddressFamily.InterNetwork))
                    {
                        IPAddresses.Add(ipInfo.Address);
                        lstIPAddresses.Items.Add(String.Format("{0}\t{1}\t{2}", ipInfo.Address, adapter.Name, adapter.Description));
                        if (adapter.Name.ToLower().Contains("wi-fi")) { 
                            lstIPAddresses.Tag = lstIPAddresses.Items.Count-1; 
                        }
                    }
                }
            }
            if (lstIPAddresses.Tag != null) { lstIPAddresses.SelectedIndex = (int)lstIPAddresses.Tag; } //set wifi as default
            btnOK.Focus();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (this.lstIPAddresses.SelectedIndex == -1) { return; }
            IPAddress = IPAddresses[this.lstIPAddresses.SelectedIndex];
            Port = (int) numericUpDown1.Value;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
