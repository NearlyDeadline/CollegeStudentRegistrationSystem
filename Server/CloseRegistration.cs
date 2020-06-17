using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.IO;

namespace Server
{
    public partial class Server
    {
        private static void CloseRegistration()
        {
            #region 文件夹
            string BillsFolderName = Directory.GetCurrentDirectory() + "\\Bills";
            if (!Directory.Exists(BillsFolderName))//检查是否存在Bills根目录
            {
                Directory.CreateDirectory(BillsFolderName);
            }
            try//清空本学期账单目录下所有内容
            {
                Directory.Delete(BillsFolderName + "\\" + CurrentYear + CurrentSemester, true);
            }
            catch (DirectoryNotFoundException e)//如果找不到目录，则创建
            {
                Console.WriteLine("创建本学期账单目录");
            }
            catch (Exception e)
            {
                Console.WriteLine("删除出现其他错误");
            }
            finally
            {
                Directory.CreateDirectory(BillsFolderName + "\\" + CurrentYear + CurrentSemester);
            }
            #endregion 文件夹
            String WorkDir = BillsFolderName + "\\" + CurrentYear + CurrentSemester + "\\";
            #region 数据库
            using (MySqlConnection conn = new MySqlConnection(mysqlConnectionString))
            {
                try
                {
                    conn.Open();
                    #region 删除有老师但是没有学生选的课
                    MySqlCommand cmd = new MySqlCommand(String.Format(
                        "delete from section as S where S.year = {1} and S.semester = '{0}' and " +
                        "1 > (select count(T.ID) from takes as T where S.course_id = T.course_id and S.sec_id = T.sec_id and S.semester = T.semester and S.year = T.year);",
                        CurrentSemester, CurrentYear), conn);
                    cmd.ExecuteNonQuery();
                    #endregion 删除有老师但是没有学生选的课

                    #region 删除没有老师的课
                    DataTable DeleteSections = new DataTable();
                    DataTable CourseInformation = new DataTable();
                    MySqlDataAdapter sda = new MySqlDataAdapter("select id, course_id, sec_id, semester, year from takes where (course_id, sec_id, semester, year) in (select course_id, sec_id, semester, year from section where isnull(id));"
                        , conn);
                    sda.Fill(DeleteSections);
                    foreach (DataRow row in DeleteSections.Rows)
                    {
                        sda.SelectCommand.CommandText = String.Format("select title from course where course_id = '{0}';",
                            row["course_id"].ToString());
                        if (sda.Fill(CourseInformation) != 1)
                        {
                            throw new Exception("数据库无效数据");
                        }

                        using (StreamWriter writer = new StreamWriter(
                            String.Format(WorkDir + "{0}_{1}{2}.txt", row["id"], CurrentYear, CurrentSemester), true))
                        {
                            writer.WriteLine("因没有老师而中止：" + CourseInformation.Rows[0]["title"]);
                        }
                        CourseInformation.Clear();
                    }

                    cmd.CommandText = "delete from section where isnull(id);";
                    cmd.ExecuteNonQuery();
                    #endregion 删除没有老师的课

                    ChooseAlternativeSection(conn, WorkDir);

                    #region 删除人数太少的课
                    DeleteSections.Clear();
                    sda.SelectCommand.CommandText = String.Format(
                        "select course_id, sec_id, semester, year, count(*) as count from takes where semester = '{0}' and year = {1} group by course_id, sec_id, semester, year;",
                        CurrentSemester, CurrentYear);
                    sda.Fill(DeleteSections);
                    foreach (DataRow row in DeleteSections.Rows)
                    {
                        if (Convert.ToInt32(row["count"]) < 3)
                        {
                            sda.SelectCommand.CommandText = String.Format("select title from course where course_id = '{0}';",
                            row["course_id"].ToString());
                            if (sda.Fill(CourseInformation) != 1)
                            {
                                throw new Exception("数据库无效数据");
                            }

                            DataTable StudentId = new DataTable();
                            sda.SelectCommand.CommandText = String.Format("select id from takes where course_id = '{0}' and sec_id = '{1}' and semester = '{2}' and year = {3};",
                                row["course_id"].ToString(),
                                row["sec_id"].ToString(),
                                row["semester"].ToString(),
                                row["year"].ToString());
                            sda.Fill(StudentId);
                            foreach (DataRow studentIdrow in StudentId.Rows)
                            {
                                using (StreamWriter writer = new StreamWriter(
                                String.Format(WorkDir + "{0}_{1}{2}.txt", studentIdrow["id"], CurrentYear, CurrentSemester), true))
                                {
                                    writer.WriteLine("因人数太少而中止：" + CourseInformation.Rows[0]["title"]);
                                }
                            }
                            CourseInformation.Clear();
                            cmd.CommandText = String.Format("delete from section where course_id = '{0}' and sec_id = '{1}' and semester = '{2}' and year = {3};",
                                row["course_id"].ToString(),
                                row["sec_id"].ToString(),
                                row["semester"].ToString(),
                                row["year"].ToString());
                            cmd.ExecuteNonQuery();
                        }
                    }

                    #endregion 删除人数太少的课

                    ChooseAlternativeSection(conn, WorkDir);

                    #region 删除所有选课失败的记录
                    cmd.CommandText = "delete from takes where status <> '已选';";
                    cmd.ExecuteNonQuery();
                    #endregion 删除所有选课失败的记录

                    #region 生成账单
                    DataTable StudentTakes = new DataTable();
                    sda.SelectCommand.CommandText = String.Format(
                        "select distinct id from takes where semester = '{0}' and year = {1};",
                        CurrentSemester, CurrentYear);
                    sda.Fill(StudentTakes);
                    DataTable EachStudentTakes = new DataTable();
                    foreach (DataRow StudentTakesRow in StudentTakes.Rows)
                    {
                        int id = Convert.ToInt32(StudentTakesRow[0].ToString());
                        sda.SelectCommand.CommandText = String.Format(
                        "select course_id, title, dept_name, fee from course where course_id in (select course_id from takes where id = {0} and semester = '{1}' and year = {2});",
                        id, CurrentSemester, CurrentYear);
                        sda.Fill(EachStudentTakes);
                        foreach (DataRow CourseRow in EachStudentTakes.Rows)
                        {
                            using (StreamWriter writer = new StreamWriter(
                                String.Format(WorkDir + "{0}_{1}{2}.txt", id, CurrentYear, CurrentSemester), true))
                            {
                                writer.WriteLine("成功选课：" + CourseRow["course_id"] + CourseRow["title"] + "——" +
                                    CourseRow["dept_name"] + "。该课程的学费为" + CourseRow["fee"] + "元。");
                            }
                        }
                        EachStudentTakes.Clear();
                        sda.SelectCommand.CommandText = String.Format(
                            "select sum(fee) from course where course_id in (select course_id from takes where id = {0} and semester = '{1}' and year = {2});",
                            id, CurrentSemester, CurrentYear);
                        sda.Fill(EachStudentTakes);
                        foreach (DataRow CourseRow in EachStudentTakes.Rows)
                        {
                            using (StreamWriter writer = new StreamWriter(
                                String.Format(WorkDir + "{0}_{1}{2}.txt", id, CurrentYear, CurrentSemester), true))
                            {
                                writer.WriteLine("总学费为：" + CourseRow["sum(fee)"] + "元。");
                            }
                        }
                        EachStudentTakes.Clear();
                    }

                    #endregion 生成账单
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                finally
                {
                    conn.Close();
                }
            }
            #endregion 数据库
            /*
            没有老师教的section:
            select id, course_id, sec_id, semester, year from takes where (course_id, sec_id, semester, year) in (select course_id, sec_id, semester, year from section where isnull(id));
            //选择学生选了没有老师教的课程
            fileout("无教课老师：course_id, sec_id"  ,id-year-semester.txt);
            //向学生账单输出信息
            delete from section where isnull(id));
            //删除它们

            选择候补课程()
            //选择上课
            select course_id, sec_id, semester, year, count(*) as count from takes where semester = '秋季' and year = 2020 group by course_id, sec_id, semester, year;
            if (Convert.ToInt32(rows[i][4] < 3)
               delete from section where course_id = ....
            //直接删除开课，由于级联删除，也会跟着删teaches和takes

            选择候补课程()

            delete from takes where status <> '已选'
            //删除没选上的课

            select distinct id from takes where semester = '秋季' and year = 2020;
            //选择本学期选过课的学生
            假设上述集合为StudentTakes,一行为StudentTakesRow
            foreach (DataRow StudentTakesRow in StudentTakes) {
                String.Format("
                select course_id, title, dept_name, fee from course where course_id in (select course_id from takes where id = {0} and semester = '秋季' and year = 2020);",
                StudentTakesRow[0].ToString());
                //选择该学生选的所有课程,自动去重
                //生成账单
                总费用计算sum(fee)即可
                
                课表:略
            }

            
            */
        }

        private static void ChooseAlternativeSection(MySqlConnection conn, String WorkDir)
        {
            DataTable TakesTable = new DataTable();
            try
            {
                MySqlDataAdapter sda = new MySqlDataAdapter(String.Format(
                    "select id, count(*) as count from takes where status = '已选' and semester = '{0}' and year = {1} group by id;",
                    CurrentSemester, CurrentYear), conn);
                sda.Fill(TakesTable);
                DataTable AlternativeTakes = new DataTable();
                foreach (DataRow row in TakesTable.Rows)
                {
                    int takesCount = Convert.ToInt32(row["count"]);//已选课数量
                    if (takesCount < 4)
                    {
                        for (int i = 1; i < 3 && takesCount < 4; i++)//有两门备选
                        {
                            sda.SelectCommand.CommandText = String.Format(
                                "select course_id, sec_id, semester, year from takes where id = {0} and status = '备选{1}';",
                                row["id"].ToString(), i);
                            sda.Fill(AlternativeTakes);
                            if (AlternativeTakes.Rows.Count == 1)
                            {
                                sda.SelectCommand.CommandText = String.Format(
                                    "select count(*) as count from takes where course_id = '{0}' and sec_id = '{1}' and semester = '{2}' and year = {3};",
                                    AlternativeTakes.Rows[0]["course_id"].ToString(),
                                    AlternativeTakes.Rows[0]["sec_id"].ToString(),
                                    AlternativeTakes.Rows[0]["semester"].ToString(),
                                    AlternativeTakes.Rows[0]["year"].ToString());
                                DataTable count = new DataTable();
                                sda.Fill(count);
                                MySqlCommand cmd = new MySqlCommand();
                                cmd.Connection = conn;
                                if (Convert.ToInt32(count.Rows[0][0]) < 10)//少于10人，可以选课
                                {
                                    cmd.CommandText = String.Format(
                                        "update takes set status = '已选' where id = {0} and course_id = '{1}' and sec_id = '{2}' and semester = '{3}' and year = {4};",
                                        row["id"],
                                        AlternativeTakes.Rows[0]["course_id"].ToString(),
                                        AlternativeTakes.Rows[0]["sec_id"].ToString(),
                                        AlternativeTakes.Rows[0]["semester"].ToString(),
                                        AlternativeTakes.Rows[0]["year"].ToString());
                                    takesCount++;
                                }
                                else//超出10人，不可选课，删除takes记录
                                {
                                    DataTable CourseInformation = new DataTable();
                                    sda.SelectCommand.CommandText = String.Format("select title from course where course_id = '{0}';",
                                    row["course_id"].ToString());
                                    if (sda.Fill(CourseInformation) != 1)
                                    {
                                        throw new Exception("数据库无效数据");
                                    }
                                    using (StreamWriter writer = new StreamWriter(
                                        String.Format(WorkDir + "{0}_{1}{2}.txt", row["id"], CurrentYear, CurrentSemester), true))
                                    {
                                        writer.WriteLine("因选课人数过多而中止：" + CourseInformation.Rows[0]["title"]);
                                    }
                                    cmd.CommandText = String.Format(
                                        "delete from takes where id = {0} and course_id = '{1}' and sec_id = '{2}' and semester = '{3}' and year = {4};",
                                        row["id"],
                                        AlternativeTakes.Rows[0]["course_id"].ToString(),
                                        AlternativeTakes.Rows[0]["sec_id"].ToString(),
                                        AlternativeTakes.Rows[0]["semester"].ToString(),
                                        AlternativeTakes.Rows[0]["year"].ToString());
                                }
                                cmd.ExecuteNonQuery();
                                cmd.Dispose();
                            }
                            else if (AlternativeTakes.Rows.Count > 1)
                            {
                                throw new Exception("备选课程数量异常");
                            }
                            //else : == 0无备选课程，跳过
                            AlternativeTakes.Clear();
                        }
                    }
                }
                sda.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            /*
            选择候补课程的子过程：
            
            按id计算选课数量
            select id, count(*) as count from takes where status = '已选' and semester = '秋季' and year = 2020 group by id;

            if (Convert.ToInt32(rows[i][1]) < 4){
            //必选课程数量小于4，需要考虑备选课
                for (int i = 1; i<3; i++) {//遍历所有备选课
                    String.Format("select course_id,sec_id,semester,year from takes where id = {0} and status = '备选{1}'", rows[i][0], i.ToString());
                    select count(*) as count from takes where course_id = '{0}' ......主码与上面那句完全相同
                    if count < 10 
                        update takes set status = '已选' where id = ... and course_id =...
                        //把如果课程选择人数小于10，把学生登记上
                    else
                        delete from takes where id = ... and course_id = ...
                        //无效，删除选课
            }
            */
        }
    }
}
