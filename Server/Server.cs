using System;

namespace Server
{
    public partial class Server
    {
        static void Main(string[] args)
        {
            MySQLConnectionString = InitializeMySQLConnection("MySQLSettings.xml");
        }
    }
}
