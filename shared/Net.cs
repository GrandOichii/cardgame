using System.Text;
using System.Net.Sockets;

namespace Shared;

static public class NetUtil
{
    static readonly int BUFFER_SIZE = 1024;
    static readonly byte[] BUFFER = new byte[BUFFER_SIZE];
    static public string Read(NetworkStream stream) {
        var count = stream.ReadByte();
        var result = "";
        
        for (byte i = 0; i < count; i++) {
            int received = stream.Read(BUFFER);
            result += Encoding.UTF8.GetString(BUFFER, 0, received);
        
        }
        
        return result;
    }    

    static public void Write(NetworkStream stream, string message) {
        stream.WriteByte((byte)(message.Length / BUFFER_SIZE + 1));
        var data = Encoding.UTF8.GetBytes(message);
        stream.Write(data);
    }
}
