using Beenius;
using System;


namespace Beenius_Tester
{
    internal class Program
    {
        static void Main(string[] args)
        {
            GeniusClient client = new GeniusClient();
            client.getLyrics("$uicideboy$", "broke(n)", string.Empty);
        }
    }
}
