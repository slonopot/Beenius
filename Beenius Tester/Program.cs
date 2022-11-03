using Beenius;

namespace Beenius_Tester
{
    internal class Program
    {
        static void Main(string[] args)
        {
            GeniusClient client = new GeniusClient("");
            client.getLyrics("GERM", "SORRY MOM I WAS FRIED");
        }
    }
}
