using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    public partial class Server
    {
        private static readonly int CurrentYear = 2020;//当前年份，用于组合sql语句使用
        private static readonly String CurrentSemester = "秋季";//当前学期，用于组合sql语句使用

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
             * 为close时，关闭注册。
             * 为end时：选课结束。
             * 为start时：选课开始。
             * 为bill时：取得账单。
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
                case "close":
                    result = CaseClose();
                    break;
                case "end":
                    result = CaseEnd();
                    break;
                case "start":
                    result = CaseStart();
                    break;
                case "bill":
                    result = CaseBill(ClientMessages[1]);
                    break;
                //在这里添加想要处理的命令，将结果赋值给result即可


                default:
                    result = "Fail@无效命令";
                    break;
            }

            //编码为字节  
            byte[] byteData = Encoding.BigEndianUnicode.GetBytes(result);
            //发回客户端
            ClientSocket.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), ClientSocket);
        }

        private static string CaseBill(String ClientMessage)
        {
            string result = "True@";
            string[] messages = ClientMessage.Split(',');
            if (messages.Length == 3)
            {
                string id = messages[0];
                string year = messages[1];
                string semester = messages[2];
                try
                {
                    using (StreamReader reader = new StreamReader(new FileStream(
                            String.Format(Directory.GetCurrentDirectory() + "\\Bills\\" + "{1}{2}\\" + "{0}_{1}{2}.txt", id, year, semester),
                            FileMode.Open)))
                    {
                        result += reader.ReadToEnd();
                    }
                }
                catch (FileNotFoundException e)
                {
                    result = "False@找不到账单文件";
                }
                catch (DirectoryNotFoundException e)
                {
                    result = "False@找不到账单文件";
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            else
            {
                result = "False@网络传输异常";
            }
            Console.WriteLine("生成账单文件");
            return result;
        }

        private static string CaseStart()
        {
            if (!IsCourseRegistrationOpen)
            {
                IsCourseRegistrationOpen = true;
                Console.WriteLine("管理员选择开始选课");
                return "Success@选课开始";
            }
            else
            {
                return "Fail@选课已开始";
            }
        }

        private static string CaseEnd()
        {
            if (IsCourseRegistrationOpen)
            {
                IsCourseRegistrationOpen = false;
                Console.WriteLine("管理员选择暂停选课");
                return "Success@选课结束";
            }
            else
            {
                return "Fail@选课已结束";
            }
        }

        private static string CaseClose()
        {
            if (IsCourseRegistrationOpen)
                return "Fail@选课正在进行，无法进行操作";
            else
            {
                new Thread(CloseRegistration).Start();
                Console.WriteLine("管理员选择结束选课，正在安排课程");
                return "Success@开始安排课程";
            }
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
                            sqlcmd = String.Format("select S.* from section as S where (S.course_id in (select CT.course_id from can_teach as CT where CT.id = {0}) and isnull(S.id));",
                                vars[1]);
                            break;
                        case "previous":
                            sqlcmd = String.Format("SELECT * FROM section natural join course where id = {0} and year < {1};",
                                vars[1], CurrentYear);
                            break;
                        case "choose":
                            sqlcmd = String.Format("SELECT * FROM section where id = {0};",
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
            using (MySqlConnection conn = new MySqlConnection(MySQLConnectionString))
            {
                try
                {
                    conn.Open();
                    MySqlDataAdapter sda = new MySqlDataAdapter(cmd, conn);
                    sda.Fill(result);
                    sda.Dispose();
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
            return result;
        }

        private static int ExecuteNonQuery(string cmd)//增删改语句，发回语句影响的行数
        {
            int result = -1;
            using (MySqlConnection conn = new MySqlConnection(MySQLConnectionString))
            {
                try
                {
                    conn.Open();
                    MySqlCommand command = new MySqlCommand(cmd);
                    result = command.ExecuteNonQuery();
                    command.Dispose();
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
                if (loginInfos[1] == String.Empty)
                {
                    result = "False" + "@请输入用户名";
                    return result;
                }
                else if (loginInfos[2] == String.Empty)
                {
                    result = "False" + "@请输入密码";
                    return result;
                }
                DataTable dt = ExecuteSelectCommand(cmd);
                if (dt.Rows.Count == 1)//比较密码
                {
                    string password = (string)dt.Rows[0][0];
                    if (password.Equals(userPassword))
                    {
                        result = "True@" + MySQLConnectionString;
                        if (loginInfos[0].Equals("professor"))
                        {
                            Console.WriteLine("一名教授登录了客户端");
                        }
                        if (loginInfos[0].Equals("registrar"))
                        {
                            Console.WriteLine("一名管理员登录了客户端");
                        }
                        if (loginInfos[0].Equals("student"))
                        {
                            Console.WriteLine("一名学生登录了客户端");
                        }
                        if (IsCourseRegistrationOpen)
                        {
                            result += "@CourseRegistrationOpen";
                        }
                        else
                        {
                            result += "@CourseRegistrationClose";
                        }
                    }
                    else
                        result = "False" + "@密码错误";
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
