namespace FakeChan
{
    public enum Methods
    {
        async,  // 非同期
        sync    // 同期
    }

    public enum VoiceIndex
    {
        IPC1 = 0,
        Socket1,
        Http1,
        Socket2,
        Http2,
        IPC2
    }
}
