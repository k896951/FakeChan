using System.Collections.Generic;

namespace FakeChan
{
    public enum Methods
    {
        sync  = 0,  // 同期
        async = 1   // 非同期
    }

    public enum ListenInterface
    {
        IPC1    = 0,
        Socket1 = 1,
        Http1   = 2,
        Socket2 = 3,
        Http2   = 4,
        Clipboard = 5
    }

    public enum BouyomiVoice
    {
        voice0   = 0,
        female1  = 1,
        female2  = 2,
        male1    = 3,
        male2    = 4,
        nogender = 5,
        robot    = 6,
        machine1 = 7,
        machine2 = 8
    }
}
