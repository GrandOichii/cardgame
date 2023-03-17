using System;
using WebSocketSharp;
using WebSocketSharp.Server;

public class Laputa : WebSocketBehavior
{
    protected override void OnMessage (MessageEventArgs e)
    {
        System.Console.WriteLine(e.Data);
        // var msg = e.Data == "BALUS"
            // ? "Are you kidding?"
            // : "I'm not available now.";

        // Send (msg);
    }

    protected override void OnError(WebSocketSharp.ErrorEventArgs e)
    {
        System.Console.WriteLine("ERROR");
        base.OnError(e);
    }

    protected override void OnOpen()
    {
        base.OnOpen();
        System.Console.WriteLine("Connection established");
    }
    public static void Run ()
    {
        var wssv = new WebSocketServer(9090);
        wssv.AddWebSocketService<Laputa>("/Laputa");
        wssv.Start ();
        System.Console.WriteLine("Server started");
        Console.ReadKey (true);
        wssv.Stop ();
    }
}
