using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class SocketLib
{
	private Thread thread;
	private TcpClient client;
	private TcpListener listener;

	public bool IsServer { get; private set; } = false;
	public bool IsClient { get; private set; } = false;

	private Action<Message> onMessageRecieve;
	public void AddMessageRecieve(Action<Message> msg) => onMessageRecieve += msg;

	public bool StartWithServer()
	{
		if (IsServer || IsClient) return false;

		thread = new Thread(new ThreadStart(RunServerListen));
		thread.IsBackground = true;
		thread.Start();

		return IsServer = true;
	}

	public bool StartWithClient()
	{
		if (IsServer || IsClient) return false;

		thread = new Thread(new ThreadStart(RunClientListen));
		thread.IsBackground = true;
		thread.Start();

		return IsClient = true;
	}

	private void RunClientListen()
	{
		onMessageRecieve(new Message() { head = 0, data = "Client Start!" });

		client = new TcpClient("localhost", 5252);

		byte[] bytes = new byte[1024];

		while (true)
		{
			using (NetworkStream ns = client.GetStream())
			{
				int length;
				while ((length = ns.Read(bytes, 0, bytes.Length)) != 0)
				{
					var data = new byte[length];
					Array.Copy(bytes, 0, data, 0, length);
					string str = Encoding.UTF8.GetString(data);

					var arr = str.Split('|');
					Message msg = new Message();
					msg.head = int.Parse(arr[0]);
					msg.data = arr[1];
					onMessageRecieve(msg);
					bytes.Initialize();
				}
			}
		}
	}

	private void RunServerListen()
	{
		onMessageRecieve(new Message() { head = 0, data = "Server Start!" });

		listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 5252);
		listener.Start();

		byte[] bytes = new byte[1024];

		while (true)
		{
			using (client = listener.AcceptTcpClient())
			{
				onMessageRecieve(new Message() { head = 0, data = "Detected Client!" });
				using (NetworkStream ns = client.GetStream())
				{
					int length;
					while ((length = ns.Read(bytes, 0, bytes.Length)) != 0)
					{
						var data = new byte[length];
						Array.Copy(bytes, 0, data, 0, length);
						string str = Encoding.UTF8.GetString(data);

						var arr = str.Split('|');
						Message msg = new Message();
						msg.head = int.Parse(arr[0]);
						msg.data = arr[1];
						onMessageRecieve(msg);
						bytes.Initialize();
					}
				}
			}
		}
	}

	public void SendMessage(int head, string msg)
	{
		if (!IsClient && !IsServer)
		{
			Debug.LogError("Require Connection");
			return;
		}
		if (client == null)
		{
			Debug.LogError("Require connected Socket");
		}

		onMessageRecieve(new Message() { head = head, data = msg });

		try
		{
			NetworkStream ns = client.GetStream();
			{
				if (ns.CanWrite)
				{
					string str = $"{head}|{msg}";
					byte[] bytes = Encoding.UTF8.GetBytes(str);
					ns.Write(bytes, 0, bytes.Length);
				}
			}
		}
		catch { }
	}
}