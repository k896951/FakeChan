using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeChan
{
    class MessageData
    {
        public int Cid { get; set; }

        public int BouyomiVoice { get; set; }

        public int TaskId { get; set; }

        public string Message { get; set; }

        public Dictionary<string, decimal> Effects { get; set; }

        public Dictionary<string, decimal> Emotions { get; set; }
    }
}
