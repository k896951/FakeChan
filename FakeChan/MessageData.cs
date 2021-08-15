using System.Collections.Generic;

namespace FakeChan
{
    public class MessageData
    {
        public int Cid { get; set; }

        public int BouyomiVoice { get; set; }

        public int ListenInterface { get; set; }

        public int TaskId { get; set; }

        public string Message { get; set; }

        public string OrgMessage {
            get { return orgmessage; }
            set { orgmessage = Message = value;  }
        }
        private string orgmessage;

        public Dictionary<string, decimal> Effects { get; set; }

        public Dictionary<string, decimal> Emotions { get; set; }
    }
}
