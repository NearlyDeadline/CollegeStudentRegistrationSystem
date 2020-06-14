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
        DataTable dt_report_card;
        private void ViewReportCard(object sender, EventArgs e)
        {
            {
                using (MySqlConnection conn = new MySqlConnection(mysqlConnectionString))
                {
                    dt_report_card = new DataTable();
                    try
                    {
                        conn.Open();//建立连接。考虑到可能出现异常,使用try catch语句
  
                        string sql = "select course_id,sec_id,grade from takes where id = '" + id + "'  and semester = '" + semester
                                   + "'  and year = " + year + "; ";//查询学生上学期学习课程
                        MySqlCommand cmd = new MySqlCommand(sql, conn);
                        MySqlDataReader reader = cmd.ExecuteReader();

                        if (!reader.HasRows)
                        {
                            MessageBox.Show("您上学期没有成绩信息！", "错误提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            reader.Close();
                            return;
                        }
                        reader.Close();

                        this.dataGridView成绩展示.Visible = true;
                        this.label成绩提示.Visible = true;

                        string sqlStr = "select course_id,sec_id,grade from takes where id = '" + id + "'  and semester = '" + semester
                                   + "'  and year = " + year + "; ";//构造查询语句
                        MySqlDataAdapter sda = new MySqlDataAdapter(sqlStr, conn);
                        sda.Fill(dt_report_card);//把读取的MySQL数据库数据填到离线数据库里
                        dataGridView成绩展示.DataSource = dt_report_card;//把离线数据库的内容直接绑定到GridView里显示
                        for (int i = 0; i < dataGridView成绩展示.ColumnCount; i++)
                        {
                            dataGridView成绩展示.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;//禁止每一列的排序功能
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
        }
    }
}
