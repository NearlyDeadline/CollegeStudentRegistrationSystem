using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Crmf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

namespace Client
{
    public partial class FormClient
    {
       //private bool IsTeachInformationChanged = false;//是否有修改
       //private static readonly int CurrentYear = 2020;//当前年份，用于组合sql语句使用
       //private static readonly String CurrentSemester = "秋季";//当前学期，用于组合sql语句使用

        private DataTable dataTable本学期已选择课程 = new DataTable();
        private DataTable data本学期已选择课程的详细信息 = new DataTable();

        //由于奇怪的选中要求 需要这两个表来在完成选课功能
        private DataTable dataTable选中与已选 = new DataTable();
        private DataTable dataTable选中与已选详细信息 = new DataTable();
        private DataTable dataTable备选 = new DataTable();
        private DataTable dataTableScheduleForWeek = new DataTable();   //用于加载显示某周的课程
        private DataTable dataTableBackgroundForStu = new DataTable();//后台表，存储可教与已教课程的并集，负责偷偷地与数据库联络，更新数据库

        private bool IsThereASchedule = true;
        //private DataTable dataTable本学期可教课程 = new DataTable();
        //private DataTable dataTable以前教授课程 = new DataTable();
        //得亏MySQL支持个union啊，真好


        private void InitializeStudentPart()
        {
            data本学期已选择课程的详细信息.Clear();
            dataTable本学期已选择课程.Clear();
            //dataTable本学期已教课程.Clear();
            //dataTable以前教授课程.Clear();
            dataTableBackgroundForStu.Clear();
            new Thread(new ThreadStart(InitializeSchoolTimeTable)).Start();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(mysqlConnectionString))
                {
                    #region 已选择课程
                    conn.Open();//已选择课程，直接选择section中id符合条件的即可
                    MySqlDataAdapter sda = new MySqlDataAdapter(String.Format("SELECT * FROM takes where id = {0} and year = {1} and semester = '{2}';",
                                textBoxLoginName.Text, CurrentYear, CurrentSemester), conn);
                    sda.Fill(dataTable本学期已选择课程);
                    //dataGridView学生已选与选中课程.DataSource = dataTable本学期已选择课程;
                    //dataGridView学生已选与选中课程.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                    //foreach (DataGridViewColumn col in dataGridView学生已选与选中课程.Columns)
                    //{
                    //    col.ReadOnly = true;
                    //    col.SortMode = DataGridViewColumnSortMode.NotSortable;
                    //}
                    #endregion 已选择课程
                    #region 已选课程详细信息
                    sda = new MySqlDataAdapter(String.Format("select * from section where (course_id,sec_id, semester, `year`) in (select course_id,sec_id, semester, `year` from takes where id = {0} );",
                          textBoxLoginName.Text), conn);
                    sda.Fill(data本学期已选择课程的详细信息);
                    # endregion 已选课程详细信息
                    #region 填充TimeTable
                    foreach (DataRow sectionRow in data本学期已选择课程的详细信息.Rows)
                    {
                        DataRow[] rows = dataTableTimeSlot.Select(
                            String.Format("time_slot_id = '{0}'", sectionRow[6]));//取出已选课程的time_slot_id
                        foreach (DataRow TimeSlotRow in rows)
                        {
                            int day = (int)Enum.Parse(typeof(星期枚举), (string)TimeSlotRow.ItemArray[1]);//得到课程表第二维坐标
                            int start_wk = Convert.ToInt32(TimeSlotRow.ItemArray[2]) - 1;//得到课程表第一维循环开始值
                            int end_wk = Convert.ToInt32(TimeSlotRow.ItemArray[3]) - 1;//得到课程表第一维循环结束值
                            int start_tm = Convert.ToInt32(TimeSlotRow.ItemArray[4]) - 1;//得到课程表第三维循环开始值
                            int end_tm = Convert.ToInt32(TimeSlotRow.ItemArray[5]) - 1;//得到课程表第三维循环结束值
                            for (int i = start_wk; i <= end_wk; i++)
                            {
                                for (int j = start_tm; j <= end_tm; j++)
                                {
                                    TimeTable[i, day, j].SetCourseOccupied();
                                    TimeTable[i, day, j].course_id = sectionRow[0].ToString();
                                    TimeTable[i, day, j].sec_id = sectionRow[1].ToString();
                                    TimeTable[i, day, j].semester = sectionRow[2].ToString();
                                    TimeTable[i, day, j].year = sectionRow[3].ToString();
                                    TimeTable[i, day, j].id = sectionRow[7].ToString();
                                }
                            }
                        }
                    }
                    #endregion 填充TimeTable
                    #region 显示课程表
                    dataTableScheduleForWeek.Columns.Add("节/周", typeof(string));   //添加列集，下面都是
                    dataTableScheduleForWeek.Columns.Add("周一", typeof(string));
                    dataTableScheduleForWeek.Columns.Add("周二", typeof(string));
                    dataTableScheduleForWeek.Columns.Add("周三", typeof(string));
                    dataTableScheduleForWeek.Columns.Add("周四", typeof(string));
                    dataTableScheduleForWeek.Columns.Add("周五", typeof(string));
                    dataTableScheduleForWeek.Columns.Add("周六", typeof(string));
                    dataTableScheduleForWeek.Columns.Add("周日", typeof(string));
                    if (dataTable本学期已选择课程.Rows.Count == 0)
                    {
                        IsThereASchedule = false;
                        this.label学生课程表周数.Visible = false;
                        this.comboBoxWeekChange.Visible = false;
                        this.buttonStudentUpdate.Visible = false;
                        this.buttonStudentDelete.Visible = false;
                    }
                    else
                    {
                        this.comboBoxWeekChange.SelectedIndex = 0;
                        dataGridViewStudentSchedule.DataSource = dataTableScheduleForWeek;
                        foreach (DataGridViewColumn col in dataGridViewStudentSchedule.Columns)
                        {
                            col.ReadOnly = true;
                            col.SortMode = DataGridViewColumnSortMode.NotSortable;
                        }
                    }
                    #endregion 显示课程表

                    #region 后台课程
                        //为已教和可教课程的并集，用于提交数据库时使用
                        sda = new MySqlDataAdapter(String.Format("select * from section where (course_id in " +
                        "(select course_id from can_teach where id = {0}) " +
                        "and isnull(id)) and year = {1} and semester = '{2}' " +
                        "union SELECT * FROM section where id = {0}" +
                        " and year = {1} and semester = '{2}';",
                                textBoxLoginName.Text, CurrentYear, CurrentSemester), conn);
                    sda.Fill(dataTableBackground);
                    dataTableBackground.PrimaryKey = new DataColumn[]
                    {
                        dataTableBackground.Columns["course_id"],
                        dataTableBackground.Columns["sec_id"],
                        dataTableBackground.Columns["semester"],
                        dataTableBackground.Columns["year"]
                    };
                    #endregion 后台课程
                    //if (dataTableBackground.Rows.Count == 0)
                    //    throw new Exception("没有可选课程");
                    sda.Dispose();
                    conn.Close();
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("无法进入课程目录系统", "严重错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "严重错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        //进入Register需要加载一些奇怪的表目
        private void RegisterCoursesTabPage_Enter(object sender, EventArgs e)
        {
            dataTable选中与已选.Clear();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(mysqlConnectionString))
                {
                    #region 选择与选中课程
                    conn.Open();//已选择课程，直接选择section中id符合条件的即可
                    MySqlDataAdapter sda = new MySqlDataAdapter(String.Format("SELECT * FROM takes where id = {0} and year = {1} and semester = '{2}'and (`status` = '已选' OR `status` = '选中');",
                                textBoxLoginName.Text, CurrentYear, CurrentSemester), conn);
                    sda.Fill(dataTable选中与已选);
                    dataGridView学生已选与选中课程.DataSource = dataTable选中与已选;
                    dataGridView学生已选与选中课程.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                    foreach (DataGridViewColumn col in dataGridView学生已选与选中课程.Columns)
                    {
                        col.ReadOnly = true;
                        col.SortMode = DataGridViewColumnSortMode.NotSortable;
                    }
                    #endregion 选择与选中课程

                    #region 备选课程
                    sda = new MySqlDataAdapter(String.Format("SELECT * FROM takes where id = {0} and year = {1} and semester = '{2}'and `status` = '备选';",
                                textBoxLoginName.Text, CurrentYear, CurrentSemester), conn);
                    sda.Fill(dataTable备选);
                    dataGridView学生备选课程.DataSource = dataTable备选;
                    dataGridView学生备选课程.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                    foreach (DataGridViewColumn col in dataGridView学生备选课程.Columns)
                    {
                        col.ReadOnly = true;
                        col.SortMode = DataGridViewColumnSortMode.NotSortable;
                    }
                    #endregion 备选课程

                    #region 可选课程
                    sda = new MySqlDataAdapter(String.Format("select * from section where (course_id in (select course_id from can_teach where id = {0}) and isnull(id)) and year = {1} and semester = '{2}';",
                        textBoxLoginName.Text, CurrentYear, CurrentSemester), conn);
                    sda.Fill(dataTable本学期可教课程);
                    dataGridView本学期可教课程.DataSource = dataTable本学期可教课程;
                    dataGridView本学期可教课程.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                    foreach (DataGridViewColumn col in dataGridView本学期可教课程.Columns)
                    {
                        col.ReadOnly = true;
                        col.SortMode = DataGridViewColumnSortMode.NotSortable;
                    }
                    #endregion 可教课程
                    #region 后台课程
                    //为已教和可教课程的并集，用于提交数据库时使用
                    sda = new MySqlDataAdapter(String.Format("select * from section where (course_id in " +
                    "(select course_id from can_teach where id = {0}) " +
                    "and isnull(id)) and year = {1} and semester = '{2}' " +
                    "union SELECT * FROM section where id = {0}" +
                    " and year = {1} and semester = '{2}';",
                            textBoxLoginName.Text, CurrentYear, CurrentSemester), conn);
                    sda.Fill(dataTableBackground);
                    dataTableBackground.PrimaryKey = new DataColumn[]
                    {
                        dataTableBackground.Columns["course_id"],
                        dataTableBackground.Columns["sec_id"],
                        dataTableBackground.Columns["semester"],
                        dataTableBackground.Columns["year"]
                    };
                    #endregion 后台课程
                    //if (dataTableBackground.Rows.Count == 0)
                    //    throw new Exception("没有可选课程");
                    sda.Dispose();
                    conn.Close();
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("无法进入课程目录系统", "严重错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "严重错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //主要用于加载课程表
        private void ScheduleTabPage_Enter(object sender, EventArgs e)
        {
            //dataTable本学期已选择课程.Clear();
            //dataTable本学期已教课程.Clear();
            //dataTable以前教授课程.Clear();
            //dataTableBackgroundForStu.Clear();
            //new Thread(new ThreadStart(InitializeSchoolTimeTable)).Start();
        }
        private void comboBoxWeekChange_SelectedIndexChanged(object sender, EventArgs e)
        {
            ChangeScheduleWeek(this.comboBoxWeekChange.SelectedIndex);
        }

        private void buttonStudentCreate_Click(object sender, EventArgs e)
        {
            if(IsThereASchedule == false)
            {
                IsThereASchedule = true;
                this.label学生课程表周数.Visible = true;
                this.comboBoxWeekChange.Visible = true;
                this.comboBoxWeekChange.SelectedIndex = 0;
                dataGridViewStudentSchedule.DataSource = dataTableScheduleForWeek;
                foreach (DataGridViewColumn col in dataGridViewStudentSchedule.Columns)
                {
                    col.ReadOnly = true;
                    col.SortMode = DataGridViewColumnSortMode.NotSortable;
                }
                this.buttonStudentUpdate.Visible = true;
                this.buttonStudentDelete.Visible = true;
                MessageBox.Show("创建成功！", "创建课表", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show( "课表已创建。","创建课表", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void buttonStudentUpdate_Click(object sender, EventArgs e)
        {
            if (!this.tabControl1.TabPages.Contains(RegisterCoursesTabPage))
            {
                this.tabControl1.TabPages.Add(this.RegisterCoursesTabPage);
            }
            this.tabControl1.SelectTab(this.RegisterCoursesTabPage);
        }

        private void dataGridViewStudentSchedule_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            TimeTableCell cell = TimeTable[this.comboBoxWeekChange.SelectedIndex, e.ColumnIndex - 1, e.RowIndex];
            if (cell.IsCourseOccupied)
            {
                try
                {
                    string course_id = cell.course_id;
                    string professor_id = cell.id;
                    DataTable course_info = new DataTable();//详细介绍课程
                    DataTable professor_info = new DataTable();//详细介绍时间
                    using (MySqlConnection conn = new MySqlConnection(mysqlConnectionString))
                    {
                        conn.Open();
                        MySqlDataAdapter sda = new MySqlDataAdapter(String.Format("select * from course where course_id = '{0}';",
                            course_id)
                            , conn);
                        sda.Fill(course_info);
                        #region 处理course_info列名
                        course_info.Columns[0].ColumnName = "课程序号";
                        course_info.Columns[1].ColumnName = "课程名称";
                        course_info.Columns[2].ColumnName = "所属学院";
                        course_info.Columns[3].ColumnName = "学分";
                        //course_info.Columns[4].ColumnName = "费用";
                        #endregion 处理course_info列名
                        sda = new MySqlDataAdapter(String.Format("select id,name,ssn,`status`,dept_name from professor where id = '{0}';", 
                            professor_id)
                            , conn);
                        sda.Fill(professor_info);
                        #region 处理professor_info列名
                        professor_info.Columns[0].ColumnName = "教授id";
                        professor_info.Columns[1].ColumnName = "教授名称";
                        professor_info.Columns[2].ColumnName = "ssn";
                        professor_info.Columns[3].ColumnName = "职位";
                        professor_info.Columns[4].ColumnName = "部门";
                        #endregion 处理professor_info列名
                        sda.Dispose();
                        conn.Close();
                    }
                    String result = String.Empty;
                    foreach (DataRow row in course_info.Rows)
                    {
                        for (int i = 0; i < course_info.Columns.Count; i++)
                            result += course_info.Columns[i].ColumnName + "：" + row.ItemArray[i].ToString() + "\n";
                    }
                    foreach (DataRow row in professor_info.Rows)
                    {
                        for (int i = 0; i < professor_info.Columns.Count; i++)
                            result += professor_info.Columns[i].ColumnName + "：" + row.ItemArray[i].ToString() + "\n";
                    }
                    MessageBox.Show(result, "课程详细信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("无法进入课程目录系统", "严重错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ChangeScheduleWeek(int Week)
        {
            dataTableScheduleForWeek.Clear();
            for (int i = 0; i < 11; i++)                                        //用循环添加13个行集
            {
                DataRow dr = dataTableScheduleForWeek.NewRow();
                dataTableScheduleForWeek.Rows.Add(dr);
            }
            #region 天数节数
            dataTableScheduleForWeek.Rows[0][0] = "第1节";
            dataTableScheduleForWeek.Rows[1][0] = "第2节";
            dataTableScheduleForWeek.Rows[2][0] = "第3节";
            dataTableScheduleForWeek.Rows[3][0] = "第4节";
            dataTableScheduleForWeek.Rows[4][0] = "第5节";
            dataTableScheduleForWeek.Rows[5][0] = "第6节";
            dataTableScheduleForWeek.Rows[6][0] = "第7节";
            dataTableScheduleForWeek.Rows[7][0] = "第8节";
            dataTableScheduleForWeek.Rows[8][0] = "第9节";
            dataTableScheduleForWeek.Rows[9][0] = "第10节";
            dataTableScheduleForWeek.Rows[10][0] = "第11节";
            # endregion 天数节数
            for (int i = 0; i < 11; i++)
            {
                for (int j = 1; j < 8; j++)
                {
                    if(TimeTable[Week, j - 1, i].IsCourseOccupied)
                    {
                        String mess = TimeTable[Week, j - 1, i].course_id;
                        dataTableScheduleForWeek.Rows[i][j] = mess;
                    }
                }
            }
        }
    }
}
