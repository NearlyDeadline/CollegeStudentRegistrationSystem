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
    public partial class SubGrades : Form
    { 
        DataTable dt_subgrades;
        string course_id;
        string sec_id;
        string semester;
        string year;
        string mysqlConnectionString;
        public SubGrades(string course_id, string sec_id, string semester, string year, string mysqlConnectionString)
        {
            InitializeComponent();
            this.course_id = course_id;
            this.sec_id = sec_id;
            this.semester = semester;
            this.year = year;
            this.mysqlConnectionString = mysqlConnectionString;
            using (MySqlConnection conn = new MySqlConnection(this.mysqlConnectionString))
            {
                dt_subgrades = new DataTable();
                try
                {
                    conn.Open();//建立连接。考虑到可能出现异常,使用try catch语句
                    
                    string sqlStr = String.Format("select s.id,s.name,t.grade from takes as t,student as s" +
                                    " where t.course_id = '{0}' and t.sec_id = '{1}' and t.semester = '{2}' and t.year = {3}" +
                                    " and t.id = s.id; ", course_id,sec_id, semester, year);//查询相应学生的ID、姓名、成绩

                    MySqlDataAdapter sda = new MySqlDataAdapter(sqlStr, conn);
                    sda.Fill(dt_subgrades);//把读取的MySQL数据库数据填到离线数据库里
                    dataGridView1.DataSource = dt_subgrades;//把离线数据库的内容直接绑定到GridView里显示

                    for (int i = 0; i < dataGridView1.ColumnCount; i++)
                    {
                        dataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;//禁止每一列的排序功能
                    }

                    this.dataGridView1.Columns[0].ReadOnly = true;
                    this.dataGridView1.Columns[1].ReadOnly = true;
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
        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            for (int index = 0; index < dt_subgrades.Rows.Count; index++)
            {
                dt_subgrades.Rows[index].EndEdit();
            }//一定要把这个循环加进CellValueChanged事件，千万别忘了
            using (MySqlConnection conn = new MySqlConnection(mysqlConnectionString))//初始化连接
            {
                try
                {
                    conn.Open();
                    string input = dt_subgrades.Rows[e.RowIndex][e.ColumnIndex].ToString();
                    //检查输入是否为A,B,C,D,F,I
                    if (input.Length!=1 && input!=String.Empty)
                    {
                        MessageBox.Show("请输入一位字母！");
                        dt_subgrades.Rows[e.RowIndex][e.ColumnIndex] = null;
                        return;
                    }
                    byte by = Convert.ToByte(input[0]);
                    if (((by <65)||(by==69)||(by>70))&&(by!=73))
                    {
                        MessageBox.Show("输入必须为A,B,C,D,F,I之一");
                        dt_subgrades.Rows[e.RowIndex][e.ColumnIndex] = null;
                    }
                    MySqlCommand cmd = new MySqlCommand(String.Format("update takes set grade= '{0}' where id = {1}" +
                                    " and course_id = '{2}' and sec_id = '{3}' and semester = '{4}' and year = {5}"
                                    , input, dt_subgrades.Rows[e.RowIndex][0].ToString(), course_id, sec_id, semester, year), conn);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        private void button完成_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
