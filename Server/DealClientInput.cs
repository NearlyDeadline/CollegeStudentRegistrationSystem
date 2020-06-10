using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Server
{
    public partial class Server
    {
        private static int CurrentYear = 2020;//当前年份，用于组合sql语句使用
        private static String CurrentSemester = "Fall";//当前学期，用于组合sql语句使用

        private static bool IsCourseRegistrationOpen = true;

        private static void DealClientInput(object ob)//处理客户端输入，需要在switch里加上各种命令的处理办法
        {
            ClientThreadObject ClientConnection = (ClientThreadObject)ob;
            Socket ClientSocket = ClientConnection.socket;
            String ClientMessage = ClientConnection.data;
            String result = String.Empty;
            string[] ClientMessages = ClientMessage.Split('@');
            //@为分隔符，判断第一个字符串
            /*为login时：登录。由孙博文于2020年6月9日19点29分完成
             * 为sql时：让服务器直接调用。由孙博文于2020年6月9日19点39分完成
             * 为teach时：教授选择课程。由孙博文于2020年6月10日13点06分开始挖坑
             * 
             * 
             * 
             * 
             * 
             * 
             */
            switch (ClientMessages[0])
            {
                case "login":
                    result = CaseLogin(ClientMessages[1]);
                    break;
                case "sql":
                    result = CaseSQL(ClientMessages[1]);
                    break;
                case "teach":
                    result = CaseTeach(ClientMessages[1]);
                    break;
                //在这里添加想要处理的命令，将结果赋值给result即可
            }
            
            //编码为字节  
            byte[] byteData = Encoding.BigEndianUnicode.GetBytes(result);
            //发回客户端
            ClientSocket.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), ClientSocket);
        }

        private static string CaseTeach(string cmd)
        {
            string result = String.Empty;
            if (IsCourseRegistrationOpen)//如果课程注册系统开启，才能处理
            {
                string[] vars = cmd.Split(',');
                if (vars.Length != 2)
                    result = "错误：Teach命令的参数个数不正确";
                else
                {
                    String sqlcmd = String.Empty;
                    switch (vars[0])
                    {
                        case "can":
                            sqlcmd = String.Format("SELECT C.* FROM can_teach as CT,section as S, course as C where CT.id = {0} and C.course_id = CT.course_id and C.course_id = S.course_id;",
                                vars[1]);
                            break;
                        case "previous":
                            sqlcmd = String.Format("SELECT C.course_id, C.title, C.dept_name, T.year, T.semester FROM teaches as T natural join course as C where T.id = {0};",
                                vars[1]);
                            break;
                        case "choose":
                            sqlcmd = String.Format("SELECT T.*, C.title, C.dept_name, C.credits, C.fee from course as C natural join teaches as T where T.id = {0};",
                                vars[1]);
                            break;
                    }
                    result = ExecuteSelectCommand(sqlcmd).ToJson();
                }

            }
            else
            {
                result = "课程注册系统已关闭";
            }
            return result;
        }

        private static String CaseSQL(string cmd)
        {
            String result = String.Empty;
            String[] sqls = cmd.Split(' ');
            if (sqls[0].Equals("SELECT") || sqls[0].Equals("select"))//选择语句，发回JSON
                result = ExecuteSelectCommand(cmd).ToJson();
            else
            {
                result = ExecuteNonQuery(cmd).ToString();//更新更改删除语句，发回受影响的行数
            }
            return result;
        }

        private static DataTable ExecuteSelectCommand(string cmd)//选择语句，发回选择结果，以DataTable形式保存
        {
            DataTable result = new DataTable();
            MySqlConnection conn = new MySqlConnection(MySQLConnectionString);
            conn.Open();
            MySqlDataAdapter sda = new MySqlDataAdapter(cmd, conn);
            sda.Fill(result);
            sda.Dispose();
            conn.Close();
            return result;
        }

        private static int ExecuteNonQuery(string cmd)//增删改语句，发回语句影响的行数
        {
            MySqlConnection conn = new MySqlConnection(MySQLConnectionString);
            conn.Open();
            MySqlCommand command = new MySqlCommand(cmd);
            int result = command.ExecuteNonQuery();
            command.Dispose();
            conn.Close();
            return result;
        }
        private static String CaseLogin(String LoginInfo)
        {
            string[] loginInfos = LoginInfo.Split(',');
            string tableName = loginInfos[0];
            string userId = loginInfos[1];
            string userPassword = loginInfos[2];
            string result = String.Empty;
            string cmd = String.Format("SELECT password FROM {0} where id = '{1}';", tableName, userId);
            try
            {
                DataTable dt = ExecuteSelectCommand(cmd);
                if (dt.Rows.Count == 1)//比较密码
                {
                    string password = (string)dt.Rows[0][0];
                    if (password.Equals(userPassword))
                        result = "True@"+ MySQLConnectionString;
                    else
                        result = "False"+"@密码错误";
                }
                else//无该用户
                {
                    result = "False" + "@找不到用户";
                }
            }
            catch (MySqlException e)
            {
                result = "False" + "@服务器连接数据库异常";
            }
            
            return result;
        }
    }
}
