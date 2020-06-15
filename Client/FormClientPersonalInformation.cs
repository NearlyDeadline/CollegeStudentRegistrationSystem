using MySql.Data.MySqlClient;
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
    public partial class FormClient
    {
        DataTable dt_per_info;
        private void Show_personal_info()
        {
            MySqlConnection conn = new MySqlConnection(mysqlConnectionString);
            try
            {
                conn.Open();
                if (this.UserType == 0)
                {
                    string sql = "select * from student where id = '" + id + "';";//查询该ID对应的教授信息
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();//此时reader储存了1条语句
                    reader.Read();
                    int status = -1, graduate_date = -1;
                    string ID = reader[0].ToString();
                    string password = reader[6].ToString();

                    if (reader[4].ToString().Equals("本科生"))
                        status = 0;
                    if (reader[4].ToString().Equals("硕士研究生"))
                        status = 1;
                    if (reader[4].ToString().Equals("博士研究生"))
                        status = 2;

                    if (reader[5].ToString().Equals("2020"))
                        graduate_date = 0;
                    if (reader[5].ToString().Equals("2021"))
                        graduate_date = 1;
                    if (reader[5].ToString().Equals("2022"))
                        graduate_date = 2;
                    if (reader[5].ToString().Equals("2023"))
                        graduate_date = 3;
                    if (reader[5].ToString().Equals("2024"))
                        graduate_date = 4;

                    this.textBoxID.Text = reader[0].ToString();
                    this.textBoxID.ReadOnly = true;

                    this.textBoxName.Text = reader[1].ToString();

                    this.textBoxPassword.Text = reader[6].ToString();

                    this.textBoxSsn.Text = reader[3].ToString();

                    string date = reader[2].ToString().Substring(0, 9);
                    string[] dat = Regex.Split(date, "/", RegexOptions.IgnoreCase);
                    this.dateTimePicker1.Value = new DateTime(int.Parse(dat[0]), int.Parse(dat[1]), int.Parse(dat[2]));

                    this.comboBoxStatus.SelectedIndex = status;
                    this.comboBoxStatus.Enabled = false;

                    this.comboBoxGra_year.SelectedIndex = graduate_date;
                    this.comboBoxGra_year.Enabled = false;

                    reader.Close();
                }
                else
                {
                    string sql = "select * from professor where id = '" + id + "';";//查询该ID对应的教授信息
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();//此时reader储存了1条语句
                    reader.Read();
                    int status = -1, dept_name = -1;
                    string ID = reader[0].ToString();
                    string password = reader[6].ToString();

                    if (reader[4].ToString().Equals("助教"))
                        status = 0;
                    if (reader[4].ToString().Equals("讲师"))
                        status = 1;
                    if (reader[4].ToString().Equals("副教授"))
                        status = 2;
                    if (reader[4].ToString().Equals("教授"))
                        status = 3;

                    if (reader[5].ToString().Equals("计算机科学与技术学院"))
                        dept_name = 0;
                    if (reader[5].ToString().Equals("数学学院"))
                        dept_name = 1;
                    if (reader[5].ToString().Equals("外语学院"))
                        dept_name = 2;
                    if (reader[5].ToString().Equals("马克思学院"))
                        dept_name = 3;

                    this.labelStatus.Text = "职称";
                    this.labelGraduateDate.Text = "所属学院";
                    this.comboBoxStatus.Items.Clear();
                    this.comboBoxStatus.Items.AddRange(new object[] {
                    "助教",
                    "讲师",
                    "副教授",
                    "教授"});
                    this.comboBoxGra_year.Items.Clear();
                    this.comboBoxGra_year.Items.AddRange(new object[] {
                    "计算机科学与技术学院",
                    "数学学院",
                    "外语学院",
                    "马克思学院"});

                    this.textBoxID.Text = reader[0].ToString();
                    this.textBoxID.ReadOnly = true;

                    this.textBoxName.Text = reader[1].ToString();

                    this.textBoxPassword.Text = reader[6].ToString();

                    this.textBoxSsn.Text = reader[3].ToString();
                    
                    string date = reader[2].ToString().Substring(0, 9);
                    string[] dat = Regex.Split(date, "/", RegexOptions.IgnoreCase);
                    this.dateTimePicker1.Value = new DateTime(int.Parse(dat[0]), int.Parse(dat[1]), int.Parse(dat[2]));

                    this.comboBoxStatus.SelectedIndex = status;
                    this.comboBoxStatus.Enabled = false;

                    this.comboBoxGra_year.SelectedIndex = dept_name;
                    this.comboBoxGra_year.Enabled = false;

                    reader.Close();
                    }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                conn.Close();//关闭连接
            }
        }

        public void buttonReset_click(object sender, EventArgs e)
        {
            //重置按钮，会弹出确认选框
            DialogResult result = MessageBox.Show("确定要重置所有信息？", "确认提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            //重新调用show函数，将文本框信息复原为初值
            if (result == DialogResult.OK)
            {
                Show_personal_info();
                return;
            }
        }

        public void buttonSave_click(object sender, EventArgs e)
        {
            //保存按钮，会弹出确认选框
            DialogResult result = MessageBox.Show("确定要保存所有信息？", "确认提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            //将信息传回数据库（先删除原有记录，再插入新的记录）
            if (result == DialogResult.OK)
            {
                MySqlConnection conn = new MySqlConnection(mysqlConnectionString);
                try
                {
                    conn.Open();
                    if (this.UserType == 0)
                    {
                        string sql = "select * from student where id = '" + id + "';";//查询该ID对应的学生信息
                        MySqlCommand cmd = new MySqlCommand(sql, conn);
                        MySqlDataReader reader = cmd.ExecuteReader();//此时reader储存了1条语句
                        reader.Read();

                        string ID = reader[0].ToString();
                        string status = reader[4].ToString();
                        string gra_year = reader[5].ToString();//这三个为只读属性

                        if (textBoxName.Text == String.Empty)
                        {
                            MessageBox.Show("请填写您的姓名");
                            textBoxName.Focus();
                            return;
                        }
                        if (textBoxSsn.Text == String.Empty)
                        {
                            MessageBox.Show("请填写您的身份证号");
                            textBoxSsn.Focus();
                            return;
                        }
                        if (textBoxPassword.Text == String.Empty)
                        {
                            MessageBox.Show("请填写您的密码");
                            textBoxPassword.Focus();
                            return;
                        }
                        //检查身份证号是否为9位纯数字
                        if (textBoxSsn.Text.Length != 9)
                        {
                            MessageBox.Show("身份证号应为9位纯数字");
                            textBoxSsn.Focus();
                            return;
                        }
                        else
                        {
                            byte tempbyte;
                            for (int i = 0; i < 9; i++)
                            {
                                tempbyte = Convert.ToByte(textBoxSsn.Text[i]);
                                if (tempbyte < 48 || tempbyte > 57)
                                {
                                    MessageBox.Show("身份证号应为9位纯数字");
                                    textBoxSsn.Focus();
                                    return;
                                }
                            }

                        }
                        reader.Close();

                        sql = String.Format("update student set name = '{0}',date_of_birth='{1}',ssn={2},password='{3}'" + " where id={4}",
                                            this.textBoxName.Text.ToString(), this.dateTimePicker1.Value.ToString("yyyy-MM-dd"),
                                            this.textBoxSsn.Text.ToString(), textBoxPassword.Text.ToString(), id);
                        cmd = new MySqlCommand(sql, conn);
                        cmd.ExecuteNonQuery();

                        MessageBox.Show("信息修改成功！");
                        Show_personal_info();
                    }
                    else
                    {
                        string sql = "select * from professor where id = '" + id + "';";//查询该ID对应的学生信息
                        MySqlCommand cmd = new MySqlCommand(sql, conn);
                        MySqlDataReader reader = cmd.ExecuteReader();//此时reader储存了1条语句
                        reader.Read();

                        string ID = reader[0].ToString();
                        string status = reader[4].ToString();
                        string dept_name = reader[5].ToString();//这三个为只读属性

                        if (textBoxName.Text == String.Empty)
                        {
                            MessageBox.Show("请填写您的姓名");
                            textBoxName.Focus();
                            return;
                        }
                        if (textBoxSsn.Text == String.Empty)
                        {
                            MessageBox.Show("请填写您的身份证号");
                            textBoxSsn.Focus();
                            return;
                        }
                        if (textBoxPassword.Text == String.Empty)
                        {
                            MessageBox.Show("请填写您的密码");
                            textBoxPassword.Focus();
                            return;
                        }
                        //检查身份证号是否为9位纯数字
                        if (textBoxSsn.Text.Length != 9)
                        {
                            MessageBox.Show("身份证号应为9位纯数字");
                            textBoxSsn.Focus();
                            return;
                        }
                        else
                        {
                            byte tempbyte;
                            for (int i = 0; i < 9; i++)
                            {
                                tempbyte = Convert.ToByte(textBoxSsn.Text[i]);
                                if (tempbyte < 48 || tempbyte > 57)
                                {
                                    MessageBox.Show("身份证号应为9位纯数字");
                                    textBoxSsn.Focus();
                                    return;
                                }
                            }

                        }

                        reader.Close();

                        sql = String.Format("update professor set name = '{0}',date_of_birth='{1}',ssn={2},password='{3}'" +" where id={4}", 
                                            this.textBoxName.Text.ToString(), this.dateTimePicker1.Value.ToString("yyyy-MM-dd"),
                                            this.textBoxSsn.Text.ToString(), textBoxPassword.Text.ToString(),id);
                        cmd = new MySqlCommand(sql, conn);
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("信息修改成功！");
                        Show_personal_info();
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                finally
                {
                    conn.Close();//关闭连接
                }
            }
        }
    }
}
