using System;
using WebSocketSharp;

namespace Example
{
  public class Program
  {
    public static void Main (string[] args)
    {
      using (var ws = new WebSocket ("ws://localhost:8080/Laputa")) {
        ws.OnMessage += (sender, e) =>
                          Console.WriteLine ("Laputa says: " + e.Data);

        ws.Connect ();
        ws.Send ("BALUS");
        Console.ReadKey (true);
      }
    }
  }
}