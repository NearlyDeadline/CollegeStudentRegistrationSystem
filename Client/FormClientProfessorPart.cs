using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Crmf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Windows.Forms;

namespace Client
{
    public partial class FormClient
    {
        private bool IsTeachInformationChanged = false;//是否有修改
        private static readonly int CurrentYear = 2020;//当前年份，用于组合sql语句使用
        private static readonly String CurrentSemester = "秋季";//当前学期，用于组合sql语句使用

        private DataTable dataTable本学期已教课程 = new DataTable();
        private DataTable dataTable本学期可教课程 = new DataTable();
        private DataTable dataTable以前教授课程 = new DataTable();
        private DataTable dataTableBackground = new DataTable();//后台表，存储可教与已教课程的并集，负责偷偷地与数据库联络，更新数据库
        //得亏MySQL支持个union啊，真好

        private void TeachCourseManageTabPage_Enter(object sender, EventArgs e)
        {
            dataTable本学期可教课程.Clear();
            dataTable本学期已教课程.Clear();
            dataTable以前教授课程.Clear();
            dataTableBackground.Clear();
            new Thread(new ThreadStart(InitializeSchoolTimeTable)).Start();
            //读取本学期已教课程信息
            try
            {
                //不好意思，我背叛了无产阶级.jpg，只有这个之前教的课完全不需要主键信息，因此只把这个表通过JSON传播
                //裸传JSON的问题在于当select语句结果为空集时，传回的DataTable没有列头
                //因此只对如下情况选择JSON：
                //(1)一定不会返回空集
                //(2)返回空集时视为报错（比如找不到目标，用例直接终止或重新开始）
                #region 之前教的课
                this.GetServerMessage("teach@previous," + this.textBoxLoginName.Text);//之前教的课
                if (ReceiveMessage.IsJson())
                {
                    dataTable以前教授课程 = ReceiveMessage.ToDataTable();
                    dataGridView以前教授课程.DataSource = dataTable以前教授课程;
                    dataGridView以前教授课程.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                    foreach (DataGridViewColumn col in dataGridView以前教授课程.Columns)
                    {
                        col.ReadOnly = true;
                        col.SortMode = DataGridViewColumnSortMode.NotSortable;
                    }
                }
                else//选课结束
                {
                    throw new Exception("选课系统已关闭");
                }
                #endregion 之前教的课
                using (MySqlConnection conn = new MySqlConnection(mysqlConnectionString))
                {
                    #region 已教课程
                    conn.Open();//已教课程，直接选择section中id符合条件的即可
                    MySqlDataAdapter sda = new MySqlDataAdapter(String.Format("SELECT * FROM section where id = {0} and year = {1} and semester = '{2}';",
                                textBoxLoginName.Text, CurrentYear, CurrentSemester), conn);
                    sda.Fill(dataTable本学期已教课程);
                    dataGridView本学期已教课程.DataSource = dataTable本学期已教课程;
                    dataGridView本学期已教课程.DataSource = dataTable本学期已教课程;
                    dataGridView本学期已教课程.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                    foreach(DataGridViewColumn col in dataGridView本学期已教课程.Columns)
                    {
                        col.ReadOnly = true;
                        col.SortMode = DataGridViewColumnSortMode.NotSortable;
                    }
                    #endregion 已教课程
                    #region 填充TimeTable
                    foreach (DataRow sectionRow in dataTable本学期已教课程.Rows)
                    {
                        DataRow[] rows = dataTableTimeSlot.Select(
                            String.Format("time_slot_id = '{0}'", sectionRow[6]));//取出已教课程的time_slot_id
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
                                }
                            }
                        }
                    }
                    #endregion 填充TimeTable
                    #region 可教课程
                    sda = new MySqlDataAdapter(String.Format("select * from section where (course_id in (select course_id from can_teach where id = {0}) and isnull(id)) and year = {1} and semester = '{2}';",
                        textBoxLoginName.Text,CurrentYear,CurrentSemester), conn);
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
                                textBoxLoginName.Text, CurrentYear,CurrentSemester), conn);
                    sda.Fill(dataTableBackground);
                    dataTableBackground.PrimaryKey = new DataColumn[]
                    {
                        dataTableBackground.Columns["course_id"],
                        dataTableBackground.Columns["sec_id"],
                        dataTableBackground.Columns["semester"],
                        dataTableBackground.Columns["year"]
                    };
                    #endregion 后台课程
                    if (dataTableBackground.Rows.Count == 0)
                        throw new Exception("没有可选课程");
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

        private void TeachCourseManageTabPage_Leave(object sender, EventArgs e)
        {
            if (IsTeachInformationChanged)
            {
                DialogResult dr = MessageBox.Show("是否将更改提交到服务器？", "选择教授课程", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                switch (dr)
                {
                    case DialogResult.Yes:
                        提交选择讲授课程();
                        break;
                    case DialogResult.No:
                        break;
                }
            }
            IsTeachInformationChanged = false;
                
        }

        private void button选择教授课程_Click(object sender, EventArgs e)
        {
            if (dataTable本学期可教课程.Rows.Count == 0)
            {
                MessageBox.Show("已无更多课程可以教授", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                string time_slot_id = dataGridView本学期可教课程.CurrentRow.Cells[6].Value.ToString();
                HashSet<TimeTableCell> conflictCells = IsConflicted(time_slot_id);
                if (conflictCells.Count == 0)//无时间冲突
                {
                    IsTeachInformationChanged = true;
                    AddTeachSection(time_slot_id);
                }
                else//有时间冲突
                {
                    String ConflictCourseIds = String.Empty;
                    foreach (TimeTableCell conflictSection in conflictCells)
                        ConflictCourseIds += conflictSection.course_id + "\n";
                    DialogResult dialogResult = MessageBox.Show(String.Format(
                        "很抱歉，发生时间冲突，是否取消掉如下冲突课程\n{0}用选择的课程替换？",ConflictCourseIds), "解决时间冲突", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                    switch (dialogResult)
                    {
                        case DialogResult.Yes:
                            //TODO:删除之前的课，需要知道Cell内的主码
                            #region 把已教课程中冲突的课清除掉
                            foreach (TimeTableCell conflictCell in conflictCells)
                                //遍历每个冲突的格子，把选的课删掉，类似于点击“移除”按钮
                            {
                                if (dataGridView本学期已教课程.Rows.Count > 0)
                                {
                                    dataGridView本学期已教课程.CurrentRow.Selected = false;
                                    int index = 0;
                                    for (int i = 0; i < dataTable本学期已教课程.Rows.Count; i++)//搜索下标
                                    {
                                        if (dataTable本学期已教课程.Rows[i][0].ToString().Equals(conflictCell.course_id) &&
                                                dataTable本学期已教课程.Rows[i][1].ToString().Equals(conflictCell.sec_id) &&
                                            dataTable本学期已教课程.Rows[i][2].ToString().Equals(conflictCell.semester) &&
                                            dataTable本学期已教课程.Rows[i][3].ToString().Equals(conflictCell.year))
                                        {
                                            index = i;
                                            break;
                                        }
                                    }
                                    DataRow[] conflictRows = dataTable本学期已教课程.Select(
                                        String.Format("course_id = '{0}' and sec_id = '{1}' " +
                                        "and semester = '{2}' and year = '{3}'",
                                        conflictCell.course_id, conflictCell.sec_id, conflictCell.semester, conflictCell.year));
                                    //在已教课程中选出有冲突的课程，由于主码原因应该只有1个
                                    //把这个课程选中，调用删除函数删掉
                                    if (conflictRows.Length == 1)
                                    {
                                        dataGridView本学期已教课程.Rows[index].Selected = true;
                                        CancelTeachSection();
                                    }
                                }
                            }
                            #endregion 把已教课程中冲突的课清除掉
                            #region 加入选的课
                            AddTeachSection(time_slot_id);
                            #endregion 加入选的课
                            break;

                        case DialogResult.No:

                            break;
                    }
                }
            }
        }

        private void button取消选择教授课程_Click(object sender, EventArgs e)
        {
            if (dataTable本学期已教课程.Rows.Count == 0)
            {
                MessageBox.Show("已无更多课程可以取消", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                IsTeachInformationChanged = true;
                CancelTeachSection();
            }
        }
        private void AddTeachSection(String time_slot_id)//把dataGridView本学期可教课程中选中行的课加入已教课程
             //为什么要把这个过程移出来呢？因为解决时间冲突要用到
              //这个过程很难复用，学生选课的话请自行处理DataGridView与DataTable关系，注意表第几列对应section主码
        {
            #region 把可教课程中选中的行复制到已教课程里
            DataRow newRow = dataTable本学期已教课程.NewRow();
            for (int i = 0; i < dataGridView本学期可教课程.CurrentRow.Cells.Count - 1; i++)//考虑到id是数字，不输入id
            {
                newRow[i] = dataGridView本学期可教课程.CurrentRow.Cells[i].Value.ToString();
            }
            newRow[7] = Convert.ToInt32(textBoxLoginName.Text);
            dataTable本学期已教课程.Rows.Add(newRow);
            #endregion 把可教课程中选中的行复制到已教课程里
            #region 把后台表中刚才那行的id改为textBoxLoginName.Text
            DataRow[] TargetBackgroundRow = dataTableBackground.Select(
                String.Format("course_id = '{0}' AND sec_id = '{1}' AND semester = '{2}' AND year = {3}",
                dataGridView本学期可教课程.CurrentRow.Cells[0].Value.ToString(),
                dataGridView本学期可教课程.CurrentRow.Cells[1].Value.ToString(),
                dataGridView本学期可教课程.CurrentRow.Cells[2].Value.ToString(),
                dataGridView本学期可教课程.CurrentRow.Cells[3].Value.ToString()));
            if (TargetBackgroundRow.Length == 1 && TargetBackgroundRow[0].IsNull(7))
            {
                TargetBackgroundRow[0][7] = this.textBoxLoginName.Text;
            }
            #endregion 把后台表中刚才那行的id改为textBoxLoginName.Text
            #region 填充课程表为有课
            TimeTableCell tableCell = new TimeTableCell();
            tableCell.SetCourseOccupied();
            tableCell.course_id = dataGridView本学期可教课程.CurrentRow.Cells[0].Value.ToString();
            tableCell.sec_id = dataGridView本学期可教课程.CurrentRow.Cells[1].Value.ToString();
            tableCell.semester = dataGridView本学期可教课程.CurrentRow.Cells[2].Value.ToString();
            tableCell.year = dataGridView本学期可教课程.CurrentRow.Cells[3].Value.ToString();
            SetTimeTableOccupied(time_slot_id, tableCell);
            #endregion 填充课程表为有课
            #region 把可教课程中选中的行删除掉
            dataTable本学期可教课程.Rows[dataGridView本学期可教课程.CurrentRow.Index].Delete();//可教课程选中那一行标为删除
            dataTable本学期可教课程.AcceptChanges();
            #endregion 把可教课程中选中的行删除掉
        }
        private void CancelTeachSection()//把dataGridView本学期已教课程选中行移除
            //为什么要把这个过程移出来呢？因为解决时间冲突要用到
            //这个过程很难复用，学生选课的话请自行处理DataGridView与DataTable关系，注意表第几列对应section主码
        {
            #region 把已教课程中选中的行复制到可教课程里
            DataRow newRow = dataTable本学期可教课程.NewRow();
            for (int i = 0; i < dataGridView本学期已教课程.CurrentRow.Cells.Count - 1; i++)//考虑到id是数字，不输入id
            {
                newRow[i] = dataGridView本学期已教课程.CurrentRow.Cells[i].Value.ToString();
            }
            newRow[7] = System.DBNull.Value;
            dataTable本学期可教课程.Rows.Add(newRow);
            #endregion 把已教课程中选中的行复制到可教课程里
            #region 把后台表中刚才那行的id改为null
            DataRow[] TargetBackgroundRow = dataTableBackground.Select(//后台表中，找到这一行，把id从登录老师的id改为null
                String.Format("course_id = '{0}' AND sec_id = '{1}' AND semester = '{2}' AND year = {3}",
                dataGridView本学期已教课程.CurrentRow.Cells[0].Value.ToString(),
                dataGridView本学期已教课程.CurrentRow.Cells[1].Value.ToString(),
                dataGridView本学期已教课程.CurrentRow.Cells[2].Value.ToString(),
                dataGridView本学期已教课程.CurrentRow.Cells[3].Value.ToString()));
            if (TargetBackgroundRow.Length == 1 && !TargetBackgroundRow[0].IsNull(7))
            {
                TargetBackgroundRow[0][7] = System.DBNull.Value;
            }
            #endregion 把后台表中刚才那行的id改为null
            #region 把已教课程中选中的行删除掉
            dataTable本学期已教课程.Rows[dataGridView本学期已教课程.CurrentRow.Index].Delete();
            dataTable本学期已教课程.AcceptChanges();
            #endregion 把已教课程中选中的行删除掉
            #region 填充课程表为没课
            SetTimeTableUnoccupied((string)TargetBackgroundRow[0].ItemArray[6]);
            #endregion 填充课程表为没课
        }
        private void button提交选择讲授课程_Click(object sender, EventArgs e)
        {
            IsTeachInformationChanged = false;
            提交选择讲授课程();
        }
        private void 提交选择讲授课程()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(mysqlConnectionString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("select * from section;", conn);
                    MySqlDataAdapter sda = new MySqlDataAdapter(cmd);
                    MySqlCommandBuilder myCommandBuilder = new MySqlCommandBuilder(sda);
                    sda.InsertCommand = myCommandBuilder.GetInsertCommand();
                    sda.UpdateCommand = myCommandBuilder.GetUpdateCommand();
                    sda.DeleteCommand = myCommandBuilder.GetDeleteCommand();
                    sda.Update(dataTableBackground); //更新
                    conn.Close();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("无法进入课程目录系统", "严重错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGridView本学期已教课程_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            ShowSectionInfomation(dataTable本学期已教课程, e);
        }

        private void dataGridView本学期可教课程_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            ShowSectionInfomation(dataTable本学期可教课程, e);
        }
        private void ShowSectionInfomation(DataTable dt, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                try
                {
                    string course_id = (string)dt.Rows[e.RowIndex][0];
                    string time_slot_id = (string)dt.Rows[e.RowIndex][6];
                    DataTable course_info = new DataTable();//详细介绍课程
                    DataTable timeslot_info = new DataTable();//详细介绍时间
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
                        course_info.Columns[4].ColumnName = "费用";
                        #endregion 处理course_info列名
                        sda = new MySqlDataAdapter(String.Format("select day, start_wk, end_wk, start_tm, end_tm from time_slot " +
                            "where time_slot_id ='{0}';", time_slot_id)
                            , conn);
                        sda.Fill(timeslot_info);
                        #region 处理timeslot_info列名
                        timeslot_info.Columns[0].ColumnName = "星期";
                        timeslot_info.Columns[1].ColumnName = "开始上课星期";
                        timeslot_info.Columns[2].ColumnName = "结束上课星期";
                        timeslot_info.Columns[3].ColumnName = "上课时间";
                        timeslot_info.Columns[4].ColumnName = "下课时间";
                        #endregion 处理timeslot_info列名
                        sda.Dispose();
                        conn.Close();
                    }
                    String result = String.Empty;
                    foreach (DataRow row in course_info.Rows)
                    {
                        for (int i = 0; i < course_info.Columns.Count; i++)
                            result += course_info.Columns[i].ColumnName + "：" + row.ItemArray[i].ToString() + "\n";
                    }
                    foreach (DataRow row in timeslot_info.Rows)
                    {
                        for (int i = 0; i < timeslot_info.Columns.Count; i++)
                            result += timeslot_info.Columns[i].ColumnName + "：" + row.ItemArray[i].ToString() + "\n";
                    }
                    MessageBox.Show(result, "课程详细信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("无法进入课程目录系统", "严重错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}