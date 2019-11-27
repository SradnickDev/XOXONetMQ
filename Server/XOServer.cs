using System;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;
using Shared;

namespace Server
{
	public class XOServer
	{
		private XOGame m_game;
		private ResponseSocket m_responseSocket;
		private PublisherSocket m_publisherSocket;
		private const string PublisherTopic = "GameUpdate";

		public void Initialize()
		{
			m_game = new XOGame();
			m_responseSocket = new ResponseSocket();
			m_responseSocket.Bind("tcp://*:5050");

			m_publisherSocket = new PublisherSocket();
			m_publisherSocket.Bind("tcp://*:5051");

			Console.WriteLine("Server is running!");
			Console.WriteLine("..waiting for clients");
		}

		public void Update()
		{
			var msg = new Msg();
			msg.InitEmpty();

			while (true)
			{
				m_responseSocket.Receive(ref msg);

				if (msg.Data.Length > 0)
				{
					var header = msg.Header();
					switch (header)
					{
						case Header.JoinRequest:

							OnJoinRequest(msg);

							Thread.Sleep(1000);

							if (m_game.PlayerCount == 2)
							{
								PublishGameUpdate();
							}

							break;

						case Header.TurnRequest:
							PlayerTurn(msg);
							PublishGameUpdate();
							break;
					}
				}
			}
		}

		private void PublishGameUpdate()
		{
			var gameUpdatePacket = new GameUpdate()
			{
				CurrentPlayerId = m_game.CurrentPlayer.Id, Fields = m_game.GetField()
			};


			if (m_game.FoundWinner())
			{
				Console.WriteLine("Found Winner : " + m_game.Winner.Id);
				gameUpdatePacket.Winner = m_game.Winner.Id;
			}

			var responseMsg = Message.Create(Header.GameUpdate, gameUpdatePacket);

			Console.WriteLine(m_game.CurrentPlayer.Id);
			m_publisherSocket
				.SendMoreFrame(PublisherTopic)
				.Send(ref responseMsg, false);
		}

		private void PlayerTurn(Msg msg)
		{
			var turnRequest = msg.ToPacket<TurnRequest>();

			var result = m_game.CanMakeMove(turnRequest.Id) && m_game.ValidMove(turnRequest.Field);

			Console.WriteLine("Player Turn");

			if (result)
			{
				Console.WriteLine("Valid");
				m_game.MakeTurn(turnRequest.Id, turnRequest.Field);
			}
			else
			{
				Console.WriteLine("NotValid");
			}

			var turnResponse = new TurnResponse()
			{
				ValidMove = result
			};

			var responseMsg = Message.Create(Header.TurnResponse, turnResponse);
			m_responseSocket.Send(ref responseMsg, false);
		}

		private void OnJoinRequest(Msg msg)
		{
			var packet = msg.ToPacket<JoinRequest>();

			if (m_game.AddPlayer(packet.Id, out var sign))
			{
				AcceptClient(sign);
			}
			else
			{
				RejectClient(packet);
			}
		}

		private void AcceptClient(string sign)
		{
			var joinResponsePacket = new JoinResponse
			{
				Sign = sign, Accept = true, Address = "tcp://192.168.10.145:5051", Topic = PublisherTopic
			};

			var responseMsg = Message.Create(Header.JoinResponse, joinResponsePacket);
			m_responseSocket.Send(ref responseMsg, false);

			Console.WriteLine($"{sign} : joined the Game");
		}

		private void RejectClient(JoinRequest packet)
		{
			var joinResponsePacket = new JoinResponse {Accept = false};
			var responseMsg = Message.Create(Header.JoinResponse, joinResponsePacket);

			m_responseSocket.Send(ref responseMsg, false);

			Console.WriteLine($"{packet.Id} : rejected");
		}

		public void Shutdown()
		{
			m_responseSocket.Dispose();
			m_publisherSocket.Dispose();
		}
	}
}