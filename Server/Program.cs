namespace Server
{
	class Program
	{
		static void Main(string[] args)
		{
			var server = new XOServer();
			server.Initialize();
			server.Update();
			server.Shutdown();
		}
	}
}