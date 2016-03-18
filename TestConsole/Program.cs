using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole
{
    class Program
    {
        const string uri = "tcp://127.0.0.1:7777";

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                var client = new Client(uri);
                do
                {
                    var line = Console.ReadLine();
                    client.Invoke(Encoding.UTF8.GetBytes(line));

                } while (true);
            }
            else
            {
                var server = new Server(uri);

                Console.ReadLine();
            }

        }
    }
}
