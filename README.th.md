#Microgear-csharp
-----------
microgear-csharp คือ client library ภาษา C#  ที่ทำหน้าที่เป็นตัวกลางในการเชื่อมโยง application code หรือ hardware เข้ากับบริการของ netpie platform เพื่อการพัฒนา IOT application รายละเอียดเกี่ยวกับ netpie platform สามารถศึกษาได้จาก http://netpie.io



##การติดตั้ง
-----------
```sh
Install-Package Microgear
```


##ตัวอย่างการเรียกใช้งาน
-----------
```C#
using System.Threading;
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
            microgear.onInfo += Info;
            microgear.Connect(AppID, Key, Secret);
            microgear.SetAlias("test");
            microgear.Subscribe("/topic");
            for (int i = 0; i < 10; i++)
            {
                microgear.Chat("test", "test message no."+i.ToString());
                Thread.Sleep(2000);
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

        public static void Info(string info)
        {
            Console.WriteLine(info);
        }
    }
}

```


##การใช้งาน library
------------
###Microgear
---------------
**Connect(AppID, Key, Secret)**

arguments

 * *AppID* `string` - กลุ่มของ application ที่ microgear จะทำการเชื่อมต่อ
 * *Key* `string` - เป็น key สำหรับ gear ที่จะรัน ใช้ในการอ้างอิงตัวตนของ gear
 * *Secret* `string` - เป็น secret ของ key ซึ่งจะใช้ประกอบในกระบวนการยืนยันตัวตน

<br/>


**SetAlias(*alias*)** กำหนดชื่อเรียกสำหรับ microgear นี้ โดยจะปรากฎที่หน้า key management และสามารถเป็นชื่อที่ microgear ตัวอื่นใช้สำหรับ `chat()` ได้

argument

* *alias* `string` - ชื่อของ microgear นี้

<br/>



**Chat(*gearname*, *message*)** การส่งข้อความโดยระบุ gearname และข้อความที่ต้องการส่ง

arguments

* *gearname* `string` - ชื่อของ microgear นี้
* *message* `string` – ข้อความ

<br/>



**Publish(*topic*, *message*, *retain*):** ในกรณีที่ต้องการส่งข้อความแบบไม่เจาะจงผู้รับ สามารถใช้ฟังชั่น publish ไปยัง topic ที่กำหนดได้ ซึ่งจะมีแต่ microgear ที่ subscribe topoic นี้เท่านั้น ที่จะได้รับข้อความ

arguments

* *topic* `string` - ชื่อของ topic ที่ต้องการจะส่งข้อความไปถึง
* *message* `string` – ข้อความ
* *retain* `boolean` - ระบุค่า `True` ถ้าต้องการเก็บข้อความไว้ หากมีการ subscribe topic นี้ก็จะได้รับข้อความนี้อีก ค่าปริยายเป็น `False` หากไม่ระบุ และถ้าต้องการลบข้อความที่บันทึกไว้ให้ส่งข้อความ "" ซึ่งมีความยาวเป็น 0 เพื่อล้างค่าข้อความที่ไว้ทึกไว้

<br/>



**Subscribe(*topic*)** microgear อาจจะมีความสนใจใน topic
ใดเป็นการเฉพาะ เราสามารถใช้ฟังก์ชั่น subscribe() ในการบอกรับ message ของ topic นั้นได้

argument

* *topic* `string` - ชื่อของ topic ที่ความสนใจ โดยขึ้นต้นด้วยเครื่องหมาย "/" 

<br/>

**ResetToken()** สำหรับต้องการลบ Token ที่มีอยู่ ซึ่งจะทำการลบ Token ที่อยู่ภายใน cache และบน platform เมื่อลบแล้ว จำเป็นจะต้องขอ Token ใหม่ทุกครั้ง

<br/>

**wrtieFeed(Feedid, Data, Apikey):** เขียนข้อมูลลง feed storage

arguments

* *Feedid* `string` - ชื่อของ feed ที่ต้องการจะเขียนข้อมูล
* *Data* `jsonobject` – ข้อมูลที่จะบันทึก ในรูปแบบ JSONObject
* *Apikey* `string` - apikey สำหรับตรวจสอบสิทธิ์ หากไม่กำหนด จะใช้ default apikey ของ feed ที่ให้สิทธิ์ไว้กับ AppID

<br/>




###Event
---------------
application ที่รันบน microgear จะมีการทำงานในแบบ event driven คือเป็นการทำงานตอบสนองต่อ event ต่างๆ ด้วยการเขียน callback function ขึ้นมารองรับในลักษณะๆดังต่อไปนี้

**onConnect**  เกิดขึ้นเมื่อ microgear library เชื่อมต่อกับ platform สำเร็จ

ค่าที่ set

* *callback* `function` - ฟังก์ชั่นที่จะทำงาน เมื่อมีการ connect

<br/>



**onDisconnect** เกิดขึ้นเมื่อ microgear library ตัดการเชื่อมต่อกับ platform

ค่าที่ set


* *callback* `function` - callback function

<br/>




**onMessage** เกิดขึ้นเมื่อ ได้รับข้อความจากการ chat หรือ หัวข้อที่ subscribe

ค่าที่ set
* *callback* `function` - ฟังก์ชั่น ที่จะทำงานเมื่อได้รับข้อความ โดยฟังก์ชั่นนี้จะรับ parameter 2 ตัวคือ
    * *topic* - ชื่อ topic ที่ได้รับข้อความนี้
    * *message* - ข้อความที่ได้รับ

<br/>


**onPresent** event นี้จะเกิดขึ้นเมื่อมี microgear ใน appid เดียวกัน online เข้ามาเชื่อมต่อ netpie

ค่าที่ set


* *callback* `function` - จะทำงานเมื่อเกิดเหตุการณ์นี้ โดยจะรับค่า parameter คือ
     * *gearkey* - ระบุค่าของ gearkey ที่เกี่ยวข้องกับเหตุการณ์นี้


<br/>


**onAbsent** event นี้จะเกิดขึ้นเมื่อมี microgear ใน appid เดียวกัน offline หายไป

ค่าที่ set


* *callback* `function` - จะทำงานเมื่อเกิดเหตุการณ์นี้ โดยจะรับค่า parameter คือ
    * *gearkey* - ระบุค่าของ gearkey ที่เกี่ยวข้องกับเหตุการณ์นี้


<br/>


**onError** event นี้จะเกิดขึ้นเมื่อมี error

ค่าที่ set


* *callback* `function` - จะทำงานเมื่อเกิดเหตุการณ์นี้ โดยจะรับค่า parameter คือ
    * *msg* - ระบุ error ที่เกี่ยวข้องกับเหตุการณ์นี้


<br/>

**onInfo** event นี้จะเกิดขึ้นเมื่อมี Info

ค่าที่ set


* *callback* `function` - จะทำงานเมื่อเกิดเหตุการณ์นี้ โดยจะรับค่า parameter คือ
    * *msg* - ระบุ info ที่เกี่ยวข้องกับเหตุการณ์นี้


<br/>
