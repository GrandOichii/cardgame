using System.Net;
using System.Net.Sockets;
using System.Text;

using Shared;

class Client {
    static int BUFFER_SIZE = 4096;
    static byte[] buffer = new byte[BUFFER_SIZE];
    static TcpClient client = new TcpClient();


    static void Main(string[] args)
    {
        var host = "127.0.0.1";
        if (args.Length == 1) host = args[0];
        var endpoint = new IPEndPoint(IPAddress.Parse(host), 8080);
        client.Connect(endpoint);
        System.Console.WriteLine("Connected");
        var stream = client.GetStream();
        while (true) {
            System.Console.WriteLine("Reading message");
            var prompt = Read();
            if (prompt == null || prompt == "") break;
            System.Console.WriteLine("Received: " + prompt);
            var stateJ = Read();
            var state = MatchState.From(stateJ);
            System.Console.WriteLine("State parsed");
            Write();
        }
        client.Close();
    }

    static string Read() {
        var stream = client.GetStream();
        // var message = "";
        // int received;
        // // while ((received = stream.Read(buffer)) != 0) {
        // //     message += Encoding.UTF8.GetString(buffer, 0, received);
        // //     System.Console.WriteLine("READ " + received);
        // // }
        // received = stream.Read(buffer);
        // message += Encoding.UTF8.GetString(buffer, 0, received);
        var message = NetUtil.Read(stream);
        return message;
    }

    static void Write() {
        System.Console.Write("> ");
        var stream = client.GetStream();
        var message = Console.ReadLine();
        if (message is null) message = "";

        // var data = Encoding.UTF8.GetBytes(message);
        // stream.Write(data);
        NetUtil.Write(stream, message);
    }
}