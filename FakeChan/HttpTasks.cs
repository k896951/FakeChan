using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace FakeChan
{
    public class HttpTasks
    {
        Configs Config;
        MessQueueWrapper MessQue;
        WCFClient WcfClient;

        Dictionary<int, Dictionary<int, Dictionary<string, Dictionary<string, Dictionary<string, decimal>>>>> ParamAssignList;

        bool KeepListen = false;
        HttpListener HTTPListener;
        Action BGTcpListen;
        int taskId = 0;

        public methods PlayMethod { get; set; }

        public HttpTasks(ref Configs cfg, ref MessQueueWrapper mq, ref WCFClient wcf, ref Dictionary<int, Dictionary<int, Dictionary<string, Dictionary<string, Dictionary<string, decimal>>>>> Params)
        {
            Config = cfg;
            MessQue = mq;
            WcfClient = wcf;
            ParamAssignList = Params;
        }

        public void StartHttpTasks(IPAddress addr, int port)
        {
            PlayMethod = methods.sync;

            // TCP/IP リスナタスク起動
            HTTPListener = new HttpListener();
            HTTPListener.Prefixes.Add(string.Format(@"http://{0}:{1}/", addr, port));
            HTTPListener.Start();
            KeepListen = true;
            BGTcpListen = SetupBGHttpListenerTask();
            Task.Run(BGTcpListen);
        }

        public void StopHttpTasks()
        {
            // TCP/IP リスナタスク停止
            KeepListen = false;
            HTTPListener?.Stop();
        }

        public void SetTaskId(int tid)
        {
            taskId = tid;
        }

        private Action SetupBGHttpListenerTask()
        {
            Action BGHttpListen = (() => {

                StringBuilder sb = new StringBuilder();
                string listFmt = @"""id"":{0}, ""kind"":""AquesTalk"", ""name"":""{1}"", ""alias"":""""";

                while (KeepListen) // とりあえずの待ち受け構造
                {
                    try
                    {
                        HttpListenerContext  context  = HTTPListener.GetContext();
                        HttpListenerRequest  request  = context.Request;
                        HttpListenerResponse response = context.Response;
                        int voice = 0;
                        string TalkText = "本日は晴天ですか？";
                        string UrlPath = request.Url.AbsolutePath.ToUpper();

                        foreach(var item in Regex.Split(request.Url.Query, @"[&?]"))
                        {
                            if (item == "") continue;

                            string[] s = Regex.Split(item, "=") ;
                            if (s.Length <  2) s = new string[] { HttpUtility.UrlDecode(s[0]), "" };
                            if (s.Length >= 2) s = new string[] { HttpUtility.UrlDecode(s[0]), HttpUtility.UrlDecode(s[1]) };

                            switch (s[0])
                            {
                                case "text":
                                    TalkText = s[1];
                                    break;

                                case "voice":
                                    int.TryParse(s[1], out voice);
                                    voice = (short)(voice > 8 ? 18 : voice + 18);
                                    break;

                                case "volume":
                                case "speed":
                                case "tone":
                                default:
                                    break;
                            }
                        }

                        response.ContentType = "application/json; charset=utf-8";

                        // dispath url
                        switch(UrlPath)
                        {
                            case "/TALK":
                                int cid = Config.B2Amap.First().Value;
                                int tid = MessQue.count + 1;
                                cid = Config.B2Amap[voice];
                                Dictionary<string, decimal> Effects = ParamAssignList[voice][cid]["effect"].ToDictionary(k => k.Key, v => v.Value["value"]);
                                Dictionary<string, decimal> Emotions = ParamAssignList[voice][cid]["emotion"].ToDictionary(k => k.Key, v => v.Value["value"]);

                                MessageData talk = new MessageData()
                                {
                                    Cid = cid,
                                    Message = TalkText,
                                    BouyomiVoice = voice,
                                    TaskId = tid,
                                    Effects = Effects,
                                    Emotions = Emotions
                                };

                                switch (PlayMethod)
                                {
                                    case methods.sync:
                                        MessQue.AddQueue(talk);
                                        break;

                                    case methods.async:
                                        WcfClient.TalkAsync(cid, TalkText, Effects, Emotions);
                                        break;
                                }

                                byte[] responseTalkContent = Encoding.UTF8.GetBytes("{" + string.Format(@"""taskId"":{0}", tid) + "}");
                                response.OutputStream.Write(responseTalkContent, 0, responseTalkContent.Length);
                                response.Close();
                                break;

                            case "/GETVOICELIST":
                                sb.Clear();
                                sb.AppendLine(@"{ ""voiceList"":[");
                                sb.AppendLine("{" + string.Join(",", Config.BouyomiVoicesHttp.Select(v => string.Format(listFmt, v.Key, v.Value)).ToArray()) + "}");
                                sb.AppendLine(@"] }");

                                byte[] responseListContent = Encoding.UTF8.GetBytes(sb.ToString());
                                response.OutputStream.Write(responseListContent, 0, responseListContent.Length);
                                response.Close();
                                break;

                            case "/GETTALKTASKCOUNT":
                                byte[] responseTaskCountContent = Encoding.UTF8.GetBytes( "{" + string.Format(@"""talkTaskCount"":{0}", MessQue.count) +"}");
                                response.OutputStream.Write(responseTaskCountContent, 0, responseTaskCountContent.Length);
                                response.Close();
                                break;

                            case "/GETNOWTASKID":
                                byte[] responseTaskNowContent = Encoding.UTF8.GetBytes("{" + string.Format(@"""nowTaskId"":{0}", taskId) + "}");
                                response.OutputStream.Write(responseTaskNowContent, 0, responseTaskNowContent.Length);
                                response.Close();
                                break;

                            case "/GETNOWPLAYING":
                                byte[] responseTaskPlayingContent = Encoding.UTF8.GetBytes("{" + string.Format(@"""nowPlaying"":{0}", MessQue.count != 0) + "}");
                                response.OutputStream.Write(responseTaskPlayingContent, 0, responseTaskPlayingContent.Length);
                                response.Close();
                                break;

                            case "/CLEAR":
                                byte[] responseTaskClearContent = Encoding.UTF8.GetBytes(@"{}");
                                response.OutputStream.Write(responseTaskClearContent, 0, responseTaskClearContent.Length);
                                response.Close();
                                break;

                            default:
                                byte[] responseMessageContent = Encoding.UTF8.GetBytes(@"{ ""Message"":""content not found.""}");
                                response.StatusCode = 404;
                                response.OutputStream.Write(responseMessageContent, 0, responseMessageContent.Length);
                                response.Close();
                                break;
                        }

                    }
                    catch (Exception)
                    {
                        //Dispatcher.Invoke(() =>
                        //{
                        //    MessageBox.Show(e.Message, "sorry3");
                        //});
                    }
                }
            });

            return BGHttpListen;
        }

    }
}
