using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.IO;

namespace WindowsFormsApplication1
{
    public partial class AddService : Form
    {
        public AddService()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "/DTUInstances.xml";
            var IP = textBox1.Text;
            var ServiceName = textBox2.Text;
            var user = textBox3.Text;
            var pass = textBox4.Text;
            var domain = textBox5.Text;
            var dtuname = textBox7.Text;
            if (String.IsNullOrEmpty(IP) || String.IsNullOrEmpty(ServiceName) || String.IsNullOrEmpty(user) || String.IsNullOrEmpty(pass) || String.IsNullOrEmpty(domain) || String.IsNullOrEmpty(dtuname))
            {
                MessageBox.Show("All values must be entered.");
                return;
            }
            String[] args = { IP, ServiceName, "/GETSTATUS", "/user:" + user, "/password:" + pass, "/domain:" + domain};
            XDocument doc = XDocument.Load(path);  //+		[5]	{dtuname="ClinicA"}	System.Xml.Linq.XAttribute
            int users = doc.Root.Elements("DTUInstance").Where(el => el.Attribute("dtuname").Value.Equals(dtuname) && el.Attribute("IP").Value.Equals(IP)).Count();
            var user1 = doc.Root.Elements("DTUInstance").Where(el => el.Attribute("dtuname").ToString().Equals(dtuname));
            var user2 = doc.Root.Elements("DTUInstance").Attributes();
            if (users > 0)
            {
                MessageBox.Show("DTU Instance with the same IP and service name already added.", "Action failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                ServiceClass.ExecuteService(args);
                XElement root = new XElement("DTUInstance");
                root.Add(new XAttribute("servicename", ServiceName));
                root.Add(new XAttribute("user", user));
                root.Add(new XAttribute("password", pass));
                root.Add(new XAttribute("domain", domain));
                root.Add(new XAttribute("IP", IP));
                root.Add(new XAttribute("dtuname", dtuname));
                doc.Root.Add(root);
                doc.Save(path); 
            }
            catch(Exception exc)
            {
                MessageBox.Show("ERROR: " + exc.Message, "Action failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            button3.PerformClick();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var form2 = (Dashboard)Tag;
            form2.Show();
            Close();
        }
    }
}
