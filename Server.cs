using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UnitySocketManager
{
	internal class Server : ILogger
	{
		string ILogger.Name => "Server";

		private readonly Socket Listener;
		private readonly Dictionary<ULongId, Connection> ClientConnections = new Dictionary<ULongId, Connection>();

		internal Server(IPEndPoint ip)
		{
			Listener = new Socket(SocketType.Stream, ProtocolType.Tcp);

			try
			{
				RelayListener rl = new RelayListener();
				this.Log("Listening for incoming online connections ...");
				rl.Start(
					c => { },
					id =>
					{
						RelayServer rs = new RelayServer(id, rl);
						rs.Start(m =>
						{
							this.Log(id + " ==> Relay: " + m.Content.Length + " bytes");
							OnMessage(id, m);
						});

						ClientConnections.Add(id, rs);
						this.Log(id + " connected");
					}
				).GetAwaiter().GetResult();
			}
			catch
			{
				this.Log("Failed to connect to relay. Running local only.");
			}

			Task.Run(() =>
			{
				Listener.Bind(ip);
				Listener.Listen(100);

				this.Log("Listening for incoming socket connections ...");

				while (true)
				{
					Socket new_socket = Listener.Accept();

					byte[] buf = new byte[8];
					new_socket.Receive(buf);
					ULongId id = new ULongId(buf);
					ServerConnection s = new ServerConnection(new_socket);
					s.Start(m => OnMessage(id, m));
					ClientConnections.Add(id, s);
					this.Log(id + " connected");
				}
			});
		}

		private void OnMessage(ULongId id, Message m)
		{
			this.Log(id.ToString().PadLeft(20) + " ==> Server: " + m.Content.Length + " bytes");
			if (m.IsCallback)
			{
				this.Log(id + " responding ...");
				m.SetResponse(Encoding.ASCII.GetBytes("TESTSETS"));
				Send(id, m);
			}
		}

		internal void Send(ULongId id, Message m) => ClientConnections[id]?.Send(m);
	}
}
