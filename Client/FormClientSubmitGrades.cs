using MySql.Data.MySqlClient;
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
    public partial class FormClient
    {

        DataTable dt_course;
        DataTable dt_subgrades;
        string semester = "秋季";
        string year = "2020";//可根据当前时间修改，或由管理员定期修改
        private void QueryTeaches(object sender, EventArgs e)
        {
            //单击查询按钮查询上学期教授的课程
            {
                using (MySqlConnection conn = new MySqlConnection(mysqlConnectionString))
                {
                    dt_course = new DataTable();
                    try
                    {
                        conn.Open();//建立连接。考虑到可能出现异常,使用try catch语句
                        string sql = "select course_id,sec_id from teaches where professor_id = '" + id + "'  and semester = '" + semester
                                    + "'  and year = " + year + "; ";//查询教授上学期教授课程
                        MySqlCommand cmd = new MySqlCommand(sql, conn);
                        MySqlDataReader reader = cmd.ExecuteReader();//reader内存储了0或1条记录

                        if (!reader.HasRows)
                        {
                            MessageBox.Show("您上学期未教授任何课程！", "错误提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            reader.Close();
                            return;
                        }
                        reader.Close();
                        string sqlStr = "select course_id,sec_id from teaches where professor_id = '" + id + "' and semester = '" + semester
                                    + "' and year = " + year + "; ";
                        MySqlDataAdapter sda = new MySqlDataAdapter(sqlStr, conn);
                        sda.Fill(dt_course);//把读取的MySQL数据库数据填到离线数据库里
                        dataGridView上学期课程.DataSource = dt_course;//把离线数据库的内容直接绑定到GridView里显示
                        this.label以前教授课程.Visible = false;
                        this.label双击课程提示.Visible = true;
                        this.button查询教课.Visible = false;

                        for (int i = 0; i < dataGridView上学期课程.ColumnCount; i++)
                        {
                            dataGridView上学期课程.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;//禁止每一列的排序功能
                        }
                        sda.Dispose();//至此课程id和课段id被显示在GridView里
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

        private void SubmitGrades(object sender, DataGridViewCellEventArgs e)
        {
            SubGrades sub = new SubGrades(dt_course.Rows[e.RowIndex][0].ToString(),
                                    dt_course.Rows[e.RowIndex][1].ToString(), semester, year, mysqlConnectionString);

            sub.ShowDialog();
            QueryTeaches(null, null);
        }
    }
}
