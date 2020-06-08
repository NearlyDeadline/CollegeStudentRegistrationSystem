using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace Client
{
    public partial class FormClient : System.Windows.Forms.Form
    {
        public FormClient()
        {
            InitializeComponent();
            InitializeConnection();
            //在此添加更多的其他初始化函数
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GetResultToBuffer("select * from course;");
            dataGridView1.DataSource = ResultBuffer.ToDataTable();
        }

        private void FormClient_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
        {
            
        }
    }
}
