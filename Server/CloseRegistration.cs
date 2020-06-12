using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public partial class Server
    {
        private static void CloseRegistration()
        {
            /*
            没有老师教的section:
            delete from section where isnull(id));
            删除它们

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
