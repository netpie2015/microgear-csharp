using System;
using io.netpie.microgear;
using System.Threading;

namespace Basic
{
    class Program
    {
        private static String AppID = < appid >;
        private static String Key = < key >;
        private static String Secret =  < secret >;
        private static Microgear microgear;
        static void Main(string[] args)
        {
            microgear = new Microgear();
            microgear.onConnect += Connect;
            microgear.onMessage += Message;
            microgear.onAbsent += Absent;
            microgear.onPresent += Present;
            microgear.onError += Error;
            microgear.Connect(AppID, Key, Secret);
            microgear.SetAlias("test");
            microgear.Subscribe("/topic");
            for (int i = 0; i < 10; i++)
            {
                microgear.Chat("test", "test message no." + i.ToString());
                Thread.Sleep(2000);
            }
        }

        public static void Connect()
        {
            Console.WriteLine("Now I'm connecting with NETPIE");
        }

        public static void Message(string topic, string message)
        {
            Console.WriteLine(topic + " " + message);
        }

        public static void Present(string token)
        {
            Console.WriteLine(token);
        }

        public static void Absent(string token)
        {
            Console.WriteLine(token);
        }

        public static void Error(string error)
        {
            Console.WriteLine(error);
        }
    }
}
