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
        DataTable dt_stu;
        private void Show_stu_info()//将学生信息显示在DataGridView1上
        {
            using (MySqlConnection conn = new MySqlConnection(mysqlConnectionString))
            {
                dt_stu = new DataTable();
                try
                {
                    conn.Open();//建立连接。考虑到可能出现异常,使用try catch语句
                    string sqlStr = String.Format("select {0} from {1};",
                        "*", "student");//构造查询语句
                    MySqlDataAdapter sda = new MySqlDataAdapter(sqlStr, conn);
                    sda.Fill(dt_stu);//把读取的MySQL数据库数据填到离线数据库里
                    dataGridView学生信息.DataSource = dt_stu;//把离线数据库的内容直接绑定到GridView里显示
                    for (int i = 0; i < dataGridView学生信息.ColumnCount; i++)
                    {
                        dataGridView学生信息.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;//禁止每一列的排序功能
                    }
                    sda.Dispose();
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
        private void AddStudent(object sender, EventArgs e)
        {
            //新增学生子流程,单击学生管理中的添加按钮，弹出添加窗口
            StuInfo tempform = new StuInfo();
            tempform.ShowDialog();
            if (tempform.Text.Equals("no"))
            {
                return;
            }
            string[] stu_info = Regex.Split(tempform.Text, " ", RegexOptions.IgnoreCase); //通过分隔符空格拆开必要信息
            MySqlConnection conn = new MySqlConnection(mysqlConnectionString);
            try
            {
                conn.Open();//连接数据库
                string sql = "INSERT INTO student(name, date_of_birth, ssn, status, graduate_date) VALUES" +
                            "('" + stu_info[0] + "','" + stu_info[1] + "','" + stu_info[2] +
                            "','" + stu_info[3] + "'," + stu_info[4] + ");";//插入语句，添加新学生
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                int result = cmd.ExecuteNonQuery();
                if (result == 1)
                {
                    sql = "select id from student where ssn = '" + stu_info[2] + "';";//由于ID是自动生成的，故以身份证号为媒介查询新学生ID
                    cmd = new MySqlCommand(sql, conn);
                    Object newid = cmd.ExecuteScalar();
                    MessageBox.Show("学生信息添加成功！\n该学生ID为：" + newid.ToString());
                    Show_stu_info();
                }
            }
            catch (Exception ex)
            {
            }
            conn.Close();
        }
        private void DeleteStudent(object sender, EventArgs e)
        {
            //删除学生子流程，弹出选框要求输入学生ID号，输入后查询并回显，询问是否删除
            InputID tempform = new InputID();
            tempform.ShowDialog();
            if (tempform.Text.Equals("no"))
                return;
            MySqlConnection conn = new MySqlConnection(mysqlConnectionString);

            try
            {
                conn.Open();//连接数据库
                string sql = "select * from student where id = '" + tempform.Text + "';";//查询该ID对应的学生信息
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();//reader内存储了0或1条记录

                if (reader.HasRows)
                {
                    reader.Read();

                    DialogResult result = MessageBox.Show("姓名：" + reader[1].ToString() + "\n出生日期：" + reader[2].ToString().Substring(0,9)
                                            + "\n学历：" + reader[4].ToString() + "\n毕业年份：" + reader[5].ToString() + "\n是否确定删除该名学生？"
                                            , "删除确认", MessageBoxButtons.OKCancel);

                    reader.Close();
                    if (result == DialogResult.OK)
                    {
                        sql = "delete from student where id = '" + tempform.Text + "';";//从数据库中删除该学生信息
                        cmd = new MySqlCommand(sql, conn);
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("删除成功！");
                        Show_stu_info();//更新信息，重新显示
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("未找到对应学生！");
                    DeleteStudent(null, null);//重新输入
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            conn.Close();
        }
        private void UpdateStudent(object sender, EventArgs e)
        {
            //更新学生子流程，查询学生ID号，显示学生信息，删除原本数据，插入新数据
            InputID tempform = new InputID();
            tempform.ShowDialog();
            if (tempform.Text.Equals("no"))
                return;
            MySqlConnection conn = new MySqlConnection(mysqlConnectionString);
            try
            {
                conn.Open();//连接数据库
                string sql = "select * from student where id = '" + tempform.Text + "';";//查询该ID对应的学生信息
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();//此时reader储存了1条语句
                if (reader.HasRows)
                {
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

                    StuInfo stuInfo = new StuInfo();
                    stuInfo.update(reader[1].ToString(), reader[2].ToString(), reader[3].ToString(), status, graduate_date);
                    stuInfo.ShowDialog();
                    reader.Close();

                    if (stuInfo.Text.Equals("no"))
                        return;

                    sql = "delete from student where id = '" + ID + "';";//从数据库中删除该学生信息
                    cmd = new MySqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();

                    string[] stu_info = Regex.Split(stuInfo.Text, " ", RegexOptions.IgnoreCase); //通过分隔符空格拆开修改窗口返回的必要信息
                    sql = "INSERT INTO student VALUES(" + ID + ",'" + stu_info[0] + "','" + stu_info[1] + "','" + stu_info[2] +
                                "','" + stu_info[3] + "','" + stu_info[4] + "','" + password + "');";//插入语句，添加新学生
                    cmd = new MySqlCommand(sql, conn);
                    int result = cmd.ExecuteNonQuery();
                    if (result == 1)
                    {
                        MessageBox.Show("学生信息修改成功！");
                        Show_stu_info();
                    }
                }
                else
                {
                    MessageBox.Show("未找到对应学生！");
                    DeleteStudent(null, null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            conn.Close();
        }
    }
}
