using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeChan
{
    public class MessQueueWrapper
    {
        BlockingCollection<MessageData> MessQue = null;
        object lockObj = new object();

        public MessQueueWrapper()
        {
            MessQue = new BlockingCollection<MessageData>();
            ClearQueue();
        }

        public void ClearQueue()
        {
            BlockingCollection<MessageData>[] t = { MessQue };
            BlockingCollection<MessageData>.TryTakeFromAny(t, out MessageData item);
        }

        public void AddQueue(MessageData item)
        {
            lock (lockObj)
            {
                MessQue.TryAdd(item, 1000);
            }
        }

        public ref BlockingCollection<MessageData> QueueRef()
        {
            return ref MessQue;
        }

        public int count
        {
            get
            {
                return MessQue.Count;
            }
        }
    }
}
