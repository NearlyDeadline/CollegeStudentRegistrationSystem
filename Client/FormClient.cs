using MySqlX.XDevAPI.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Windows.Forms;

namespace Client
{
    public partial class FormClient : System.Windows.Forms.Form
    {
        public int UserType = 0;        //用户类型 0学生 1教授 2管理员

        private String UserTypeName = String.Empty;

        public Boolean Selecting = true;
        public FormClient()
        {
            InitializeComponent();
            InitializeConnection();
            //在此添加更多的其他初始化函数
            this.tabControl1.TabPages.Clear();
            this.tabControl1.TabPages.Add(this.LoginTabpage);
            this.UserTypeComboBox.SelectedIndex = 0;
        }

        private void FormClient_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
        {
            
        }

        private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if(Selecting == false)
            {
                e.Cancel = true;
            }
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
            if (log())
            {
                this.UserType = this.UserTypeComboBox.SelectedIndex;
                this.tabControl1.TabPages.Clear();
                switch (this.UserType)
                {
                    case 0:
                        this.tabControl1.TabPages.Add(this.PersonalInformationTabPage);
                        this.ShowTabPage1.Text = "查看已选课程";
                        this.tabControl1.TabPages.Add(this.ShowTabPage1);
                        this.ShowTabPage2.Text = "查看课程信息";
                        this.tabControl1.TabPages.Add(this.ShowTabPage2);
                        this.tabControl1.TabPages.Add(this.NotificationTabPage);
                        this.tabControl1.TabPages.Add(this.RegisterCoursesTabPage);
                        break;
                    case 1:
                        this.tabControl1.TabPages.Add(this.PersonalInformationTabPage);
                        this.tabControl1.TabPages.Add(this.TeachCourseManageTabPage);
                        this.ShowTabPage1.Text = "以往教授课程";
                        this.tabControl1.TabPages.Add(this.ShowTabPage1);
                        this.tabControl1.TabPages.Add(this.WatingForGradeTabPage);
                        break;
                    case 2:
                        this.tabControl1.TabPages.Add(this.ProfessorInformationTabPage);
                        this.tabControl1.TabPages.Add(this.StudentInformationTabPage);
                        this.tabControl1.TabPages.Add(this.SystemManageTabPage);
                        break;
                }
            }
            else
            {
                if (ResultBuffer.Length == 0)
                    ResultBuffer = "登录失败";
                MessageBox.Show(ResultBuffer, "无效登录", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private bool log()
        {
            ResultBuffer = String.Empty;
            string message = String.Format("login@{0},{1},{2}", UserTypeName, textBoxLoginName.Text, textBoxLoginPassword.Text);
            GetResultToBuffer(message);
            if (ResultBuffer.Equals("True"))
                return true;
            else
                return false;
        }

        private void UserTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (this.UserTypeComboBox.SelectedIndex)
            {
                case 0:
                    this.UserTypeName = "student";
                    break;
                case 1:
                    this.UserTypeName = "professor";
                    break;
                case 2:
                    this.UserTypeName = "registrar";
                    break;
            }
        }
    }
}
