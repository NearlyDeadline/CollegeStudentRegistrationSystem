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
                    DataTable SectionWithoutTeacher = new DataTable();
                    DataTable CourseInformation = new DataTable();
                    conn.Open();
                    MySqlDataAdapter sda = new MySqlDataAdapter("select id, course_id, sec_id, semester, year from takes where (course_id, sec_id, semester, year) in (select course_id, sec_id, semester, year from section where isnull(id));"
                        , conn);
                    sda.Fill(SectionWithoutTeacher);
                    foreach (DataRow row in SectionWithoutTeacher.Rows)
                    {
                        sda.SelectCommand.CommandText = String.Format("select title from course where course_id = '{0}';",
                            row["corse_id"].ToString());
                        if (sda.Fill(CourseInformation) != 1)
                        {
                            throw new Exception("数据库无效数据");
                        }

                        using (StreamWriter writer = new StreamWriter(
                            String.Format(WorkDir + "{0}_{1}{2}.txt", row["id"], CurrentYear, CurrentSemester), true))
                        {
                            writer.WriteLine("因没有老师而中止：" + CourseInformation.Rows[0]["title"]);
                        }
                    }
                    sda.Dispose();

                    MySqlCommand cmd = new MySqlCommand("delete from section where isnull(id));");
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();

                    ChooseAlternativeSection(conn);
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
            假设上述集合为studentRows,一行为studentRow
            foreach (DataRow studentRow in studentRows) {
                String.Format("
                select course_id, title, dept_name, fee from course where course_id in (select course_id from takes where id = {0} and semester = '秋季' and year = 2020);",
                studentRow[0].ToString());
                //选择该学生选的所有课程,自动去重
                //生成账单
                Dictionary<Int32, String> Bills = new Dictionary<Int32, String>();
                //字典,key为学号,value为账单,暂时实现为上述表的Json,输入文件中
                总费用计算sum(fee)即可
                
                课表:略
            }

            
            */
        }

        private static void ChooseAlternativeSection(MySqlConnection conn)
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
                    if (Convert.ToInt32(row["count"]) < 4)
                    {
                        for (int i = 1; i < 3; i++)//有两门备选
                        {
                            sda.SelectCommand = new MySqlCommand(String.Format(
                                "select course_id, sec_id, semester, year from takes where id = {0} and status = '备选{1}';",
                                row["id"].ToString(), i));
                            sda.Fill(AlternativeTakes);
                            if (AlternativeTakes.Rows.Count == 1)
                            {
                                sda.SelectCommand = new MySqlCommand(String.Format(
                                    "select count(*) as count from takes where course_id = '{0}' and sec_id = '{1}' and semester = '{2}' and year = {3};",
                                    AlternativeTakes.Rows[0]["course_id"].ToString(),
                                    AlternativeTakes.Rows[0]["course_id"].ToString(),
                                    AlternativeTakes.Rows[0]["course_id"].ToString(),
                                    AlternativeTakes.Rows[0]["course_id"].ToString()));
                                DataTable count = new DataTable();
                                sda.Fill(count);
                                if (Convert.ToInt32(count.Rows[0][0]) < 10)
                                {

                                }
                                else
                                {

                                }
                            }
                            else
                            {
                                throw new Exception("备选课程数量异常");
                            }
                        }
                    }
                }
            }
            catch(Exception e)
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
