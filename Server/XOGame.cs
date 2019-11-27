using System;
using System.Collections.Generic;

namespace Shared
{
	public class Player
	{
		public long Id { get; private set; }
		public string Sign { get; private set; }

		public Player(long id, string sign)
		{
			Id = id;
			Sign = sign;
		}
	}

	public class XOGame
	{
		public int PlayerCount => m_players.Count;

		private readonly string[] Fields = new string[]
		{
			".", ".", ".", ".", ".", ".", ".", ".", ".",
		};

		private string[] m_sign = {"X", "O"};

		private List<Player> m_players = new List<Player>();

		private int m_currentPlayer = 0;

		public Player CurrentPlayer => m_players[m_currentPlayer];
		public Player Winner { get; private set; }

		public bool AddPlayer(long id, out string sign)
		{
			sign = "";
			if (m_players.Count < 2 && m_players.FindIndex(x => x.Id == id) == -1)
			{
				sign = m_sign[m_players.Count];

				m_players.Add
				(
					new Player(id, m_sign[m_players.Count])
				);
				m_currentPlayer = 0;
				return true;
			}

			return false;
		}

		public void MakeTurn(long id, int field)
		{
			var player = m_players.Find(x => x.Id == id);

			field -= 1;
			Fields[field] = player.Sign;
			Console.WriteLine(player.Sign + " made a move.");
			m_currentPlayer = (m_currentPlayer + 1) % m_players.Count;
		}

		public bool CanMakeMove(long id)
		{
			var player = m_players.Find(x => x.Id == id);

			return player != null &&
				   m_players[m_currentPlayer].Id == id &&
				   m_players.Count >= 2;
		}

		public bool ValidMove(int field)
		{
			//offset
			field -= 1;

			return field <= Fields.Length && field >= 0 &&
				   Fields[field].Contains(".");
		}

		public string GetField()
		{
			var retVal = "";
			for (var i = 1; i <= Fields.Length; i++)
			{
				retVal += Fields[i - 1];
				if (i % 3 == 0) retVal += "\n";
			}

			return retVal;
		}

		public bool FoundWinner()
		{
			if (CompareFields(Fields[0], Fields[1], Fields[2]))
			{
				Winner = m_players.Find(x => x.Sign == Fields[0]);
				return true;
			}
			else if (CompareFields(Fields[3], Fields[4], Fields[5]))
			{
				Winner = m_players.Find(x => x.Sign == Fields[3]);
				return true;
			}
			else if (CompareFields(Fields[6], Fields[7], Fields[8]))
			{
				Winner = m_players.Find(x => x.Sign == Fields[6]);
				return true;
			}
			else if (CompareFields(Fields[0], Fields[3], Fields[6]))
			{
				Winner = m_players.Find(x => x.Sign == Fields[0]);
				return true;
			}
			else if (CompareFields(Fields[1], Fields[4], Fields[7]))
			{
				Winner = m_players.Find(x => x.Sign == Fields[1]);
				return true;
			}
			else if (CompareFields(Fields[2], Fields[5], Fields[8]))
			{
				Winner = m_players.Find(x => x.Sign == Fields[2]);
				return true;
			}
			else if (CompareFields(Fields[6], Fields[4], Fields[2]))
			{
				Winner = m_players.Find(x => x.Sign == Fields[6]);
				return true;
			}
			else if (CompareFields(Fields[0], Fields[4], Fields[8]))
			{
				Winner = m_players.Find(x => x.Sign == Fields[0]);
				return true;
			}

			Winner = null;
			return false;
		}

		private bool CompareFields(string a, string b, string c)
		{
			return a == b && b == c && a != ".";
		}
	}
}