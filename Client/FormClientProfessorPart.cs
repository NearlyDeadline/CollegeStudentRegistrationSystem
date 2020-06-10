using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Client
{
    public partial class FormClient
    {
        private bool IsTeachInformationChanged = false;//是否有修改
        private static readonly int CurrentYear = 2020;//当前年份，用于组合sql语句使用
        private static readonly String CurrentSemester = "Fall";//当前学期，用于组合sql语句使用

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
                    sda.Dispose();
                    conn.Close();
                }
                //TODO:填充课程表

                #region 历史残留
                //this.GetServerMessage("teach@choose," + this.textBoxLoginName.Text);//已选课程
                //if (ReceiveMessage.IsJson())
                //{
                //    dataTable本学期已教课程 = ReceiveMessage.ToDataTable();
                //    dataGridView本学期已教课程.DataSource = dataTable本学期已教课程;
                //    dataGridView本学期已教课程.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                //    foreach(DataGridViewColumn col in dataGridView本学期已教课程.Columns)
                //    {
                //        col.ReadOnly = true;
                //        col.SortMode = DataGridViewColumnSortMode.NotSortable;
                //    }
                //}
                //else
                //{
                //    throw new Exception(ReceiveMessage);
                //}

                //this.GetServerMessage("teach@can," + this.textBoxLoginName.Text);//能教的课
                //if (ReceiveMessage.IsJson())
                //{
                //    dataTable本学期可教课程 = ReceiveMessage.ToDataTable();
                //    dataGridView本学期可教课程.DataSource = dataTable本学期可教课程;
                //    dataGridView本学期可教课程.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                //    foreach (DataGridViewColumn col in dataGridView本学期可教课程.Columns)
                //    {
                //        col.ReadOnly = true;
                //        col.SortMode = DataGridViewColumnSortMode.NotSortable;
                //    }
                //}
                #endregion 历史残留
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
                
        }

        private void button选择教授课程_Click(object sender, EventArgs e)
        {
            if (dataTable本学期可教课程.Rows.Count == 0)
            {
                MessageBox.Show("已无更多课程可以教授", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                //TODO:检测时间冲突
                IsTeachInformationChanged = true;
                DataRow newRow =  dataTable本学期已教课程.NewRow();//把可教课程中选中那一行复制到已教课程里
                for (int i = 0; i< dataGridView本学期可教课程.CurrentRow.Cells.Count - 1; i++)//考虑到id是数字，不输入id
                {
                    newRow[i] = dataGridView本学期可教课程.CurrentRow.Cells[i].Value.ToString();
                }
                newRow["id"] = Convert.ToInt32(textBoxLoginName.Text);
                dataTable本学期已教课程.Rows.Add(newRow);

                DataRow[] TargetBackgroundRow = dataTableBackground.Select(//后台表中，找到这一行，把id从null改为登录老师的id
                    String.Format("course_id = '{0}' AND sec_id = '{1}' AND semester = '{2}' AND year = {3}",
                    dataGridView本学期可教课程.CurrentRow.Cells["course_id"].Value.ToString(),
                    dataGridView本学期可教课程.CurrentRow.Cells["sec_id"].Value.ToString(),
                    dataGridView本学期可教课程.CurrentRow.Cells["semester"].Value.ToString(),
                    dataGridView本学期可教课程.CurrentRow.Cells["year"].Value.ToString()));
                if (TargetBackgroundRow.Length == 1 && TargetBackgroundRow[0].IsNull("id"))
                {
                    TargetBackgroundRow[0]["id"] = this.textBoxLoginName.Text;
                }
                dataTable本学期可教课程.Rows[dataGridView本学期可教课程.CurrentRow.Index].Delete();//可教课程选中那一行标为删除
                dataTable本学期可教课程.AcceptChanges();
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
                DataRow newRow = dataTable本学期可教课程.NewRow();
                for (int i = 0; i < dataGridView本学期已教课程.CurrentRow.Cells.Count-1; i++)//考虑到id是数字，不输入id
                {
                    newRow[i] = dataGridView本学期已教课程.CurrentRow.Cells[i].Value.ToString();
                }
                newRow["id"] = System.DBNull.Value;
                dataTable本学期可教课程.Rows.Add(newRow);

                DataRow[] TargetBackgroundRow = dataTableBackground.Select(//后台表中，找到这一行，把id从登录老师的id改为null
                    String.Format("course_id = '{0}' AND sec_id = '{1}' AND semester = '{2}' AND year = {3}",
                    dataGridView本学期已教课程.CurrentRow.Cells["course_id"].Value.ToString(),
                    dataGridView本学期已教课程.CurrentRow.Cells["sec_id"].Value.ToString(),
                    dataGridView本学期已教课程.CurrentRow.Cells["semester"].Value.ToString(),
                    dataGridView本学期已教课程.CurrentRow.Cells["year"].Value.ToString()));
                if (TargetBackgroundRow.Length == 1 && !TargetBackgroundRow[0].IsNull("id"))
                {
                    TargetBackgroundRow[0]["id"] = System.DBNull.Value;
                }
                dataTable本学期已教课程.Rows[dataGridView本学期已教课程.CurrentRow.Index].Delete();
                dataTable本学期已教课程.AcceptChanges();
            }
        }

        private void button提交选择讲授课程_Click(object sender, EventArgs e)
        {
            IsTeachInformationChanged = false;
            提交选择讲授课程();
        }
        private void 提交选择讲授课程()
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
        private void dataGridView本学期已教课程_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            //TODO:双击展示课程详细信息
        }
    }
}