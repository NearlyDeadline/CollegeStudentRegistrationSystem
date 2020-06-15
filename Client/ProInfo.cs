using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class ProInfo : Form
    {
        string status;//教授职称
        string dept_name;//教授所属学院
        public ProInfo()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            //重置按钮，会弹出确认选框
            DialogResult result = MessageBox.Show("确定要重置所有信息？", "确认提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            //将各文本框内容清空
            if (result == DialogResult.OK) 
            {
                foreach (Control control in this.Controls)
                {
                    if (control is TextBox)
                    {
                        ((TextBox)control).Clear();
                    }
                    else if(control is ComboBox)
                    {
                        ((ComboBox)control).Text = "<请选择>";
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //用户点击添加按钮，此时所有项都应填写完毕，首先检查文本框是否有空余项
            if (textBox1.Text == String.Empty)
            {
                MessageBox.Show("请填写教授姓名");
                textBox1.Focus();
                return;
            }
            if (textBox3.Text == String.Empty)
            {
                MessageBox.Show("请填写教授身份证号");
                textBox3.Focus();
                return;
            }
            //检查下拉选单是否有未选择的项
            if (status == null)
            {
                MessageBox.Show("请选择一个职称");
                comboBox1.Focus();
                return;
            }
            if (dept_name == null)
            {
                MessageBox.Show("请选择一个学院");
                comboBox2.Focus();
                return;
            }
            //检查身份证号是否为9位纯数字
            if (textBox3.Text.Length != 9)
            {
                MessageBox.Show("身份证号应为9位纯数字");
                textBox3.Focus();
                return;
            }
            else
            {
                byte tempbyte;
                for (int i = 0; i < 9; i++) 
                {
                    tempbyte = Convert.ToByte(textBox3.Text[i]);
                    if (tempbyte < 48 || tempbyte > 57)
                    {
                        MessageBox.Show("身份证号应为9位纯数字");
                        textBox3.Focus();
                        return;
                    }
                }

            }
            //至此，检查完毕，信息无误，将信息以空格做分割，传到外层FormClient中，导入数据库
            this.Text = textBox1.Text + " " + dateTimePicker1.Value.ToString("yyyy-MM-dd") + " " 
                        + textBox3.Text + " " + status + " " + dept_name;
            this.Close();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (this.comboBox1.SelectedIndex)
            {
                case 0:
                    this.status = "助教";
                    break;
                case 1:
                    this.status = "讲师";
                    break;
                case 2:
                    this.status = "副教授";
                    break;
                case 3:
                    this.status = "教授";
                    break;
                default:
                    this.status = null;
                    break;
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (this.comboBox2.SelectedIndex)
            {
                case 0:
                    this.dept_name = "计算机科学与技术学院";
                    break;
                case 1:
                    this.dept_name = "数学学院";
                    break;
                case 2:
                    this.dept_name = "外语学院";
                    break;
                case 3:
                    this.dept_name = "马克思学院";
                    break;
                default:
                    this.dept_name = null;
                    break;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Text = "no";
            this.Close();
        }
        public void update(string name, string date, string ssn, int status, int dept_name)
        {
            date = date.Substring(0, 9);
            string[] dat = Regex.Split(date, "/", RegexOptions.IgnoreCase);
            this.textBox1.Text = name;
            this.textBox3.Text = ssn;
            this.comboBox1.SelectedIndex = status;
            this.comboBox2.SelectedIndex = dept_name;
            this.dateTimePicker1.Value = new DateTime(int.Parse(dat[0]), int.Parse(dat[1]), int.Parse(dat[2]));
            this.button1.Visible = false;
            this.label5.Text = "请修改教授信息：";
            this.button2.Text = "更新";
            this.Text = "更新教授";
        }
    }
}
