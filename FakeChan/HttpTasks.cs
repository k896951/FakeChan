using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FakeChan
{
    public class HttpTasks
    {
        Configs Config;
        MessQueueWrapper MessQue;
        WCFClient WcfClient;
        UserDefData UserData;
        EditParamsBefore EditInputText = new EditParamsBefore();
        Random r = new Random(Environment.TickCount);

        bool KeepListen = false;
        HttpListener HTTPListener;
        Action BGHttpListen;
        int taskId = 0;
        int ListenPort;

        public delegate void CallEventHandlerCallAsyncTalk(MessageData talk);
        public event CallEventHandlerCallAsyncTalk OnCallAsyncTalk;

        public Methods PlayMethod { get; set; }

        public HttpTasks(ref Configs cfg, ref MessQueueWrapper mq, ref WCFClient wcf, ref UserDefData UsrData)
        {
            Config = cfg;
            MessQue = mq;
            WcfClient = wcf;
            UserData = UsrData;
        }

        public void StartHttpTasks(IPAddress addr, int port)
        {
            PlayMethod = Methods.sync;

            // HTTP リスナタスク起動
            HTTPListener = new HttpListener();
            HTTPListener.Prefixes.Add(string.Format(@"http://{0}:{1}/", addr, port));
            HTTPListener.Start();
            KeepListen = true;
            BGHttpListen = SetupBGHttpListenerTask();
            Task.Run(BGHttpListen);
            ListenPort = port;
        }

        public void StopHttpTasks()
        {
            // HTTP リスナタスク停止
            KeepListen = false;
            try
            {
                HTTPListener?.Stop();
            }
            catch(Exception)
            {
                //
            }
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

                List<int> CidList = Config.AvatorNames.Select(c => c.Key).ToList();
                int cnt = CidList.Count;
                int cid;
                int ListenIf;

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

                        foreach (var item in Regex.Split(request.Url.Query, @"[&?]"))
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
                                    break;

                                case "volume":
                                case "speed":
                                case "tone":
                                default:
                                    break;
                            }
                        }

                        response.ContentType = "application/json; charset=utf-8";

                        if (ListenPort == Config.HttpPortNum2)
                        {
                            ListenIf = (int)ListenInterface.Http2;
                        }
                        else
                        {
                            ListenIf = (int)ListenInterface.Http1;
                        }

                        voice = EditInputText.EditInputString((voice > 8 || voice == -1 ? 0 : voice), TalkText);

                        if (UserData.IsRandomVoice)
                        {
                            voice = r.Next(0, 9);
                            cid = UserData.SelectedCid[ListenIf][voice];
                        }
                        else if (UserData.IsRandomAvator)
                        {
                            cid = CidList[r.Next(0, cnt)];
                            if (Config.AvatorNames.ContainsKey(cid))
                            {
                                UserData.VoiceParams[ListenIf][voice][cid] = Config.AvatorParams(cid);
                            }
                        }
                        else
                        {
                            cid = UserData.SelectedCid[ListenIf][voice];
                        }

                        // dispath url
                        switch (UrlPath)
                        {
                            case "/TALK":
                                Dictionary<string, decimal> Effects = UserData.VoiceParams[ListenIf][voice][cid]["effect"].ToDictionary(k => k.Key, v => v.Value["value"]);
                                Dictionary<string, decimal> Emotions = UserData.VoiceParams[ListenIf][voice][cid]["emotion"].ToDictionary(k => k.Key, v => v.Value["value"]);

                                MessageData talk = new MessageData()
                                {
                                    Cid = cid,
                                    Message = EditInputText.ChangedTalkText,
                                    BouyomiVoice = voice,
                                    ListenInterface = ListenIf,
                                    TaskId = MessQue.count + 1,
                                    Effects = Effects,
                                    Emotions = Emotions
                                };

                                switch (PlayMethod)
                                {
                                    case Methods.sync:
                                        MessQue.AddQueue(talk);
                                        break;

                                    case Methods.async:
                                        OnCallAsyncTalk?.Invoke(talk);
                                        break;
                                }

                                byte[] responseTalkContent = Encoding.UTF8.GetBytes("{" + string.Format(@"""taskId"":{0}", talk.TaskId) + "}");
                                response.OutputStream.Write(responseTalkContent, 0, responseTalkContent.Length);
                                response.Close();
                                break;

                            case "/GETVOICELIST":
                                sb.Clear();
                                sb.AppendLine(@"{ ""voiceList"":[");
                                sb.Append(
                                    string.Join(",", Config.AvatorNames.Select(v => string.Format(listFmt, v.Key, v.Value))
                                                                       .Select(v => "{" + v + "}")
                                                                       .ToArray())
                                );
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
