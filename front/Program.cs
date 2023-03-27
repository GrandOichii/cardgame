﻿using System.Net;
using System.Net.Sockets;
using System.Text;

class Client {
    static TcpClient client = new TcpClient();

    static void Main(string[] args)
    {
        var endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080);

        System.Console.WriteLine("Connected");
        client.Connect(endpoint);
        var stream = client.GetStream();
        while (true) {
            System.Console.WriteLine("Reading message");
            var message = Read();
            System.Console.WriteLine("Received: " + message);
            Write();
            if (message == "quit") break;
        }
        client.Close();
    }

    static string Read() {
        var stream = client.GetStream();
        var buffer = new byte[1024];
        var message = "";
        int received;
        // while ((received = stream.Read(buffer)) != 0) {
        //     message += Encoding.UTF8.GetString(buffer, 0, received);
        //     System.Console.WriteLine("READ " + received);
        // }
        received = stream.Read(buffer);
        message += Encoding.UTF8.GetString(buffer, 0, received);
        System.Console.WriteLine("READ " + received);
        return message;
    }

    static void Write() {
        System.Console.Write("> ");
        var stream = client.GetStream();
        var message = Console.ReadLine();
        if (message is null) message = "";

        var data = Encoding.UTF8.GetBytes(message);
        stream.Write(data);
    }
}