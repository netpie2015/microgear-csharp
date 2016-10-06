#Microgear-csharp
-----------
microgear-csharp is a client library for  C#  The library is used to connect application code or hardware with the NETPIE Platform's service for developing IoT applications. For more details on the NETPIE Platform, please visit https://netpie.io . 



##Installation
-----------
```sh
Install-Package Microgear
```


##Usage Example
-----------
```C#
using io.netpie.microgear;

namespace ConsoleApplication1
{
    class Program
    {
        private static String AppID = <appid>;
        private static String Key = <key>;
        private static String Secret =  <secret>;
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
                microgear.Publish("test", "test message no."+i.ToString());
            }
        }

        public static void Connect()
        {
            Console.WriteLine("Now I'm connecting with NETPIE");
        }

        public static void Message(string topic,string message)
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

```


##Library Usage
------------
###Microgear
---------------
**Connect(AppID, Key, Secret)**

arguments

 * *AppID* `string` - defines a group of application that microgear 
 * *Key* `string` -  is used as a microgear identity
 * *Secret* `string` - comes in a pair with gearkey. The secret is used for authentication and integrity. 

<br/>


**SetAlias(*alias*):** microgear can set its own alias, which to be used for others make a function call chat(). The alias will appear on the key management portal of netpie.io .

argument

* *alias* `string` - name of this microgear 

<br/>





**Chat(*gearname*, *message*):** sending a message to a specified gearname 

arguments

* *gearname* `string` - name of microgear to which to send a message. 
* *message* `string` - message to be sent.

<br/>






**Publish(*topic*, *message*, *retain*):** In the case that the microgear want to send a message to an unspecified receiver, the developer can use the function publish to the desired topic, which all the microgears that subscribe such topic will receive a message.

arguments

* *topic* `string` - name of topic to be send a message to. 
* *message* `string` - message to be sent.
* *retain* `boolean` - retain a message or not (the default is `False`) If `True`, the message is kept.  To remove the retained message, publish an empty message or  "" which is a message with length 0. 

<br/>




**Subscribe(*topic*)** microgear may be interested in some topic.  The developer can use the function subscribe() to subscribe a message belong to such topic. If the topic used to retain a message, the microgear will receive a message everytime it subscribes that topic.

argument

* *topic* `string` -  name of the topic to send a message to. Should start with "/". 



<br/>

**ResetToken()** For deleting Token in cache and on the platform. If deleted, need to get a new Token for the next connection.

<br/>



###Event
---------------
An application that runs on a microgear is an event-driven type, which responses to various events with the callback function in a form of event function call:


**onConnect**  This event is created when the microgear library successfully connects to the NETPIE platform.

Parameter set

* *callback* `function` - A function to execute after getting connected


<br/>




**onDisconnect** This event is created when the microgear library disconnects the NETPIE platform.

Parameter set


* *callback* `function` - A function to execute after getting disconnected


<br/>




**onMessage** When there is an incomming message from chat or from subscribed topic. This event is created with the related information to be sent via the callback function.

Parameter set
* *callback* `function` - A function to execute after getting a message. It takes 2 arguments.
    * *topic* - The subscribed topic that he message belongs to. 
    * *message* - The received message.


<br/>


**onPresent** This event is created when there is a microgear under the same appid appears online to connect to NETPIE.

Parameter set


* *callback* `function` - A function to executed after this event. It takes 1 argument.
     * *gearkey* - The gearkey related to this event.


<br/>




**onAbsent** This event is created when the microgear under the same appid appears offline.

Parameter set


* *callback* `function` - A function to executed after this event. It takes 1 argument.
    * *gearkey* - The gearkey related to this event.


<br/>



**onError** This event is created when an error occurs within a microgear.

Parameter set


* *callback* `function` - A function to executed after this event. It takes 1 argument.
    * *msg* - An error message related to this event.


<br/>