using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using MySql.Data.MySqlClient;
using System.Web.Script.Serialization;

namespace Server
{
    public static partial class Server
    {
        private static string mysqlConnectionString;//保存MySQL连接信息的字符串

        public static String MySQLConnectionString { get { return mysqlConnectionString; }  set { mysqlConnectionString = value; } }
        //使用MySqlConnection conn = new MySqlConnection(MySQLConnectionString);来创建数据库连接
        private static void DealClientInput(object ob)
        {
            ClientThreadObject ClientConnection = (ClientThreadObject)ob;
            Socket ClientSocket = ClientConnection.socket;
            String ClientMessage = ClientConnection.data;
            String result = String.Empty;
            DataTable dt = new DataTable();
            using (MySqlConnection conn = new MySqlConnection(MySQLConnectionString))
            {
                conn.Open();
                MySqlDataAdapter sda = new MySqlDataAdapter(ClientMessage, conn);
                sda.Fill(dt);
                result = dt.ToJson();
                sda.Dispose();
            }
            //编码为字节  
            byte[] byteData = Encoding.BigEndianUnicode.GetBytes(result);
            //发回客户端
            ClientSocket.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), ClientSocket);
        }

        private static string InitializeMySQLConnection(String SettingFilePath)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(SettingFilePath);
            XmlNode xmlNode = xml.SelectSingleNode("mysql");
            XmlNodeList settings = xmlNode.ChildNodes;
            String server = settings.Item(0).InnerText;
            String port = settings.Item(1).InnerText;
            String user = settings.Item(2).InnerText;
            String password = settings.Item(3).InnerText;
            String database = settings.Item(4).InnerText;
            return String.Format("server={0};port={1};user={2};password={3}; database={4};",
                server, port, user, password, database);
        }
    }
}
