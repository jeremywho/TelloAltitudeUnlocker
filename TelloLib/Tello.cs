using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TelloLib
{
    public class Tello
    {
        private static UdpUser _client;
        private static DateTime _lastMessageTime;   //for connection timeouts.
        private readonly bool _enableVideo;
        private readonly Action<Exception,string> _errorLog;
        private readonly bool _enableController;

        private ConnectionState _connectionState = ConnectionState.Disconnected;
        private CancellationTokenSource _cancelTokens = new CancellationTokenSource();   //used to cancel listeners
        private CommandHandlers _handlers;

        public bool Connected { get; private set; }        
        public int FrameRate = 5;    //How often to ask for iFrames in 50ms. Ie 2=10x 5=4x 10=2xSecond 5 = 4xSecond
        public FlyData State { get; } = new FlyData();
        public Controller Controller { get; private set; }
        public Messages Messages { get; private set; }

        public Tello(bool controllerEnabled, bool videoEnabled, Action<Exception,string> errorLog)
        {
            _enableController = controllerEnabled;
            _enableVideo = videoEnabled;
            _errorLog = errorLog;
        }

        private void Log(Exception e, string info = null)
        {
            _errorLog?.Invoke(e, info);
        }

        public delegate void UpdateDelegate(int cmdId);
        public event UpdateDelegate OnUpdate;

        public delegate void ConnectionDelegate(ConnectionState newState);
        public event ConnectionDelegate OnConnection;

        public delegate void VideoUpdateDelegate(byte[] data);
        public event VideoUpdateDelegate OnVideoData;

        private void SetConnectionState(ConnectionState newState)
        {
            var oldState = _connectionState;
            _connectionState = newState;

            if (oldState == newState) return;

            Connected = newState == ConnectionState.Connected;
            OnConnection?.Invoke(_connectionState);
        }

        private void Disconnect()
        {
            //kill listeners
            _cancelTokens.Cancel();

            if (_connectionState == ConnectionState.Connecting)
                return;

            SetConnectionState(ConnectionState.Disconnected);
        }

        private void Connect()
        {
            //Console.WriteLine("Connecting to tello.");
            _client = UdpUser.ConnectTo("192.168.10.1", 8889);

            Messages = new Messages(_client);
            _handlers = new CommandHandlers(State, Messages, OnUpdate);

            SetConnectionState(ConnectionState.Connecting);

            var connectPacket = Encoding.UTF8.GetBytes("conn_req:\x00\x00");
            connectPacket[connectPacket.Length - 2] = 0x96;
            connectPacket[connectPacket.Length - 1] = 0x17;
            _client.Send(connectPacket);
        }

        //Pause connections. Used by aTello when app paused.
        public void ConnectionSetPause(bool pause)
        {
            //NOTE only pause if connected and only un-pause (connect) when paused.
            if (pause && _connectionState == ConnectionState.Connected)
            {
                SetConnectionState(ConnectionState.Paused);
            }
            else if (pause == false && _connectionState == ConnectionState.Paused)
            {
                //NOTE:send un-pause and not connection event
                OnConnection?.Invoke(ConnectionState.UnPausing);

                _connectionState = ConnectionState.Connected;
            }
        }

        private void StartListeners()
        {
            _cancelTokens = new CancellationTokenSource();
            var token = _cancelTokens.Token;

            //wait for reply messages from the tello and process. 
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                   try
                    {
                        if (token.IsCancellationRequested)  //handle canceling thread.
                            break;

                        var received = await _client.Receive();
                        _lastMessageTime = DateTime.Now;    //for timeouts

                        if(_connectionState == ConnectionState.Connecting)
                        {
                            if(received.Message.StartsWith("conn_ack"))
                            {
                                SetConnectionState(ConnectionState.Connected);

                                if(_enableVideo || Controller != null)
                                    StartHeartbeat();

                                if (_enableController)
                                    Controller = new Controller(_client.Send);

                                if (_enableVideo)
                                    Messages.RequestIframe();

                                continue;
                            }
                        }

                        var cmdId = received.Bytes[5] | (received.Bytes[6] << 8);
                        _handlers.Handle(cmdId, received.Bytes);
                    }

                    catch (Exception e)
                    {
                        Log(e, "Receive thread error");
                        Disconnect();
                        break;
                    }
                }
            }, token);

            if (_enableVideo)
                StartVideoServer(token);
        }

        private void StartVideoServer(CancellationToken token)
        {
            //video server
            var videoServer = new UdpListener(6038);
            //var videoServer = new UdpListener(new IPEndPoint(IPAddress.Parse("192.168.10.2"), 6038));

            Task.Factory.StartNew(async () => {
                //Console.WriteLine("video:1");
                var started = false;

                while (true)
                {
                    try
                    {
                        if (token.IsCancellationRequested)//handle canceling thread.
                            break;
                        var received = await videoServer.Receive();
                        if (received.Bytes[2] == 0 && received.Bytes[3] == 0 && received.Bytes[4] == 0 && received.Bytes[5] == 1)   //Wait for first NAL
                        {
                            //var nal = (received.bytes[6] & 0x1f);
                            //if (nal != 0x01 && nal!=0x07 && nal != 0x08 && nal != 0x05)
                            //    Console.WriteLine("NAL type:" +nal);
                            started = true;
                        }

                        if (started)
                        {
                            OnVideoData?.Invoke(received.Bytes);
                        }

                    }
                    catch (Exception ex)
                    {
                        Log(ex, "Video receive thread error:");
                    }
                }
            }, token);
        }

        private void StartHeartbeat()
        {
            var token = _cancelTokens.Token;

            //heartbeat.
            Task.Factory.StartNew(() =>
            {
                var tick = 0;
                while (true)
                {
                    try
                    {
                        if (token.IsCancellationRequested)
                            return;

                        if (_connectionState == ConnectionState.Connected)  //only send if not paused
                        {
                            Controller?.SendControllerUpdate();

                            tick++;
                            if (_enableVideo && tick % FrameRate == 0)
                                Messages.RequestIframe();
                        }

                        Thread.Sleep(50);//Often enough?
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Heartbeat error:" + ex.Message);
                        if (ex.Message.StartsWith("Access denied") //Denied means app paused
                            && _connectionState != ConnectionState.Paused)
                        {
                            //Can this happen?
                            Console.WriteLine("Heartbeat: access denied and not paused:" + ex.Message);

                            Disconnect();
                            break;
                        }


                        if (!ex.Message.StartsWith("Access denied"))//Denied means app paused
                        {
                            Disconnect();
                            break;
                        }
                    }
                }
            }, token);
        }

        // check every 500ms to make sure we are still recv-ing msgs
        public void StartConnecting()
        {
            var token = _cancelTokens.Token;

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    try
                    {
                        if (token.IsCancellationRequested)
                            return;

                        switch (_connectionState)
                        {
                            case ConnectionState.Disconnected:
                                Connect();
                                _lastMessageTime = DateTime.Now;

                                StartListeners();

                                break;
                            case ConnectionState.Connecting:
                            case ConnectionState.Connected:
                                var elapsed = DateTime.Now - _lastMessageTime;
                                if (elapsed.Seconds > 2)//1 second timeout.
                                {
                                    //Console.WriteLine("Connection timeout :");
                                    Disconnect();
                                }
                                break;
                            case ConnectionState.Paused:
                                _lastMessageTime = DateTime.Now;//reset timeout so we have time to recover if enabled. 
                                break;
                        }

                        Thread.Sleep(500);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Connection thread error:" + ex.Message);
                    }
                }
            }, token);
        }
    }
}
 