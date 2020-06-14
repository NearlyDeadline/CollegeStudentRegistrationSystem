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
    public partial class StuInfo : Form
    {
        string status;//学生学历
        string graduate_date;//教授所属学院
        public StuInfo()
        {
            InitializeComponent();
        }
        private void button重置_Click(object sender, EventArgs e)
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
                    else if (control is ComboBox)
                    {
                        ((ComboBox)control).Text = "<请选择>";
                    }
                }
            }
        }

        private void button添加_Click(object sender, EventArgs e)
        {
            //用户点击添加按钮，此时所有项都应填写完毕，首先检查文本框是否有空余项
            if (textBox学生姓名.Text == String.Empty)
            {
                MessageBox.Show("请填写学生姓名");
                textBox学生姓名.Focus();
                return;
            }
            if (textBox身份证号.Text == String.Empty)
            {
                MessageBox.Show("请填写学生身份证号");
                textBox身份证号.Focus();
                return;
            }
            //检查下拉选单是否有未选择的项
            if (status == null)
            {
                MessageBox.Show("请选择一个学历");
                comboBox学历.Focus();
                return;
            }
            if (graduate_date == null)
            {
                MessageBox.Show("请选择一个毕业年份");
                comboBox毕业年份.Focus();
                return;
            }
            //检查身份证号是否为9位纯数字
            if (textBox身份证号.Text.Length != 9)
            {
                MessageBox.Show("身份证号应为9位纯数字");
                textBox身份证号.Focus();
                return;
            }
            else
            {
                byte tempbyte;
                for (int i = 0; i < 9; i++)
                {
                    tempbyte = Convert.ToByte(textBox身份证号.Text[i]);
                    if (tempbyte < 48 || tempbyte > 57)
                    {
                        MessageBox.Show("身份证号应为9位纯数字");
                        textBox身份证号.Focus();
                        return;
                    }
                }

            }
            //至此，检查完毕，信息无误，将信息以空格做分割，传到外层FormClient中，导入数据库
            this.Text = textBox学生姓名.Text + " " + dateTimePicker1.Value.ToString("yyyy-MM-dd") + " "
                        + textBox身份证号.Text + " " + status + " " + graduate_date;
            this.Close();
        }

        private void comboBox学历_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (this.comboBox学历.SelectedIndex)
            {
                case 0:
                    this.status = "本科生";
                    break;
                case 1:
                    this.status = "硕士研究生";
                    break;
                case 2:
                    this.status = "博士研究生";
                    break;
                default:
                    this.status = null;
                    break;
            }
        }

        private void comboBox毕业年份_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (this.comboBox毕业年份.SelectedIndex)
            {
                case 0:
                    this.graduate_date = "2020";
                    break;
                case 1:
                    this.graduate_date = "2021";
                    break;
                case 2:
                    this.graduate_date = "2022";
                    break;
                case 3:
                    this.graduate_date = "2023";
                    break;
                case 4:
                    this.graduate_date = "2024";
                    break;
                default:
                    this.graduate_date = null;
                    break;
            }
        }

        private void button取消_Click(object sender, EventArgs e)
        {
            this.Text = "no";
            this.Close();
        }

        public void update(string name,string date,string ssn,int status,int graduate_date)
        {
            date = date.Substring(0, 9);
            string[] dat = Regex.Split(date, "/", RegexOptions.IgnoreCase);
            this.textBox学生姓名.Text = name;
            this.textBox身份证号.Text = ssn;
            this.comboBox学历.SelectedIndex = status;
            this.comboBox毕业年份.SelectedIndex = graduate_date;
            this.dateTimePicker1.Value = new DateTime(int.Parse(dat[0]), int.Parse(dat[1]), int.Parse(dat[2]));
            this.button重置.Visible = false;
            this.label提供信息提示.Text = "请修改学生信息：";
            this.button添加.Text = "更新";
            this.Text = "更新学生";
        }

        private void StuInfo_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Text = "no";//用来标记放弃本次更改
        }
    }
}
