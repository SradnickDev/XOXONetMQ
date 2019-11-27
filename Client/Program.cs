
namespace Client
{
	class Program
	{
		static void Main(string[] args)
		{
			var xoClient = new XOClient();
			xoClient.Initialize();
			xoClient.Update();
			xoClient.Shutdown();
		}
	}
}