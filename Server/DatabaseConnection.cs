using System;
using System.Xml;

namespace Server
{
    public static partial class Server
    {
        private static string mysqlConnectionString;//保存MySQL连接信息的字符串

        public static String MySQLConnectionString { get { return mysqlConnectionString; }  set { mysqlConnectionString = value; } }
        //使用MySqlConnection conn = new MySqlConnection(MySQLConnectionString);来创建数据库连接
        
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
