using System.Net.Sockets;
using System.Text;

namespace Client
{
    public class StateObject
    {
        public Socket WorkSocket = null;
        public static int BufferSize = 256;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder stringBuilder = new StringBuilder();
    }
}
