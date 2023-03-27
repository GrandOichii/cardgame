using System.Net;
using System.Net.Sockets;
using System.Text;

class Server {
	static void Main(string[] args)
	{
        var endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9090);
        var listener = new TcpListener(endpoint);
        listener.Start();

        var handler = listener.AcceptTcpClient();
        var stream = handler.GetStream();

        var message = "Hello, world";
        var data = Encoding.UTF8.GetBytes(message);
        stream.Write(data);
        System.Console.WriteLine("Wrote: " + message);

        listener.Stop();
	}
}