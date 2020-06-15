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
        DataTable dt;
        private void Show_pro_info()//将教授信息显示在DataGridView1上
        {
            using (MySqlConnection conn = new MySqlConnection(mysqlConnectionString))
            {
                dt = new DataTable();
                try
                {
                    conn.Open();//建立连接。考虑到可能出现异常,使用try catch语句
                    string sqlStr = String.Format("select {0} from {1};",
                        "*", "professor");//构造查询语句
                    MySqlDataAdapter sda = new MySqlDataAdapter(sqlStr, conn);
                    sda.Fill(dt);//把读取的MySQL数据库数据填到离线数据库里
                    dataGridView1.DataSource = dt;//把离线数据库的内容直接绑定到GridView里显示
                    for (int i = 0; i < dataGridView1.ColumnCount; i++)
                    {
                        dataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;//禁止每一列的排序功能
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
        private void AddProfessor(object sender, EventArgs e)
        {
            //新增教授子流程,单击教授管理中的添加按钮，弹出添加窗口
            ProInfo tempform = new ProInfo();
            tempform.ShowDialog();
            if (tempform.Text.Equals("no"))
            {
                return;
            }
            string[] pro_info = Regex.Split(tempform.Text, " ", RegexOptions.IgnoreCase); //通过分隔符空格拆开必要信息
            MySqlConnection conn = new MySqlConnection(mysqlConnectionString);
            try
            {
                conn.Open();//连接数据库
                string sql = "INSERT INTO professor(name, date_of_birth, ssn, status, dept_name) VALUES" +
                            "('" + pro_info[0] + "','" + pro_info[1] + "','" + pro_info[2] + 
                            "','" + pro_info[3] + "','" + pro_info[4] + "');";//插入语句，添加新教授
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                int result = cmd.ExecuteNonQuery();
                if (result == 1)
                {
                    sql = "select id from professor where ssn = '"+pro_info[2]+"';";//由于ID是自动生成的，故以身份证号为媒介查询新教授ID
                    cmd = new MySqlCommand(sql, conn);
                    Object newid = cmd.ExecuteScalar();
                    MessageBox.Show("教授信息添加成功！\n该教授ID为：" + newid.ToString());
                    Show_pro_info();
                }
            }
            catch (Exception ex)
            {
            }
            conn.Close();
        }
        private void DeleteProfessor(object sender, EventArgs e)
        {
            //删除教授子流程，弹出选框要求输入教授ID号，输入后查询并回显，询问是否删除
            InputID tempform = new InputID();
            tempform.ShowDialog();
            if (tempform.Text.Equals("no"))
                return;
            MySqlConnection conn = new MySqlConnection(mysqlConnectionString);
            
            try
            {
                conn.Open();//连接数据库
                string sql = "select * from professor where id = '" + tempform.Text + "';";//查询该ID对应的教授信息
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();//reader内存储了0或1条记录
                
                if (reader.HasRows)
                {
                    reader.Read();

                    DialogResult result = MessageBox.Show("姓名：" + reader[1].ToString() + "\n出生日期：" + reader[2].ToString().Substring(0, 9)
                                            + "\n职称：" + reader[4].ToString() + "\n学院：" + reader[5].ToString() + "\n是否确定删除该名教授？"
                                            , "删除确认", MessageBoxButtons.OKCancel) ;
                    
                    reader.Close();
                    if (result == DialogResult.OK)
                    {
                        sql = "delete from professor where id = '" + tempform.Text + "';";//从数据库中删除该教授信息
                        cmd = new MySqlCommand(sql, conn);
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("删除成功！");
                        Show_pro_info();
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("未找到对应教授！");
                    DeleteProfessor(null, null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            conn.Close();
        }
        private void UpdateProfessor(object sender, EventArgs e)
        {
            //更新教授子流程，查询教授ID号，显示教授信息，删除原本数据，插入新数据
            InputID tempform = new InputID();
            tempform.ShowDialog();
            if (tempform.Text.Equals("no"))
                return;
            MySqlConnection conn = new MySqlConnection(mysqlConnectionString);
            try
            {
                conn.Open();//连接数据库
                string sql = "select * from professor where id = '" + tempform.Text + "';";//查询该ID对应的教授信息
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();//此时reader储存了1条语句
                if (reader.HasRows)
                {
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

                    ProInfo proInfo = new ProInfo();
                    proInfo.update(reader[1].ToString(), reader[2].ToString(), reader[3].ToString(), status, dept_name);
                    proInfo.ShowDialog();
                    reader.Close();

                    if (proInfo.Text.Equals("no"))
                        return;

                    string[] pro_info = Regex.Split(proInfo.Text, " ", RegexOptions.IgnoreCase); //通过分隔符空格拆开修改窗口返回的必要信息
                    sql = String.Format("update professor set name='{0}',date_of_birth='{1}',ssn='{2}',status='{3}',dept_name='{4}'" +
                                      " where id={5};", pro_info[0], pro_info[1], pro_info[2], pro_info[3], pro_info[4], tempform.Text);//更新教授信息

                    cmd = new MySqlCommand(sql, conn);
                    int result = cmd.ExecuteNonQuery();
                    if (result == 1)
                    {
                        MessageBox.Show("教授信息修改成功！");
                        Show_pro_info();
                    }
                }
                else
                {
                    MessageBox.Show("未找到对应教授！");
                    DeleteProfessor(null, null);
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
