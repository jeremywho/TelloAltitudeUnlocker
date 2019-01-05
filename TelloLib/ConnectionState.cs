namespace TelloLib
{
    public enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
        Paused,     //used to keep from disconnecting when starved for input.
        UnPausing   //Transition. Never stays in this state. 
    }
}