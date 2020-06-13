using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
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
            //检查身份证号是否为18位纯数字（最后一位可以是X）
            if (textBox3.Text.Length != 18)
            {
                MessageBox.Show("身份证号应为18位数字");
                comboBox2.Focus();
                return;
            }
            else
            {
                byte tempbyte;
                for (int i = 0; i < 17; i++) 
                {
                    tempbyte = Convert.ToByte(textBox3.Text[i]);
                    if (tempbyte < 48 || tempbyte > 57)
                    {
                        MessageBox.Show("身份证号应为18位数字");
                        comboBox2.Focus();
                        return;
                    }
                }
                tempbyte = Convert.ToByte(textBox3.Text[17]);
                if(tempbyte!=88 && (tempbyte < 47 || tempbyte > 57))
                {
                    MessageBox.Show("身份证号应为18位数字");
                    comboBox2.Focus();
                    return;
                }

            }
            //至此，检查完毕，信息无误，将信息以空格做分割，传到外层FormClient中，导入数据库
            this.Text = textBox1.Text + " " + dateTimePicker1.Value.ToString("yyyy-mm-dd") + " " 
                        + textBox3.Text + " " + status + " " + dept_name + " ";
            MessageBox.Show("添加成功！");
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
                    this.status = "计算机科学与技术学院";
                    break;
                default:
                    this.status = null;
                    break;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
