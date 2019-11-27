using System;
using System.Text;
using NetMQ;
using Newtonsoft.Json;

namespace Shared
{
	public static class Message
	{
		public static Msg Create(Header header, Packet packet)
		{
			var newMsg = new Msg();
			var serializedContent = ((byte) header) + "|" + JsonConvert.SerializeObject(packet);
			var bytes = Encoding.Default.GetBytes(serializedContent);
			newMsg.InitPool(bytes.Length);
			newMsg.Put(bytes, 0, bytes.Length);
			return newMsg;
		}
		

		public static Header Header(this Msg msg)
		{
			var content = Encoding.Default.GetString(msg.Data).Split('|');
			Enum.TryParse(content[0], out Header result);
			return result;
		}

		public static T ToPacket<T>(this Msg msg) where T : Packet
		{
			var content = Encoding.Default.GetString(msg.Data).Split('|');
			var packet = JsonConvert.DeserializeObject<T>(content[1]);
			return packet;
		}
	}
}