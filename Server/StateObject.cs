using System;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    public class StateObject
    {
        public Socket workSocket = null;
        public static int BufferSize = 256;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder stringBuilder = new StringBuilder();
    }

    public class ClientThreadObject
    {
        public Socket socket = null;
        public String data = null;
    }
}
