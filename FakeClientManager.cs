using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnitySocketManager;

namespace LocalRelayServer
{
	public class FakeClientManager : ILogger
	{
		string ILogger.Name => "Fake-Client Manager";

		public FakeClientManager(IPEndPoint Server)
		{
			RelayListener rl = new RelayListener();

			Dictionary<ULongId, FakeClient> Clients = new Dictionary<ULongId, FakeClient>();
			_ = rl.Start(
				c => Clients[new ULongId(c.Take(8))]?.Send(c.Skip(8).ToArray()),
				id =>
				{
					Clients.Add(
						id,
						new FakeClient(Server, id, b =>
						{
							this.Log(id + " ==> Relay: " + b.Length + " bytes");
							rl.SendTo(id, b);
						})
					);
					this.Log(id + " connected");
				}
			);
		}
	}
}
