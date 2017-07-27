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
    public partial class Dashboard : Form
    {
        public Dashboard()
        {
            InitializeComponent();
            System.Windows.Forms.Timer timer1 = new System.Windows.Forms.Timer();
            timer1.Interval = 10000; //10 seconds
            timer1.Tick += new System.EventHandler(timer1_Tick);
            timer1.Start();
            populateDataTable();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //do whatever you want 
            populateDataTable();
        }

        private void populateDataTable(){
            dataGridView1.Rows.Clear();
            dataGridView1.AllowUserToAddRows = false;
            string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "/Service Instances.xml";
            StringBuilder result = new StringBuilder();
            foreach (XElement level2Element in XElement.Load(path).Elements("DTUInstance"))
            {
                var name = level2Element.Attribute("dtuname").Value;
                String[] args = { level2Element.Attribute("IP").Value, level2Element.Attribute("servicename").Value, "/GETSTATUS", "/user:" + level2Element.Attribute("user").Value, "/password:" + level2Element.Attribute("password").Value, "/domain:" + level2Element.Attribute("domain").Value };
                string status = ServiceClass.ExecuteService(args);
                string action = "Start";
                string restart = "";
                string pause = "Pause";
                if (status.ToUpper().Equals("PAUSED"))
                {
                    pause = "Continue";
                }
                if (status.ToUpper().Equals("RUNNING"))
                {
                    action = "Stop";
                    restart = "Restart";
                }
                else
                {
                    pause = "";
                }
                if (status.ToUpper().Contains("NOT FOUND"))
                {
                    action = ""; 
                    status = "SERVICE NOT FOUND"; 
                    pause = "";
                    restart = "";
                }
                dataGridView1.Rows.Add(new Object[] { name, level2Element.Attribute("IP").Value, level2Element.Attribute("domain").Value, status, action, pause, restart});
            }
        }
        private void button1_Click_1(object sender, EventArgs e)
        {
            AddService form1 = new AddService();
            form1.Tag = this;
            form1.Show(this);
            Hide();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var index = e.ColumnIndex;
            if (index < 4 || index > 6)
            {
                return;
            }
            var dtuname = dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString(); ;
                        string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "/DTUInstances.xml";
            StringBuilder result = new StringBuilder();
            XDocument doc = XDocument.Load(path);
            XElement users = doc.Root.Elements("DTUInstance").Where(el => el.Attribute("dtuname").ToString().Contains(dtuname)).FirstOrDefault();
            String[] args = { users.Attribute("IP").Value, users.Attribute("servicename").Value, "/GETSTATUS", "/user:" + users.Attribute("user").Value, "/password:" + users.Attribute("password").Value, "/domain:" + users.Attribute("domain").Value };
            string status = ServiceClass.ExecuteService(args).ToUpper();
            int error = 0;
            if (status.ToUpper().Contains("NOT FOUND"))
            {
                MessageBox.Show("Service could not be found in the target machine.", "Action failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (index == 4) { // Start/Stop
                if (status == "RUNNING"){
                    args[2] = "/STOP";       
                }else{
                    args[2] = "/START";
                }
            } 
            else if( index == 5) {  // Pause/Continue
                if (status == "PAUSED"){
                    args[2] = "/CONTINUE";
                }else if(status == "RUNNING"){
                    args[2] = "/PAUSE";
                } else{
                    error = 1;
                }
            }
            else if (index == 6) // Restart
            {
                if (status == "RUNNING"){
                    args[2] = "/RESTART";
                }
                else
                {
                    error = 2;
                }
            }
            if (error == 0){
                try
                {
                    ServiceClass.ExecuteService(args);
                }
                catch(Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }

            }else if(error == 1){
                MessageBox.Show("You cannot pause/unpause a stopped service; it has to be running.", "Action failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            } else if(error == 2){
                MessageBox.Show("You cannot restart a stopped service; it has to be running.", "Action failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            populateDataTable();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}