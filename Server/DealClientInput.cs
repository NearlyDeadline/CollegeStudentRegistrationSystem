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
             * 待续...
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
                //在这里添加想要处理的命令，将结果赋值给result即可
            }
            
            //编码为字节  
            byte[] byteData = Encoding.BigEndianUnicode.GetBytes(result);
            //发回客户端
            ClientSocket.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), ClientSocket);
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
