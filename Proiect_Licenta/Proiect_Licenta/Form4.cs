using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;

namespace Proiect_Licenta
{
    public partial class Form4 : Form
    {
        public Form4()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        public static string GetUSBSerialNumber(string driveLetter)
        {
            string serialNumber = string.Empty;
            string query = "SELECT * FROM Win32_DiskDrive WHERE InterfaceType='USB'";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);

            foreach (ManagementObject mo in searcher.Get())
            {
                foreach (ManagementObject b in mo.GetRelated("Win32_DiskPartition"))
                {
                    foreach (ManagementBaseObject c in b.GetRelated("Win32_LogicalDisk"))
                    {
                        if (c["DeviceID"].ToString() == driveLetter)
                        {
                            serialNumber = mo["SerialNumber"].ToString();
                            return serialNumber.Trim();
                        }
                    }
                }
            }
            return serialNumber;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string USB_SerialNumber = "6B0EAC4044C9";
            string usbDriveLetter = "D:";
            string serialNumber = GetUSBSerialNumber(usbDriveLetter);
            if(USB_SerialNumber==serialNumber)
            {
                MessageBox.Show("Accesul este permis", "Continuă", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                this.Hide();
                Form2 f = new Form2();
                f.Show();
            }
            else
            {
                MessageBox.Show("USB-ul nu este introdus sau numărul serial al USB-ului nu corespunde", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }
    }
}