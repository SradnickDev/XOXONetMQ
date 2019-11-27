namespace Shared
{
	public abstract class Packet { }

	public class JoinRequest : Packet
	{
		public long Id;
	}

	public class JoinResponse : Packet
	{
		public bool Accept;
		public string Sign;
		public string Address;
		public string Topic;
	}

	public class TurnRequest : Packet
	{
		public long Id;
		public int Field;
	}

	public class TurnResponse : Packet
	{
		public bool ValidMove;
	}

	public class GameUpdate : Packet
	{
		public long CurrentPlayerId;
		public string Fields;
		public long Winner;
	}

}