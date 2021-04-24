namespace FakeChan
{
    public enum Methods
    {
        async,  // 非同期
        sync    // 同期
    }

    public enum VoiceIndex
    {
        IPC1    = 0,
        Socket1 = 1,
        Http1   = 2,
        Socket2 = 3,
        Http2   = 4,
        IPC2    = 5
    }
}
