using MySqlX.XDevAPI.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Windows.Forms;

namespace Client
{
    public partial class FormClient
    {
        private bool IsTeachInformationLoaded = false;//已读取本地保存的教课信息
        private bool IsTeachInformationChanged = false;//是否有修改
        private void TeachCourseManageTabPage_Enter(object sender, EventArgs e)
        {
            if (!IsTeachInformationLoaded)
            {
                //TODO: 从本地读取保存的教课信息
                IsTeachInformationLoaded = true;
            }
            //读取本学期已教课程信息
            try
            {
                this.GetServerMessage("teach@choose," + this.textBoxLoginName.Text);//已选课程
                if (ReceiveMessage.IsJson())
                {
                    dataGridView本学期已教课程.DataSource = ReceiveMessage.ToDataTable();
                    dataGridView本学期已教课程.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                    foreach(DataGridViewColumn col in dataGridView本学期已教课程.Columns)
                    {
                        col.ReadOnly = true;
                        col.SortMode = DataGridViewColumnSortMode.NotSortable;
                    }
                }
                else
                {
                    throw new Exception(ReceiveMessage);
                }
                this.GetServerMessage("teach@previous," + this.textBoxLoginName.Text);//之前教的课
                if (ReceiveMessage.IsJson())
                {
                    dataGridView以前教授课程.DataSource = ReceiveMessage.ToDataTable();
                    dataGridView以前教授课程.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                    foreach (DataGridViewColumn col in dataGridView以前教授课程.Columns)
                    {
                        col.ReadOnly = true;
                        col.SortMode = DataGridViewColumnSortMode.NotSortable;
                    }
                }
                this.GetServerMessage("teach@can," + this.textBoxLoginName.Text);//能教的课
                if (ReceiveMessage.IsJson())
                {
                    dataGridView本学期可教课程.DataSource = ReceiveMessage.ToDataTable();
                    dataGridView本学期可教课程.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                    foreach (DataGridViewColumn col in dataGridView本学期可教课程.Columns)
                    {
                        col.ReadOnly = true;
                        col.SortMode = DataGridViewColumnSortMode.NotSortable;
                    }
                }
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
                DialogResult dr = MessageBox.Show("是否将更改提交到服务器？", "选择教授课程", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);
                switch (dr)
                {
                    case DialogResult.Yes:
                        //TODO:执行提交，修改数据库
                        break;
                    case DialogResult.No:
                        break;
                    case DialogResult.Cancel:
                        MustFocusOnTeachCourseManageTabPage = true;
                        break;
                }
            }
                
        }
        private bool MustFocusOnTeachCourseManageTabPage = false;

        private void button选择教授课程_Click(object sender, EventArgs e)
        {
            IsTeachInformationChanged = true;
            //TODO:从可选课程中删除选择的，移到已选
        }

        private void button取消选择教授课程_Click(object sender, EventArgs e)
        {
            IsTeachInformationChanged = true;
            //TODO:从已选课程中删除选择的，移到可选
        }

        private void button提交选择讲授课程_Click(object sender, EventArgs e)
        {
            IsTeachInformationChanged = false;
            //TODO:执行提交，修改数据库
        }
    }
}