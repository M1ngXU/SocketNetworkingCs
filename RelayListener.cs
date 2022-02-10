using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using UnitySocketManager;

namespace UnitySocketManager
{
	internal class RelayListener : ILogger
	{
		string ILogger.Name => "Relay Listener";

		public static string RelayServerDomainName = "ws://relayserver.m1ngxu.repl.co/";
		internal readonly List<Action<byte[]>> Listeners = new List<Action<byte[]>>();

		private readonly ULongId ULongId;
		private ClientWebSocket Socket;

		internal RelayListener(ULongId id = null) => ULongId = id;

		internal async Task Start(Action<byte[]> OnMessage, Action<ULongId> OnClientConnected = null)
		{
			Socket = new ClientWebSocket();
			await Socket.ConnectAsync(new Uri(RelayServerDomainName), default);

			Listeners.Add(OnMessage);

			this.Log((ULongId?.ToString() ?? "SERVER".PadLeft(20)) + " connected to the relay");

			if (ULongId == null)
			{
				//Server
				await SendBinary(new byte[0]);
			}
			else
			{
				//Client
				byte[] connect = new byte[9];
				connect[0] = 0x11;
				ULongId.GetBytes().CopyTo(connect, 1);
				await SendBinary(connect);
			}

			_ = Task.Run(async () =>
			{
				MemoryStream ms = new MemoryStream();
				while (Socket.State == WebSocketState.Open)
				{
					WebSocketReceiveResult result;
					do
					{
						ArraySegment<byte> buffer = WebSocket.CreateClientBuffer(1024, 1024);
						result = await Socket.ReceiveAsync(buffer, default);
						ms.Write(buffer.Array, buffer.Offset, result.Count);
					} while (!result.EndOfMessage);

					byte[] content = ms.ToArray();

					// only possible if Server
					if (result.MessageType == WebSocketMessageType.Text)
						OnClientConnected(new ULongId(
							Encoding.ASCII
									.GetString(content)
									.Split("|")
									.Select(s => byte.Parse(s))
									.ToArray()
						));

					if (result.MessageType == WebSocketMessageType.Binary)
					{
						foreach (Action<byte[]> m in Listeners)
						{
							m(content);
						}
					}

					ms.Seek(0, SeekOrigin.Begin);
					ms.Position = 0;
				}
			});
		}

		internal Task SendBinary(byte[] content) => Socket.SendAsync(content, WebSocketMessageType.Binary, true, default);

		internal Task SendTo(ULongId id, byte[] content) => SendBinary(id.GetBytes().Concat(content).ToArray());
	}
}
