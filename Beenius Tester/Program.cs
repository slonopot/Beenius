using Beenius;


namespace Beenius_Tester
{
    internal class Program
    {
        static void Main(string[] args)
        {
            GeniusClient client = new GeniusClient();
            client.getLyrics("Denzel Curry & 6LACK & Rico Nasty & JID & Jasiah & Powers Pleasant & Kitty Ca$h", "Ain't No Way", string.Empty);
        }
    }
}
