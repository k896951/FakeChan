using System.Collections.Generic;

namespace FakeChan
{
    public class MessageData
    {
        public int Cid { get; set; }

        public int BouyomiVoice { get; set; }

        public int BouyomiVoiceIdx { get; set; }

        public int TaskId { get; set; }

        public string Message { get; set; }

        public Dictionary<string, decimal> Effects { get; set; }

        public Dictionary<string, decimal> Emotions { get; set; }
    }
}
