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

        private DataTable dataTable本学期已选择课程 = new DataTable();
        private DataTable data本学期已选择课程的详细信息 = new DataTable();
        private DataTable dataTableCoursesInfo = new DataTable();
        private DataTable dataTableSectionInfoExpect = new DataTable();
        //由于奇怪的选中要求 需要这两个表来在完成选课功能
        private DataTable dataTable选中与已选 = new DataTable();
        private DataTable dataTable选中与已选详细信息 = new DataTable();
        private DataTable dataTable备选 = new DataTable();
        private DataTable dataTableScheduleForWeek = new DataTable();   //用于加载显示某周的课程
        private DataTable dataTableBackgroundForStu = new DataTable();  //后台表，存储可教与已教课程的并集，负责偷偷地与数据库联络，更新数据库

        private bool IsThereASchedule = true;

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
                    MySqlDataAdapter sda = new MySqlDataAdapter(String.Format("SELECT * FROM takes where id = {0} and year = {1} and semester = '{2}' ORDER BY `status` ;",
                                textBoxLoginName.Text, CurrentYear, CurrentSemester), conn);
                    sda.Fill(dataTable本学期已选择课程);
                    dataGridViewShow1.DataSource = dataTable本学期已选择课程;
                    dataGridViewShow1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                    foreach (DataGridViewColumn col in dataGridViewShow1.Columns)
                    {
                        col.ReadOnly = true;
                        col.SortMode = DataGridViewColumnSortMode.NotSortable;
                    }
                    #endregion 已选择课程
                    #region 已选课程详细信息
                    sda = new MySqlDataAdapter(String.Format("select * from section where (course_id,sec_id, semester, `year`) in (select course_id,sec_id, semester, `year` from takes where id = {0} and year = {1} and semester = '{2}' );",
                          textBoxLoginName.Text,CurrentYear, CurrentSemester), conn);
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
                    if (!IsCourseRegistrationOpen)
                    {
                        this.buttonStudentCreate.Visible = false;
                        this.buttonStudentUpdate.Visible = false;
                        this.buttonStudentDelete.Visible = false;
                    }
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

                    #region 课程详细信息
                    sda = new MySqlDataAdapter(String.Format("SELECT s.course_id, c.title, pr.prereq_id, s.sec_id as '教学班', s.semester, s.time_slot_id, s.`year`, s.building, s.room_number, p.`name` AS 'professor name', c.dept_name, c.credits, c.fee  " +
                        " FROM section s" +
                        " LEFT JOIN professor p ON s.id = p.id" +
                        " LEFT JOIN course c ON s.course_id = c.course_id" +
                        " LEFT JOIN prereq pr ON s.course_id = pr.course_id" +
                        " WHERE  s.`year` = {0} and s.semester = '{1}';", CurrentYear, CurrentSemester), conn);
                    sda.Fill(dataTableCoursesInfo);
                    dataGridViewShow2.DataSource = dataTableCoursesInfo;
                    dataGridViewShow2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                    foreach (DataGridViewColumn col in dataGridViewShow2.Columns)
                    {
                        col.ReadOnly = true;
                        col.SortMode = DataGridViewColumnSortMode.NotSortable;
                    }
                    #endregion 课程详细信息

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
            LoadRegisterCoursesTabPage();
        }

        private void LoadRegisterCoursesTabPage()
        {
            dataTable选中与已选.Clear();
            dataTable备选.Clear();
            dataTableSectionInfoExpect.Clear();
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
                    sda = new MySqlDataAdapter(String.Format("SELECT * FROM takes where id = {0} and year = {1} and semester = '{2}'and (`status` = '备选1' or `status` = '备选2' ) ORDER BY `status` ;",
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

                    #region 本学期课程目录
                    // 你以为我想写这么长的SQL吗？？？
                    sda = new MySqlDataAdapter(String.Format("SELECT s.course_id,c.title,s.sec_id,s.semester,s.time_slot_id,s.`year`,s.building,s.room_number, p.`name` AS 'professor name' , p.id AS 'professor id'  ,p.dept_name ,c.credits,c.fee" +
                        " FROM (SELECT * FROM section WHERE  year = {1} and semester = '{2}' and (course_id,sec_id, semester, `year`) " +
                        "not in (select course_id,sec_id, semester, `year`  from section where (course_id,sec_id, semester, `year`) in (select course_id,sec_id, semester, `year` from takes where id = {0} and year = {1} and semester = '{2}'  ))) s " +
                        "LEFT JOIN professor p ON s.id = p.id " +
                        "LEFT JOIN course c ON s.course_id = c.course_id; ", textBoxLoginName.Text, CurrentYear, CurrentSemester), conn);
                    sda.Fill(dataTableSectionInfoExpect);
                    dataGridView学生本学期课程目录.DataSource = dataTableSectionInfoExpect;
                    dataGridView学生本学期课程目录.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                    foreach (DataGridViewColumn col in dataGridView学生本学期课程目录.Columns)
                    {
                        col.ReadOnly = true;
                        col.SortMode = DataGridViewColumnSortMode.NotSortable;
                    }
                    #endregion 本学期课程目录

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

        private void button添加学生已选课程_Click(object sender, EventArgs e)
        {
            #region 检测是否达到上限
            if (dataTable选中与已选.Rows.Count >= 4)
            {
                MessageBox.Show("主课程数目已达上限！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            #endregion 检测是否达到上限
            
            #region 检测时间冲突
            string time_slot_id = dataGridView学生本学期课程目录.CurrentRow.Cells[4].Value.ToString();
            //MessageBox.Show(time_slot_id, time_slot_id, MessageBoxButtons.OK, MessageBoxIcon.Information);
            HashSet<TimeTableCell> conflictCells = IsConflicted(time_slot_id);
            if (conflictCells.Count != 0)//有时间冲突
            {
                String ConflictCourseIds = String.Empty;
                foreach (TimeTableCell conflictSection in conflictCells)
                    ConflictCourseIds += conflictSection.course_id + "\n";
                MessageBox.Show(String.Format(
                       "课程时间冲突，冲突课程如下\n{0}", ConflictCourseIds), "课程冲突", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            #endregion 检测时间冲突

            #region 检测先导课程
            try
            {
                using (MySqlConnection conn = new MySqlConnection(mysqlConnectionString))
                {
                    conn.Open();
                    string sql = String.Format("select prereq_id from prereq where course_id = '{0}' and prereq_id not in (select course_id from takes where id = {1} and not isnull(grade) and grade <> 'F' and grade <> 'I');",
                     dataGridView学生本学期课程目录.CurrentRow.Cells[0].Value.ToString(), textBoxLoginName.Text);
                    MySqlDataAdapter sda = new MySqlDataAdapter(sql, conn);
                    DataTable PrereqData = new DataTable();
                    sda.Fill(PrereqData);
                    sda.Dispose();
                    conn.Close();
                    if (PrereqData.Rows.Count != 0)
                    {
                        string emessage = "缺少先导课程:\n";
                        foreach (DataRow row in PrereqData.Rows)
                        {
                            emessage += row[0].ToString()+ "\n" ;
                        }
                        MessageBox.Show(emessage, "缺少先导课程", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("无法验证先导课程", "先导课程检测错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "先导课程检测错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            #endregion 检测先导课程

            AddStudentTakes(time_slot_id,this.dataTable选中与已选);
        }

        private void button删除学生已选课程_Click(object sender, EventArgs e)
        {
            if (dataTable选中与已选.Rows.Count == 0)
            {
                MessageBox.Show("已无更多课程可以删除", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            CancelStudentTakes(this.dataGridView学生已选与选中课程,this.dataTable选中与已选);
        }
        private void button添加学生备选课程_Click(object sender, EventArgs e)
        {
            #region 检测是否达到上限
            if (dataTable备选.Rows.Count >= 2)
            {
                MessageBox.Show("主课程数目已达上限！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            #endregion 检测是否达到上限

            #region 检测时间冲突
            string time_slot_id = dataGridView学生本学期课程目录.CurrentRow.Cells[4].Value.ToString();
            //MessageBox.Show(time_slot_id, time_slot_id, MessageBoxButtons.OK, MessageBoxIcon.Information);
            HashSet<TimeTableCell> conflictCells = IsConflicted(time_slot_id);
            if (conflictCells.Count != 0)//有时间冲突
            {
                String ConflictCourseIds = String.Empty;
                foreach (TimeTableCell conflictSection in conflictCells)
                    ConflictCourseIds += conflictSection.course_id + "\n";
                MessageBox.Show(String.Format(
                       "课程时间冲突，冲突课程如下\n{0}", ConflictCourseIds), "课程冲突", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            #endregion 检测时间冲突

            #region 检测先导课程
            try
            {
                using (MySqlConnection conn = new MySqlConnection(mysqlConnectionString))
                {
                    conn.Open();
                    string sql = String.Format("select prereq_id from prereq where course_id = '{0}' and prereq_id not in (select course_id from takes where id = {1} and not isnull(grade) and grade <> 'F' and grade <> 'I');",
                     dataGridView学生本学期课程目录.CurrentRow.Cells[0].Value.ToString(), textBoxLoginName.Text);
                    MySqlDataAdapter sda = new MySqlDataAdapter(sql, conn);
                    DataTable PrereqData = new DataTable();
                    sda.Fill(PrereqData);
                    sda.Dispose();
                    conn.Close();
                    if (PrereqData.Rows.Count != 0)
                    {
                        string emessage = "缺少先导课程:\n";
                        foreach (DataRow row in PrereqData.Rows)
                        {
                            emessage += row[0].ToString() + "\n";
                        }
                        MessageBox.Show(emessage, "缺少先导课程", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("无法验证先导课程", "先导课程检测错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "先导课程检测错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            #endregion 检测先导课程

            AddStudentTakes(time_slot_id, this.dataTable备选);
        }
        private void button删除学生备选_Click(object sender, EventArgs e)
        {
            if (dataTable备选.Rows.Count == 0)
            {
                MessageBox.Show("已无更多课程可以删除", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
           
            CancelStudentTakes(this.dataGridView学生备选课程, this.dataTable备选);
        }

        private void button学生课表保存_Click(object sender, EventArgs e)
        {
            try
            {
                using(MySqlConnection conn = new MySqlConnection(mysqlConnectionString))
                {
                    conn.Open();
                    string InsertSql;
                    MySqlCommand cmd = new MySqlCommand("", conn);
                    foreach (DataRow row in dataTable选中与已选.Rows)
                    {
                        if (row[6].ToString().Equals("未保存"))
                        {
                            row[6] = "选中";
                            InsertSql = String.Format("INSERT INTO takes(`id`, `course_id`, `sec_id`, `semester`, `year`, `status`) VALUES ({0}, '{1}', '{2}', '{3}', {4}, '选中');",
                            row[0].ToString(),
                            row[1].ToString(),
                            row[2].ToString(),
                            row[3].ToString(),
                            row[4].ToString());
                            cmd.CommandText= InsertSql;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    int Rowcount = 0;
                    foreach (DataRow row in dataTable备选.Rows)
                    {
                        Rowcount++;
                        if (row[6].ToString().Equals("未保存"))
                        {
                            row[6] = "备选" + Rowcount.ToString();
                            InsertSql = String.Format("INSERT INTO takes(`id`, `course_id`, `sec_id`, `semester`, `year`, `status`) VALUES ({0}, '{1}', '{2}', '{3}', {4}, '{5}');",
                            row[0].ToString(),
                            row[1].ToString(),
                            row[2].ToString(),
                            row[3].ToString(),
                            row[4].ToString(),
                            row[6].ToString());
                            cmd.CommandText = InsertSql;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    cmd.Dispose();
                    conn.Close();
                    MessageBox.Show("保存完毕", "保存课表", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadRegisterCoursesTabPage();       //回到流程开始
                    ChangeScheduleWeek(this.comboBoxWeekChange.SelectedIndex);
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("无法更新数据", "保存课表", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "保存课表", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void button学生课表提交_Click(object sender, EventArgs e)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(mysqlConnectionString))
                {
                    conn.Open();
                    string InsertSql;
                    string SelectSql;
                    string UpdateSql;
                    MySqlCommand cmd = new MySqlCommand("", conn);
                    #region 验证人数
                    string numbererrormessage = "";
                    bool numbererror = false;
                    foreach (DataRow row in dataTable选中与已选.Rows)
                    {
                       
                        if (row[6].ToString().Equals("选中")|| row[6].ToString().Equals("未保存"))
                        {
                            SelectSql = String.Format("select count(*) from takes where course_id = '{0}' and sec_id = {1} and semester = '{2}' and year = {3} and status = '已选';",
                            row[1].ToString(),
                            row[2].ToString(),
                            row[3].ToString(),
                            row[4].ToString());
                            cmd.CommandText = SelectSql;
                            object result = cmd.ExecuteScalar();
                            int num = Convert.ToInt32(result);
                            if (num >= 10)
                            {
                                numbererrormessage += String.Format("课程{0}已被选满\n", row[1].ToString());
                                numbererror = true;
                            }
                        }
                    }
                    foreach (DataRow row in dataTable备选.Rows)
                    {
                        if (row[6].ToString().Equals("选中") || row[6].ToString().Equals("未保存"))
                        {
                            SelectSql = String.Format("select count(*) from takes where course_id = '{0}' and sec_id = {1} and semester = '{2}' and year = {3} and status = '已选';",
                            row[1].ToString(),
                            row[2].ToString(),
                            row[3].ToString(),
                            row[4].ToString());
                            cmd.CommandText = SelectSql;
                            object result = cmd.ExecuteScalar();
                            int num = Convert.ToInt32(result);
                            if (num >= 10)
                            {
                                numbererrormessage += String.Format("课程{0}已被选满\n", row[1].ToString());
                                numbererror = true;
                            }
                        }
                    }
                    if(numbererror == true)
                    {
                        MessageBox.Show(numbererrormessage, "人数冲突", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    #endregion 验证人数

                    #region 更新主选
                    foreach (DataRow row in dataTable选中与已选.Rows)
                    {
                        if (row[6].ToString().Equals("未保存"))
                        {
                            row[6] = "已选";
                            InsertSql = String.Format("INSERT INTO takes(`id`, `course_id`, `sec_id`, `semester`, `year`, `status`) VALUES ({0}, '{1}', '{2}', '{3}', {4}, '已选');",
                            row[0].ToString(),
                            row[1].ToString(),
                            row[2].ToString(),
                            row[3].ToString(),
                            row[4].ToString());
                            cmd.CommandText = InsertSql;
                            cmd.ExecuteNonQuery();
                        }else if (row[6].ToString().Equals("选中"))
                        {
                            row[6] = "已选";
                            UpdateSql = String.Format("UPDATE takes SET `status` = '已选' WHERE `id` = {0} AND `course_id` = '{1}' AND `sec_id` = '{2}' AND `semester` = '{3}' AND `year` = {4};",
                            row[0].ToString(),
                            row[1].ToString(),
                            row[2].ToString(),
                            row[3].ToString(),
                            row[4].ToString());
                            cmd.CommandText = UpdateSql;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    #endregion 更新主选

                    #region 更新备选
                    int Rowcount = 0;
                    foreach (DataRow row in dataTable备选.Rows)
                    {
                        Rowcount++;
                        if (row[6].ToString().Equals("未保存"))
                        {
                            row[6] = "备选" +Rowcount.ToString();
                            InsertSql = String.Format("INSERT INTO takes(`id`, `course_id`, `sec_id`, `semester`, `year`, `status`) VALUES ({0}, '{1}', '{2}', '{3}', {4}, '{5}');",
                            row[0].ToString(),
                            row[1].ToString(),
                            row[2].ToString(),
                            row[3].ToString(),
                            row[4].ToString(),
                            row[6].ToString());
                            cmd.CommandText = InsertSql;
                            cmd.ExecuteNonQuery();
                        }
                        else if (row[6].ToString().Equals("选中"))
                        {
                            row[6] = "备选" + Rowcount.ToString();
                            UpdateSql = String.Format("UPDATE takes SET `status` = '{5}' WHERE `id` = {0} AND `course_id` = '{1}' AND `sec_id` = '{2}' AND `semester` = '{3}' AND `year` = {4};",
                            row[0].ToString(),
                            row[1].ToString(),
                            row[2].ToString(),
                            row[3].ToString(),
                            row[4].ToString(),
                            row[6].ToString());
                            cmd.CommandText = UpdateSql;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    #endregion 更新备选
                    cmd.Dispose();
                    conn.Close();
                    MessageBox.Show("提交成功", "提交课表", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadRegisterCoursesTabPage();       //回到流程开始
                    ChangeScheduleWeek(this.comboBoxWeekChange.SelectedIndex);
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("无法更新数据", "提交课表", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "提交课表", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        private void buttonStudentDelete_Click(object sender, EventArgs e)
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

        // 老卡卡西了
        private void AddStudentTakes(String time_slot_id,DataTable dataTable)
        {
            #region 把选中的行复制到已选课程里
            DataRow newRow = dataTable.NewRow();
            newRow[0] = Convert.ToInt32(textBoxLoginName.Text);                              //id
            newRow[1] = dataGridView学生本学期课程目录.CurrentRow.Cells[0].Value.ToString(); //course_id
            newRow[2] = dataGridView学生本学期课程目录.CurrentRow.Cells[2].Value.ToString(); //sec_id
            newRow[3] = dataGridView学生本学期课程目录.CurrentRow.Cells[3].Value.ToString(); //semester
            newRow[4] = dataGridView学生本学期课程目录.CurrentRow.Cells[5].Value.ToString(); //year
            newRow[6] = "未保存";
            dataTable.Rows.Add(newRow);
            #endregion 把选中的行复制到已选课程里

            #region 填充课程表为有课
            TimeTableCell tableCell = new TimeTableCell();
            tableCell.SetCourseOccupied();
            tableCell.course_id = dataGridView学生本学期课程目录.CurrentRow.Cells[0].Value.ToString();
            tableCell.sec_id = dataGridView学生本学期课程目录.CurrentRow.Cells[2].Value.ToString();
            tableCell.semester = dataGridView学生本学期课程目录.CurrentRow.Cells[3].Value.ToString();
            tableCell.year = dataGridView学生本学期课程目录.CurrentRow.Cells[5].Value.ToString();
            tableCell.id = dataGridView学生本学期课程目录.CurrentRow.Cells[8].Value.ToString();   
            SetTimeTableOccupied(time_slot_id, tableCell);
            #endregion 填充课程表为有课

            #region 把可选课程中选中的行删除掉
            dataTableSectionInfoExpect.Rows[dataGridView学生本学期课程目录.CurrentRow.Index].Delete();//可教课程选中那一行标为删除
            dataTableSectionInfoExpect.AcceptChanges();
            #endregion 把可选课程中选中的行删除掉
        }
        private void CancelStudentTakes(DataGridView dataGridView, DataTable dataTable)
        {
            bool sqldatachange = false;
            string WarningMessage = String.Format("是否确认删除掉如下课程\n{0}", dataGridView.CurrentRow.Cells[1].Value.ToString());
            if (dataGridView.CurrentRow.Cells[6].Value.ToString().Equals("未保存"))
            {
                WarningMessage += "\n所选课程为未保存课程仅会在显示界面上删除修改";
                sqldatachange = false;
            }
            else
            {
                WarningMessage += "\n课程已保存或提交，操作将修改选课数据";
                sqldatachange = true;
            }
            DialogResult dialogResult = MessageBox.Show(WarningMessage, "删除课程", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (dialogResult == DialogResult.No)
                return;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(mysqlConnectionString))
                {
                    conn.Open();
                    DataTable tempdatatable = new DataTable();
                    string Sql = String.Format("SELECT s.course_id,c.title,s.sec_id,s.semester,s.time_slot_id,s.`year`,s.building,s.room_number, p.`name` AS 'professor name' , p.id AS 'professor id',p.dept_name ,c.credits,c.fee " +
                        "FROM section s LEFT JOIN professor p ON s.id = p.id LEFT JOIN course c ON s.course_id = c.course_id " +
                        "WHERE s.course_id = '{0}'  and s.sec_id = {1} and s.semester = '{2}' and s.`year` = {3}; ",
                        dataGridView.CurrentRow.Cells[1].Value.ToString(), dataGridView.CurrentRow.Cells[2].Value.ToString(), dataGridView.CurrentRow.Cells[3].Value.ToString(), dataGridView.CurrentRow.Cells[4].Value.ToString());
                    MySqlDataAdapter sda = new MySqlDataAdapter(Sql, conn);
                    sda.Fill(tempdatatable);
                    #region 填充课程表为没课
                    SetTimeTableUnoccupied(tempdatatable.Rows[0][4].ToString());
                    #endregion 填充课程表为没课

                    #region 为dataGridView学生本学期课程目录添加新行
                    DataRow newRow = dataTableSectionInfoExpect.NewRow();
                    newRow.ItemArray = tempdatatable.Rows[0].ItemArray;
                    dataTableSectionInfoExpect.Rows.Add(newRow);
                    dataTableSectionInfoExpect.AcceptChanges();
                    #endregion 为dataGridView学生本学期课程目录添加新行

                    #region 将删除同步到数据库
                    if (sqldatachange == true)
                    {
                        string sql = String.Format(" DELETE FROM takes WHERE `id` = {0} AND `course_id` = '{1}' AND `sec_id` = '{2}' AND `semester` = '{3}' AND `year` = {4};", 
                            textBoxLoginName.Text,
                            dataGridView.CurrentRow.Cells[1].Value.ToString(), 
                            dataGridView.CurrentRow.Cells[2].Value.ToString(), 
                            dataGridView.CurrentRow.Cells[3].Value.ToString(), 
                            dataGridView.CurrentRow.Cells[4].Value.ToString());
                        MySqlCommand cmd = new MySqlCommand(sql, conn);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                    }
                    #endregion 将删除同步到数据库

                    #region 把课程中选中的行删除掉
                    dataTable.Rows[dataGridView.CurrentRow.Index].Delete();
                    dataTable.AcceptChanges();
                    #endregion 把课程中选中的行删除掉
                    sda.Dispose();
                    conn.Close();
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.ToString(), "严重错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
