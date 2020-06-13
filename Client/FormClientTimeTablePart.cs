using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Cms;
using System;
using System.Collections.Generic;
using System.Data;

namespace Client
{
    public partial class FormClient
    {

        private static int WeekCountOfEachTerm = 26;//每学期26周
        private static int DayCountOfEachWeek = 7;//一周七天，显然
        private static int PeriodCountOfEachDay = 11;//一天11节课

        private static TimeTableCell[,,] TimeTable = new TimeTableCell[WeekCountOfEachTerm, DayCountOfEachWeek, PeriodCountOfEachDay];
        //课程表，26周，7天，每天11节课

        private DataTable dataTableTimeSlot = new DataTable();//本地存储time_slot数据库表，计算冲突

        public struct TimeTableCell//课程表上一个格子
        {
            public bool IsCourseOccupied { get; private set; }//是否有课

            #region Section主码
            public string course_id;

            public string sec_id;

            public string semester;

            public string year;
            #endregion Section主码

            //true代表有课，false代表无课
            //课程信息啥的也往这塞
            public void SetCourseOccupied(TimeTableCell cell)
            {
                this = cell;
                this.IsCourseOccupied = true;
            }
            public void SetCourseOccupied()
            {
                this.IsCourseOccupied = true;
            }
            public void SetCourseUnoccupied()
            {
                this.IsCourseOccupied = false;
            }
        }

        enum 星期枚举
        {
            星期一, 星期二, 星期三, 星期四, 星期五, 星期六, 星期日
        }
        #region 枚举说明
        //如果在数据库中选择出了time_slot，使用以下方法可以将数据库中的文字转化为下标：
        //ItemArray[0]为id列，可用ToString()方法得到值
        //int day = (int)Enum.Parse(typeof(星期枚举), (string)TimeSlotRow.ItemArray[1]);//得到课程表第二维坐标
        //int start_wk = Convert.ToInt32(TimeSlotRow.ItemArray[2]) - 1;//得到课程表第一维循环开始值
        //int end_wk = Convert.ToInt32(TimeSlotRow.ItemArray[3]) - 1;//得到课程表第一维循环结束值
        //int start_tm = Convert.ToInt32(TimeSlotRow.ItemArray[4]) - 1;//得到课程表第三维循环开始值
        //int end_tm = Convert.ToInt32(TimeSlotRow.ItemArray[5]) - 1;//得到课程表第三维循环结束值
        #endregion 枚举说明

        private void SetTimeTableOccupied(String time_slot_id, TimeTableCell result)
            //给定一个time_slot_id，把课程表上所有这个时间段填充上result课程
        {
            DataRow[] rows = dataTableTimeSlot.Select(
                String.Format("time_slot_id = '{0}'", time_slot_id));
            foreach (DataRow TimeSlotRow in rows)
            {
                int day = (int)Enum.Parse(typeof(星期枚举), (string)TimeSlotRow.ItemArray[1]);//得到课程表第二维坐标
                int start_wk = Convert.ToInt32(TimeSlotRow.ItemArray[2]) - 1;//得到课程表第一维循环开始值
                int end_wk = Convert.ToInt32(TimeSlotRow.ItemArray[3]) - 1;//得到课程表第一维循环结束值
                int start_tm = Convert.ToInt32(TimeSlotRow.ItemArray[4]) - 1;//得到课程表第三维循环开始值
                int end_tm = Convert.ToInt32(TimeSlotRow.ItemArray[5]) - 1;//得到课程表第三维循环结束值
                for (int i = start_wk; i<= end_wk; i++)
                {
                    for (int j = start_tm; j<= end_tm; j++)
                    {
                        TimeTable[i, day, j] = result;
                        TimeTable[i, day, j].SetCourseOccupied(result);
                    }
                }
            }
        }
        public void SetTimeTableUnoccupied(String time_slot_id)//给定time_slot_id，把课程表里所有这个位置设为无课
        {
            DataRow[] rows = dataTableTimeSlot.Select(
                String.Format("time_slot_id = '{0}'", time_slot_id));
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
                        TimeTable[i, day, j].SetCourseUnoccupied();
                    }
                }
            }
        }
        
        private HashSet<TimeTableCell> IsConflicted(String time_slot_id)
            //对于一个给定time_slot_id的课程，判断是否与现有课表内课程冲突，有冲突返回所有冲突的格子，否则返回空集
        {
            HashSet<TimeTableCell> result = new HashSet<TimeTableCell>();
            DataRow[] rows = dataTableTimeSlot.Select(
                String.Format("time_slot_id = '{0}'", time_slot_id));
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
                        if (TimeTable[i, day, j].IsCourseOccupied == true)
                            result.Add(TimeTable[i, day, j]);
                    }
                }
            }
            return result;
        }

        private void InitializeDataTable课程表()//把time_slot数据读到客户端里dataTableSchoolTimeTable备用，用来算时间冲突
        {
            using (MySqlConnection conn = new MySqlConnection(mysqlConnectionString))
            {
                conn.Open();
                MySqlDataAdapter sda = new MySqlDataAdapter("select * from time_slot;", conn);
                sda.Fill(dataTableTimeSlot);
                sda.Dispose();
                conn.Close();
            }
        }
        private static void InitializeSchoolTimeTable()
        {
            for (int i = 0; i < WeekCountOfEachTerm; i++)
                for (int j = 0; j < DayCountOfEachWeek; j++)
                    for (int k = 0; k < DayCountOfEachWeek; k++)
                    {
                        TimeTable[i, j, k].SetCourseUnoccupied();
                        TimeTable[i, j, k].course_id = String.Empty;
                        TimeTable[i, j, k].sec_id = String.Empty;
                        TimeTable[i, j, k].semester = String.Empty;
                        TimeTable[i, j, k].year = String.Empty;
                    }
        }
    }
}