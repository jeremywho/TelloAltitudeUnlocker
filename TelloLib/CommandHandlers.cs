using System;
using System.IO;
using System.Linq;

namespace TelloLib
{
    public class CommandHandlers
    {
        private readonly FlyData _state;
        private readonly Messages _messages;
        private readonly Tello.UpdateDelegate _update;

        private static byte[] _picBuffer = new byte[3000 * 1024];
        private static bool[] _picChunkState;
        private static bool[] _picPieceState;
        private static uint _picBytesReceived;
        private static uint _picBytesExpected;
        //private static uint _picExtraPackets;
        public static bool PicDownloading { get; private set; }
        private static int _maxPieceNum;
        public static string PicPath;       //todo redo this. 
        public static string PicFilePath;   //todo redo this. 
        //public static int PicMode;          //pic or vid aspect ratio.

        public CommandHandlers(FlyData state, Messages messages, Tello.UpdateDelegate update)
        {
            _state = state;
            _messages = messages;
            _update = update;
        }

        public void Handle(int cmdId, byte[] bytes)
        {
            if (cmdId >= 74 && cmdId < 80)
            {
                //Console.WriteLine("XXXXXXXXCMD:" + cmdId);
            }

            switch (cmdId)
            {
                case Commands.TELLO_CMD_STATUS:
                    _state.Set(bytes.Skip(9).ToArray());
                    break;

                case Commands.TELLO_CMD_LOG_HEADER_WRITE:
                    //just ack.
                    var id = BitConverter.ToUInt16(bytes, 9);
                    _messages.SendAckLog((short)cmdId, id);
                    break;

                case Commands.TELLO_CMD_LOG_DATA_WRITE:
                    try
                    {
                        _state.ParseLog(bytes.Skip(10).ToArray());
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"parseLog error: [{BitConverter.ToString(bytes)}]" + e.Message);
                    }

                    break;

                case Commands.TELLO_CMD_LOG_CONFIGURATION:
                    //todo. this doesnt seem to be working.

                    //var id = BitConverter.ToUInt16(bytes, 9);
                    //var n2 = BitConverter.ToInt32(bytes, 11);
                    //sendAckLogConfig((short)cmdId, id,n2);

                    //var dataStr = BitConverter.ToString(bytes.Skip(14).Take(10).ToArray()).Replace("-", " ")/*+"  "+pos*/;


                    //Console.WriteLine(dataStr);
                    break;

                case Commands.TELLO_CMD_ATT_ANGLE:
                {
                    var array = bytes.Skip(10).Take(4).ToArray();
                    var f = BitConverter.ToSingle(array, 0);
                    Console.WriteLine(f);
                    break;
                }

                case Commands.TELLO_CMD_ALT_LIMIT:
                    _state.MaxHeight = BitConverter.ToUInt16(bytes, 10);
                    break;
                //wifi str command
                case 26:
                {
                    _state.WifiStrength = bytes[9];
                    if (bytes[10] != 0)//Disturb?
                    {
                    }

                    break;
                }
                //light str command
                case 53:
                    break;
                //start jpeg.
                case 98:
                {
                    var PicFilePath = PicPath + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".jpg";

                    var start = 9;
                    var ftype = bytes[start];
                    start += 1;
                    _picBytesExpected = BitConverter.ToUInt32(bytes, start);
                    if (_picBytesExpected > _picBuffer.Length)
                    {
                        Console.WriteLine("WARNING:Picture Too Big! " + _picBytesExpected);
                        _picBuffer = new byte[_picBytesExpected];
                    }
                    _picBytesReceived = 0;
                    _picChunkState = new bool[(_picBytesExpected / 1024) + 1]; //calc based on size. 
                    _picPieceState = new bool[(_picChunkState.Length / 8) + 1];
                    //_picExtraPackets = 0;//for debugging.
                    PicDownloading = true;

                    _messages.SendAckFileSize();
                    break;
                }
                //jpeg
                case 99:
                {
                    //var dataStr = BitConverter.ToString(bytes.Skip(0).Take(30).ToArray()).Replace("-", " ");

                    var start = 9;
                    var fileNum = BitConverter.ToUInt16(bytes, start);
                    start += 2;
                    var pieceNum = BitConverter.ToUInt32(bytes, start);
                    start += 4;
                    var seqNum = BitConverter.ToUInt32(bytes, start);
                    start += 4;
                    var size = BitConverter.ToUInt16(bytes, start);
                    start += 2;

                    _maxPieceNum = Math.Max((int)pieceNum, _maxPieceNum);
                    if (!_picChunkState[seqNum])
                    {
                        Array.Copy(bytes, start, _picBuffer, seqNum * 1024, size);
                        _picBytesReceived += size;
                        _picChunkState[seqNum] = true;

                        for (var p = 0; p < _picChunkState.Length / 8; p++)
                        {
                            var done = true;
                            for (var s = 0; s < 8; s++)
                            {
                                if (!_picChunkState[(p * 8) + s])
                                {
                                    done = false;
                                    break;
                                }
                            }

                            if (done && !_picPieceState[p])
                            {
                                _picPieceState[p] = true;
                                _messages.SendAckFilePiece(0, fileNum, (uint)p);
                                //Console.WriteLine("\nACK PN:" + p + " " + seqNum);
                            }
                        }
                        if (PicFilePath != null && _picBytesReceived >= _picBytesExpected)
                        {
                            PicDownloading = false;

                            _messages.SendAckFilePiece(1, 0, (UInt32)_maxPieceNum);//todo. Double check this. finalize

                            _messages.SendAckFileDone((int)_picBytesExpected);

                            //HACK.
                            //Send file done cmdId to the update listener so it knows the picture is done. 
                            //hack.
                            _update(100);
                            //hack.
                            //This is a hack because it is faking a message. And not a very good fake.
                            //HACK.

                            Console.WriteLine("\nDONE PN:" + pieceNum + " max: " + _maxPieceNum);

                            //Save raw data minus sequence.
                            using (var stream = new FileStream(PicFilePath, FileMode.Append))
                            {
                                stream.Write(_picBuffer, 0, (int)_picBytesExpected);
                            }
                        }
                    }
                    else
                    {
                        //_picExtraPackets++;//for debugging.

                        //if(picBytesRecived >= picBytesExpected)
                        //    Console.WriteLine("\nEXTRA PN:"+pieceNum+" max "+ maxPieceNum);
                    }

                    break;
                }
                case 100:
                    break;
            }

            //send command to listeners. 
            try
            {
                //fire update event.
                _update(cmdId);
            }
            catch (Exception ex)
            {
                //Fixed. Update errors do not cause disconnect.
                Console.WriteLine("onUpdate error:" + ex.Message);
                //break;
            }
        }
    }
}
