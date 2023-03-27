using System;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Example
{
  public class Laputa : WebSocketBehavior
  {
    protected override void OnMessage (MessageEventArgs e)
    {
      var msg = e.Data == "BALUS"
                ? "Are you kidding?"
                : "I'm not available now.";

      Send (msg);
    }
  }

  public class Program
  {
    public static void Main (string[] args)
    {
      var wssv = new WebSocketServer ("ws://localhost:8080");

      wssv.AddWebSocketService<Laputa> ("/Laputa");
      wssv.Start();
      System.Console.WriteLine("Server started");
      Console.ReadKey(true);
      wssv.Stop();
    }
  }
}