using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace UnitySocketManager
{
	internal class Program
	{
		private static void Main()
		{
			new MainRunner().Run();
			while (true) { }
		}

		private class MainRunner : ILogger
		{
			string ILogger.Name => "Main";

			internal void Run()
			{
				this.Log("START");
				new Server(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5050));
				var s = new SocketClient(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5050), new ULongId(0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF));
				s.Start(m => this.Log(m.Content));
				this.Log(s.BlockUntilCallback(new Message(new byte[] { 65, 65, 65 }, 12, true)).Content);
				while (true) { }
			}
		}
	}

	internal static class SocketEx
	{
		internal static bool IsConnected(this Socket s) => !((s.Poll(1000, SelectMode.SelectRead) && (s.Available == 0)) || !s.Connected);

		internal static void ReceiveMessages(this Socket s, MessageReceiver mr, Action<Message> OnMessage)
		{
			Task.Run(() =>
			{
				while (s.IsConnected())
				{
					byte[] buf = new byte[1];
					int read = s.Receive(buf);
					if (read == 0) break;

					Message m = mr.HandleMessage(buf[0], s.Receive);

					if (m != null) OnMessage(m);
				}
				s.Dispose();
			});
		}
	}
}
