using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    public partial class Server
    {
        static int Main(string[] args)
        {
            MySQLConnectionString = InitializeMySQLConnection("MySQLSettings.xml");
            Console.WriteLine("数据库连接加载完毕");
            StartListening();
            return 0;
        }

        public static ManualResetEvent allDone = new ManualResetEvent(false);

        public static void StartListening()
        {
            IPAddress ServerIpAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint ServerEndPoint = new IPEndPoint(ServerIpAddress, 13000);
            //创建服务器
            Socket ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                ServerSocket.Bind(ServerEndPoint);
                Console.WriteLine("服务器开始运行，等待客户端建立连接");
                ServerSocket.Listen(100);
                while (true)
                {
                    //阻断线程 
                    allDone.Reset();
                    ServerSocket.BeginAccept(new AsyncCallback(AcceptCallback), ServerSocket);
                    //建立连接后主线程继续  
                    allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            Console.WriteLine("服务器结束运行");
            Console.Read();
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            //主线程继续
            allDone.Set();
            //取回Socket
            Socket ServerSocket = (Socket)ar.AsyncState;

            Socket HandlerSocket = ServerSocket.EndAccept(ar);

            //创建保存Socket和数据的结构体
            StateObject state = new StateObject();
            state.workSocket = HandlerSocket;
            HandlerSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            //取回处理连接的Socket
            StateObject state = (StateObject)ar.AsyncState;
            Socket HandlerSocket = state.workSocket;

            //读取数据
            int bytesRead = HandlerSocket.EndReceive(ar);
            if (bytesRead > 0)
            {
                //如果读到了数据，可能因为缓存大小不足而未接收全部数据，应保存现有数据，然后递归调用继续读取
                state.stringBuilder.Append(Encoding.BigEndianUnicode.GetString(
                    state.buffer, 0, bytesRead));

                //检查结束符位置
                content = state.stringBuilder.ToString();
                if (content.IndexOf(".") > -1)
                {
                    //读到了结束符，证明数据读取结束
                    //把结束符删去，新建一个线程，处理客户端输入的指令
                    content = content.Remove(content.Length - 1);
                    ClientThreadObject ClientConnection = new ClientThreadObject();
                    ClientConnection.socket = HandlerSocket;
                    ClientConnection.data = content;
                    new Thread(DealClientInput).Start(ClientConnection);
                }
                else
                {
                    //未读到结束符，需要继续读取数据
                    HandlerSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
            }
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                //取回Socket 
                Socket HandlerSocket = (Socket)ar.AsyncState;

                //完成发送
                int bytesSent = HandlerSocket.EndSend(ar);

                //关闭连接
                HandlerSocket.Shutdown(SocketShutdown.Both);
                HandlerSocket.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
