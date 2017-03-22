using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using RestSharp;
using System.Net;

namespace io.netpie.microgear
{
    public static class init
    {
        public static string gearauthsite = "https://ga.netpie.io:8081";
        public static string requesttokenendpoint = "api/rtoken";
        public static string accesstokenendpoint = "api/atoken";
        public static string gearkey = "";
        public static string gearsecret = "";
        public static string appid = "";
        public static string gearalias = null;
        public static string accesstoken = null;
        public static string requesttoken = null;
        public static string client = null;
        public static string scope = "";
        public static string gearexaddress = null;
        public static string gearexport = null;
        public static string mqtt_client = null;
        public static string accesssecret = null;
        public static string mgrev = "CSP";

    }

    public class requesttoken
    {
        public string token { get; set; }
        public string secret { get; set; }
        public string verifier { get; set; }
    }

    public class accesstoken
    {
        public string token { get; set; }
        public string secret { get; set; }
        public string endpoint { get; set; }
        public string revokecode { get; set; }
    }

    public class tokencache
    {
        public requesttoken requesttoken { get; set; }
        public accesstoken accesstoken { get; set; }
        public string key { get; set; }
    }

    public class token
    {
        public tokencache _ { get; set; }
    }

    public class Microgear
    {
        private System.Timers.Timer aTimer;
        private tokencache tokencache;
        private token token;
        private accesstoken accesstoken;
        private requesttoken requesttoken;
        private cache cache;
        private MqttClient mqtt_client;
        private List<string> subscribe_list = new List<string>();
        private List<List<string>> pubilsh_list = new List<List<string>>();
        public Action onDisconnect;
        public Action<string> onPresent;
        public Action<string> onAbsent;
        public Action onConnect;
        public Action<string> onError;
        public Action<string> onInfo;
        public Action<string, string> onMessage;
        private bool reset = false;
        private String current_id;
        private int status = 0;

        private void do_nothing() { }
        private void do_nothing(string i) { }
        private void do_nothing(string i, string j) { }

        public Microgear()
        {
            this.onDisconnect = do_nothing;
            this.onPresent = do_nothing;
            this.onAbsent = do_nothing;
            this.onInfo = do_nothing;
            this.onConnect = do_nothing;
            this.onMessage = do_nothing;
            this.onError = do_nothing;
            this.cache = new cache();
            this.tokencache = new tokencache();
            this.accesstoken = new accesstoken();
            this.requesttoken = new requesttoken();
            this.token = new token();
        }
        public void Connect(string appid, string gearkey, string gearsecret)
        {
            init.gearkey = gearkey;
            init.gearsecret = gearsecret;
            init.appid = appid;
            if (reset)
            {
                this.ResetToken();

            }
            this.Create();
        }
        public void Connect(string appid, string gearkey, string gearsecret, string alias)
        {
            this.Connect(appid, gearkey, gearsecret);
            this.SetAlias(alias);
        }

        public void Create()
        {
            this.aTimer = new System.Timers.Timer();
            while (init.accesstoken == null)
            {
                get_token();
                aTimer.Interval = 2000;
            }
            this.mqtt_client = new MqttClient(init.gearexaddress);
            current_id = "/&id/" + this.accesstoken.token + "/#";
            this.Subscribe(current_id);
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            string username = init.gearkey + "%" + unixTimestamp;
            string pass = CreateToken(init.accesssecret + "&" + init.gearsecret, init.accesstoken + "%" + username);
            var status_code = this.mqtt_client.Connect(init.accesstoken, username, pass);
            this.mqtt_client.MqttMsgPublishReceived += HandleClientMqttMsgPublishReceived;
            this.mqtt_client.MqttMsgPublished += MqttMsgPublished;
            this.mqtt_client.ConnectionClosed += ConnectionClosedEventHandler;
            this.mqtt_client.MqttMsgSubscribed += MqttMsgSubscribed;

            if (status_code == 0)
            {
                this.status = 1;
                this.onConnect();
                AutoSubscribeAndPublish();
            }
            else if (status_code == 1)
            {
                Console.WriteLine("Unable to connect: Incorrect protocol version.");
            }
            else if (status_code == 2)
            {
                Console.WriteLine("Unable to connect: Invalid client identifier.");
            }
            else if (status_code == 3)
            {
                Console.WriteLine("Unable to connect: Server unavailable.");
            }
            else if (status_code == 4)
            {
                this.Unsubscribe(current_id);
                Console.WriteLine("Unable to connect: Invalid credential, requesting new one");
                this.Disconnect();
                reset = true;
                this.ResetToken();
                this.Create();
            }
            else if (status_code == 5)
            {
                Console.WriteLine("Unable to connect: Not authorised.");
                this.Create();
                aTimer.Interval = 2000;
            }
            else
            {
                Console.WriteLine("Unable to connect: Unknown reason");
            }
        }

        private void AutoSubscribeAndPublish()
        {
            this.Subscribe("/" + init.appid + "/&present");
            this.Subscribe("/" + init.appid + "/&absent");
            foreach (string topic in this.subscribe_list)
            {
                this.Subscribe(topic);
            }
            foreach (List<string> pubilsh in this.pubilsh_list)
            {
                if (pubilsh.Count > 2)
                {
                    this.Publish(pubilsh[0], pubilsh[1], Convert.ToBoolean(pubilsh[2]));
                }
                else
                {
                    this.Publish(pubilsh[0], pubilsh[1]);
                }

            }
            this.subscribe_list = new List<string>();
            this.pubilsh_list = new List<List<string>>();
        }


        public void ConnectionClosedEventHandler(object sender, EventArgs e)
        {
            if (status == 2)
            {
                this.onDisconnect();
            }
            else
            {
                this.Create();
            }

        }


        private void MqttMsgPublished(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishedEventArgs e)
        {

        }

        private void HandleClientMqttMsgPublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e)
        {
            var topic = e.Topic.Split('/');
            if (e.Topic.IndexOf("&present") >= 0)
            {
                this.onPresent(Encoding.UTF8.GetString(e.Message));
            }
            else if (e.Topic.IndexOf("&absent") >= 0)
            {
                this.onAbsent(Encoding.UTF8.GetString(e.Message));
            }
            else if (e.Topic.IndexOf("&id") >= 0)
            {
                //pass
            }
            else if (e.Topic.IndexOf("@info") >= 0)
            {
                this.onInfo(Encoding.UTF8.GetString(e.Message));
            }
            else if (e.Topic.IndexOf("&error") >= 0)
            {
                this.onError(Encoding.UTF8.GetString(e.Message));
            }
            else
            {
                this.onMessage(e.Topic, Encoding.UTF8.GetString(e.Message));
            }

        }

        private void MqttMsgSubscribed(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgSubscribedEventArgs e)
        {

        }

        public void Disconnect()
        {
            if (this.mqtt_client.IsConnected)
            {
                this.status = 2;
                this.mqtt_client.Disconnect();
            }
        }

        public void Unsubscribe(string topic)
        {

            if (topic.Substring(0, 1) != "/")
            {
                topic = "/" + topic;
            }
            if (this.status != 3)
            {
                if (this.mqtt_client.IsConnected)
                {
                    this.mqtt_client.Unsubscribe(new string[] { topic });
                }
            }

        }

        public void Subscribe(string topic)
        {

            if (topic.Substring(0, 1) != "/")
            {
                topic = "/" + topic;
            }
            if (this.status != 3)
            {
                if (this.mqtt_client.IsConnected)
                {
                    this.mqtt_client.Subscribe(new string[] { "/" + init.appid + topic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });

                }
                else
                {
                    topic = "/" + init.appid + topic;
                    this.subscribe_list.Add(topic);
                }
            }


        }

        public void Publish(string topic, string message, bool retained)
        {
            if (topic.Substring(0, 1) != "/")
            {
                topic = "/" + topic;
            }
            if (this.status != 3)
            {
                if (this.mqtt_client.IsConnected)
                {
                    this.mqtt_client.Publish("/" + init.appid + topic, Encoding.UTF8.GetBytes(message), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, retained);
                }
                else
                {
                    List<string> publish = new List<string>();
                    publish.Add("/" + init.appid + topic);
                    publish.Add(message);
                    publish.Add(retained.ToString());
                    this.pubilsh_list.Add(publish);

                }
            }

        }
        public void Publish(string topic, string message)
        {
            if (topic.Substring(0, 1) != "/")
            {
                topic = "/" + topic;
            }
            if (this.status != 3)
            {
                if (this.mqtt_client.IsConnected)
                {
                    this.mqtt_client.Publish("/" + init.appid + topic, Encoding.UTF8.GetBytes(message), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
                }
                else
                {
                    List<string> publish = new List<string>();
                    publish.Add("/" + init.appid + topic);
                    publish.Add(message);
                    this.pubilsh_list.Add(publish);
                }
            }

        }

        public void ResetToken()
        {
            if (reset)
            {
                reset = false;
                var cached = this.cache.get_item("microgear-" + init.gearkey + ".cache");
                if (cached == null)
                {
                    this.cache.set_item(null, "microgear-" + init.gearkey + ".cache");
                    cached = this.cache.get_item("microgear-" + init.gearkey + ".cache");
                }
                if (cached.accesstoken != null)
                {
                    var revokecode = cached.accesstoken.revokecode;
                    if (revokecode != null)
                    {
                        var path = "api/revoke/" + cached.accesstoken.token + "/" + revokecode;
                        var client = new RestClient(init.gearauthsite);
                        var request = new RestRequest(path, Method.GET);
                        var response = client.Execute(request);
                        HttpStatusCode statusCode = response.StatusCode;
                        int numericStatusCode = (int)statusCode;
                        if (numericStatusCode == 200)
                        {
                            File.Delete("microgear-" + init.gearkey + ".cache");
                            init.accesstoken = null;
                        }
                    }
                }
            }
            else
            {
                reset = true;
            }
        }


        public void SetAlias(string alias)
        {
            init.gearalias = alias;
            this.Publish("/@setalias/" + alias, "");
        }

        public void Chat(string topic, string message)
        {
            this.Publish("/gearname/" + topic, message);
        }

        private void get_token()
        {
            var cached = this.cache.get_item("microgear-" + init.gearkey + ".cache");
            if (cached == null)
            {
                this.cache.set_item(null, "microgear-" + init.gearkey + ".cache");
                cached = this.cache.get_item("microgear-" + init.gearkey + ".cache");
            }
            if (cached.accesstoken != null)
            {
                string[] endpoint = cached.accesstoken.endpoint.Split(':');
                init.accesstoken = cached.accesstoken.token;
                init.accesssecret = cached.accesstoken.secret;
                init.gearexaddress = endpoint[1].Split('/')[2];
                init.gearexport = endpoint[2];
            }
            else
            {
                this.tokencache = cached;
                forToken();
            }
        }

        private void forToken()
        {
            var verifier = "";
            var path = "";
            if (init.gearalias != null)
            {
                verifier = init.gearalias;
                path = "response_type=code&client_id=" + init.gearkey + "&scope=appid:" + init.appid + "%20alias:" + init.gearalias + "&state=mgrev:" + init.mgrev;

            }
            else
            {
                verifier = init.mgrev;
                path = "response_type=code&client_id=" + init.gearkey + "&scope=appid:" + init.appid + "&state=mgrev:" + init.mgrev;

            }
            var client = new RestClient(init.gearauthsite + "/oauth2/authorize?" + path);
            var request = new RestRequest(Method.GET);
            var response = client.Execute(request);
            string responsecode = response.ResponseUri.ToString();
            string[] responsecodelist = responsecode.Replace("code=", "|").Split('|');
            if (responsecodelist.Length == 2)
            {
                var code = responsecodelist[1];
                string oauthToken = code;
                string oauthTokenSecret = "null";
                this.requesttoken.token = oauthToken;
                this.requesttoken.secret = oauthTokenSecret;
                this.requesttoken.verifier = verifier;
                this.tokencache.requesttoken = this.requesttoken;
                path = "grant_type=authorization_code&code=" + code + "&client_id=" + init.gearkey + "&client_secret=" + init.gearsecret + "&state=mgrev:" + init.mgrev;
                client = new RestClient(init.gearauthsite + "/oauth2/token?" + path);
                request = new RestRequest(Method.POST);
                response = client.Execute(request);
                string responsetoken = response.Content;
                string[] responsetokenlist = responsetoken.Replace("{\"access_token\":\"", "|").Split('|');
                if (responsetokenlist.Length == 2)
                {
                    responsetokenlist = responsetokenlist[1].Replace("\",\"token_type\"", "|").Split('|');
                    if (responsetokenlist.Length == 2)
                    {
                        responsetokenlist = responsetokenlist[0].Replace(":", "|").Split('|');
                        oauthToken = responsetokenlist[0];
                        oauthTokenSecret = responsetokenlist[1];
                        responsetokenlist = responsetoken.Replace("\"endpoint\":\"", "|").Split('|');
                        responsetokenlist = responsetokenlist[1].Replace("\"}", "|").Split('|');
                        string endpoint = responsetokenlist[0];
                        string revokecode = CreateToken(oauthTokenSecret + "&" + init.gearsecret, oauthToken);
                        revokecode = revokecode.Replace('/', '_');
                        this.accesstoken.token = oauthToken;
                        this.accesstoken.secret = oauthTokenSecret;
                        this.accesstoken.endpoint = endpoint;
                        this.accesstoken.revokecode = revokecode;
                        this.tokencache.accesstoken = this.accesstoken;
                        this.tokencache.key = init.gearkey;
                        this.token._ = this.tokencache;
                        this.cache.set_item(this.token, "microgear-" + init.gearkey + ".cache");
                    }
                }
                else
                {
                    this.onError("Access token is not issued, please check your consumerkey and consumersecret.");
                    reset = true;
                    this.ResetToken();
                }
            }
            else
            {
                this.onError("Request token is not issued, please check your appkey and appsecret.");
            }
        }

        private string CreateToken(string secret, string message)
        {
            secret = secret ?? "";
            var encoding = new System.Text.ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(secret);
            byte[] messageBytes = encoding.GetBytes(message);
            using (var hmacsha1 = new HMACSHA1(keyByte))
            {
                byte[] hashmessage = hmacsha1.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashmessage).Trim();
            }
        }

        public void writeFeed(String feedid, String data)
        {
            String feedTopic = "/@writefeed/" + feedid;
            data = data.Replace("{", "");
            data = data.Replace("}", "");
            string[] datas = data.Split(',');
            String datajson = "{";
            foreach (string datai in datas)
            {
                string[] datais = datai.Split(':');
                if (datais[0].IndexOf("\"") < 0)
                {
                    datajson += "\"" + datais[0] + "\":" + datais[1] + ",";
                }
                else
                {
                    datajson += datais[0] + ":" + datais[1] + ",";
                }

            }
            datajson = datajson.Remove(datajson.Length - 1);
            datajson += "}";
            Publish(feedTopic, datajson);
        }

        public void writeFeed(String feedid, String data, String feedkey)
        {
            if (feedkey.Length != 0 && feedkey != null)
            {
                this.writeFeed(feedid + "/" + feedkey, data);
            }
        }

    }

    public class cache
    {
        public tokencache get_item(string key)
        {
            string path = Directory.GetCurrentDirectory();
            string pathkey = System.IO.Path.Combine(path, key);

            if (!System.IO.File.Exists(pathkey))
            {
                return null;
            }
            else
            {
                string text = System.IO.File.ReadAllText(pathkey);
                if (text.Length > 0)
                {
                    return GetDict(text)._;
                }
                tokencache tokencache = new tokencache();
                tokencache.accesstoken = null;
                tokencache.requesttoken = null;
                return tokencache;
            }
        }
        public void set_item(token token, string key)
        {
            string path = Directory.GetCurrentDirectory();
            string pathkey = System.IO.Path.Combine(path, key);
            if (!System.IO.File.Exists(pathkey))
            {
                System.IO.FileStream cache_file = System.IO.File.Create(pathkey);
                cache_file.Dispose();
            }
            if (token != null)
            {
                string ans = JsonConvert.SerializeObject(token, Formatting.Indented);
                System.IO.File.WriteAllText(@pathkey, ans);
            }
        }
        private token GetDict(string text)
        {
            Dictionary<string, string> d = new Dictionary<string, string>();
            text = text.Replace(" ", "");
            string[] words = text.Split('\n');
            token token = new token();
            tokencache tokencache = new tokencache();
            accesstoken accesstoken = new accesstoken();
            requesttoken requesttoken = new requesttoken();
            requesttoken.token = words[3].Split(':', ',')[1].Substring(1, words[3].Split(':', ',')[1].Length - 2);
            requesttoken.secret = words[4].Split(':', ',')[1].Substring(1, words[4].Split(':', ',')[1].Length - 2);
            requesttoken.verifier = words[5].Split(':')[1].Substring(1, words[5].Split(':', ',')[1].Length - 3);
            try
            {
                accesstoken.token = words[8].Split(':', ',')[1].Substring(1, words[8].Split(':', ',')[1].Length - 2);
                accesstoken.secret = words[9].Split(':', ',')[1].Substring(1, words[9].Split(':', ',')[1].Length - 2);
                accesstoken.endpoint = words[10].Split(':')[1].Substring(1, words[10].Split(':', ',')[1].Length - 1) + ":" + words[10].Split(':')[2] + ":" + words[10].Split(':')[3].Substring(0, words[10].Split(':', ',')[3].Length - 2);
                accesstoken.revokecode = words[11].Split(':', ',')[1].Substring(1, words[11].Split(':', ',')[1].Length - 3);
                tokencache.accesstoken = accesstoken;
            }
            catch
            {
                tokencache.accesstoken = null;
            }
            tokencache.requesttoken = requesttoken;
            token._ = tokencache;
            return token;
        }
    }
}





