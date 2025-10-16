using Beenius;

namespace Beenius_Tester
{
    internal class Program
    {
        static void Main(string[] args)
        {
            GeniusClient client = new GeniusClient("");
            var result = client.getLyrics("​​Chetta", "Fortykay");
        }
    }
}
