using System;

using io.netpie.microgear;

namespace Example
{
    class Program
    {
        private static String AppID = < AppID >;
        private static String Key = < Key >;
        private static String Secret = < Secret >;
        private static Microgear microgear;
        static void Main(string[] args)
        {
            microgear = new Microgear();
            microgear.onConnect += connect;
            microgear.onMessage += message;
            microgear.onAbsent += absent;
            microgear.onPresent += present;
            microgear.onDisconnect += disconnect;
            microgear.onError += error;
            microgear.Connect(AppID, Key, Secret);
            microgear.SetAlias("Csharp");
            microgear.Subscribe("/topic");
            for (int i = 0; i < 100; i++)
            {
                microgear.Chat("Csharp", i.ToString());
                System.Threading.Thread.Sleep(3000);
            }
        }

        public static void connect()
        {
            Console.WriteLine("Now I'm connecting with NETPIE");
        }

        public static void disconnect()
        {
            Console.WriteLine("disconnect");
        }

        public static void message(string topic,string message)
        {
            Console.WriteLine(topic + " " + message);
        }

        public static void present(string message)
        {
            Console.WriteLine(message);
        }

        public static void absent(string message)
        {
            Console.WriteLine(message);
        }

        public static void error(string message)
        {
            Console.WriteLine(message);
        }
    }
}
