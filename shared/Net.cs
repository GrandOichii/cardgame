using System.Text;
using System.Net.Sockets;

namespace Shared;

static public class NetUtil
{

    static public string Read(NetworkStream stream) {
        byte[] lengthBuffer = new byte[4];
        int bytesRead = stream.Read(lengthBuffer, 0, lengthBuffer.Length);
        if (bytesRead == 0) return ""; // Client disconnected
        int messageLength = BitConverter.ToInt32(lengthBuffer, 0);
        byte[] buffer = new byte[messageLength];
        bytesRead = stream.Read(buffer, 0, buffer.Length);
        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        return message;
    }    

    static public void Write(NetworkStream stream, string message) {
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        byte[] lengthBuffer = BitConverter.GetBytes(buffer.Length);
        stream.Write(lengthBuffer, 0, lengthBuffer.Length);
        stream.Write(buffer, 0, buffer.Length);
    }
}
