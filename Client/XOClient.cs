using System;
using System.Text;
using NetMQ;
using NetMQ.Sockets;
using Shared;

namespace Client
{
	public class XOClient
	{
		private RequestSocket m_requestSocket;
		private SubscriberSocket m_subscriberSocket;
		private long m_id;
		private bool m_canMove = false;
		private bool m_foundWinner = false;

		public void Initialize()
		{
			m_id = Guid.NewGuid().GetHashCode();
			m_requestSocket = new RequestSocket();
			m_requestSocket.Connect("tcp://192.168.10.145:5050");
			m_subscriberSocket = new SubscriberSocket();

			Console.WriteLine("Client is running...");
		}

		public void Update()
		{
			var msg = new Msg();
			msg.InitEmpty();

			if (Join() == false)
			{
				Console.WriteLine("YOU SHALL NOT PASS");
				Console.ReadKey();
				return;
			}

			while (true)
			{
				OnGameUpdate();

				if (m_foundWinner)
				{
					Console.ReadKey();
					return;
				}
				
				if (m_canMove)
				{
					Console.WriteLine("Enter a Field number!");
					var key = Console.ReadKey();

					if (int.TryParse(key.KeyChar.ToString(), out var result))
					{
						msg = Message.Create(Header.TurnRequest, new TurnRequest()
						{
							Field = result, Id = m_id
						});

						m_requestSocket.Send(ref msg, false);
						m_requestSocket.Receive(ref msg);

						var resp = msg.ToPacket<TurnResponse>();
						Console.WriteLine(resp.ValidMove);
					}
				}
				else
				{
					Console.WriteLine("Wait for Opponent..");
				}
			}
		}

		private bool Join()
		{
			SendJoinRequest();
			var response = ReceiveJoinResponse();

			if (response.Accept && response != null)
			{
				OnJoinSuccessful(response);
			}
			else
			{
				OnJoinDeclined();
				return false;
			}

			return true;
		}

#region Join Process

		private void SendJoinRequest()
		{
			var joinRequestPacket = new JoinRequest() {Id = m_id};
			var joinRequestMsg = Message.Create(Header.JoinRequest, joinRequestPacket);
			m_requestSocket.Send(ref joinRequestMsg, false);
			Console.WriteLine("Send a join request");
		}

		private JoinResponse ReceiveJoinResponse()
		{
			var msg = new Msg();
			msg.InitEmpty();

			m_requestSocket.Receive(ref msg);
			var responsePacket = msg.ToPacket<JoinResponse>();
			var header = msg.Header();

			if (header == Header.JoinResponse)
			{
				return responsePacket;
			}

			return null;
		}

		private void OnJoinSuccessful(JoinResponse response)
		{
			Console.WriteLine("Server accepted!");
			Subscribe(response);
		}

		private void OnJoinDeclined()
		{
			Console.WriteLine("Server declined!");
		}

#endregion

		private void Subscribe(JoinResponse response)
		{
			m_subscriberSocket.Connect(response.Address);
			m_subscriberSocket.Subscribe(response.Topic);
		}

		public void OnGameUpdate()
		{
			var msg = new Msg();
			msg.InitEmpty();

			var topic = m_subscriberSocket.ReceiveFrameString();
			m_subscriberSocket.Receive(ref msg);

			var packet = msg.ToPacket<GameUpdate>();

			DisplayField(packet.Fields);
			m_canMove = packet.CurrentPlayerId == m_id;

			if (packet.Winner != 0)
			{
				m_foundWinner = true;
				
				if (packet.Winner == m_id)
				{
					Console.Clear();
					Console.WriteLine("You Won!");
				}
				else
				{
					Console.Clear();
					Console.WriteLine("You Lost..");
				}
			}
		}

		private void DisplayField(string str)
		{
			Console.Clear();
			Console.WriteLine(str);
		}

		public void Shutdown()
		{
			m_requestSocket.Dispose();
		}
	}
}