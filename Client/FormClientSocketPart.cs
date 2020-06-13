using System;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using System.Linq;

namespace Client
{
    public partial class FormClient
    {
        private static String ReceiveMessage = String.Empty;//保存服务器回复的信息

        private void InitializeConnection()
        {
            //程序开始运行时可以测试一下连接
        }

        /*
         * 给远程服务器发送消息
         * 参数：data——保存想要发送的消息
         * 返回值：无
         * 额外影响：服务器发回的消息保存在ResultBuffer字符串中，请及时取走
         */
        public void GetServerMessage(String data)
        {
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 13000);
            receiveDone.Reset();
            sendDone.Reset();
            connectDone.Reset();
            try
            {
                clientSocket.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), clientSocket);
                connectDone.WaitOne();
                //发送信息
                Send(clientSocket, data + ".");
                sendDone.WaitOne();
                if (clientSocket.Connected)
                {
                    //接收信息 
                    Receive(clientSocket);
                    receiveDone.WaitOne();

                    //释放连接 
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("连接服务器失败，请检查网络通信情况", "严重故障", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private static void Send(Socket client, String data)
        {
            // Convert the string data to byte data using BigEndianUnicode encoding.  
            byte[] byteData = Encoding.BigEndianUnicode.GetBytes(data);

            // Begin sending the data to the remote device.  
            client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), client);
        }
        private static void Receive(Socket client)
        {
            try
            {
                //创建保存Socket和接收数据的结构体
                StateObject state = new StateObject();
                state.WorkSocket = client;

                //尝试开始接收数据
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(FormClient.ReceiveCallback), state);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                //取回ClientSocket
                Socket ClientSocket = (Socket)ar.AsyncState;

                //完成连接
                ClientSocket.EndConnect(ar);
            }
            catch (Exception e)
            {
                //MessageBox.Show("连接服务器失败，请检查网络通信情况","严重故障",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
            finally
            {
                //发送信号，连接完毕
                connectDone.Set();
            }
        }
        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static ManualResetEvent sendDone = new ManualResetEvent(false);
        private static ManualResetEvent receiveDone = new ManualResetEvent(false);

        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                //取回ClientSocket和保存接收数据的结构体
                StateObject state = (StateObject)ar.AsyncState;
                Socket ClientSocket = state.WorkSocket;

                //接收数据
                int bytesRead = ClientSocket.EndReceive(ar);
                if (bytesRead > 0)
                {
                    //如果读到了数据，可能因为缓存大小不足而未接收全部数据，应保存现有数据，然后递归调用继续读取
                    state.stringBuilder.Append(Encoding.BigEndianUnicode.GetString(state.buffer, 0, bytesRead));

                    //继续读取
                    ClientSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    //未读到数据，证明读取完毕
                    if (state.stringBuilder.Length > 1)
                    {
                        ReceiveMessage = state.stringBuilder.ToString();

                    }
                    //发送信号，接收完毕
                    receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                //取回Socket
                Socket ClientSocket = (Socket)ar.AsyncState;

                //记录发送字节数
                int bytesSent = ClientSocket.EndSend(ar);

                //发送完毕
                sendDone.Set();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
    }
}
