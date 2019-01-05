using System;

namespace TelloLib
{
    public class Messages
    {
        private UdpUser _client;
        private static ushort _sequence = 1;

        public Messages(UdpUser client)
        {
            _client = client;
        }

        public void TakeOff()
        {
            var packet = new byte[] { 0xcc, 0x58, 0x00, 0x7c, 0x68, 0x54, 0x00, 0xe4, 0x01, 0xc2, 0x16 };
            SendPacket(packet);
        }

        public void ThrowTakeOff()
        {
            var packet = new byte[] { 0xcc, 0x58, 0x00, 0x7c, 0x48, 0x5d, 0x00, 0xe4, 0x01, 0xc2, 0x16 };
            SendPacket(packet);
        }

        public void Land()
        {
            var packet = new byte[] { 0xcc, 0x60, 0x00, 0x27, 0x68, 0x55, 0x00, 0xe5, 0x01, 0x00, 0xba, 0xc7 };

            //payload
            packet[9] = 0x00;   //todo. Find out what this is for.
            SendPacket(packet);
        }

        public void RequestIframe()
        {
            var iframePacket = new byte[] { 0xcc, 0x58, 0x00, 0x7c, 0x60, 0x25, 0x00, 0x00, 0x00, 0x6c, 0x95 };
            _client.Send(iframePacket);
        }

        public void SetMaxHeight(int height)
        {
            //                                          crc    typ  cmdL  cmdH  seqL  seqH  heiL  heiH  crc   crc
            var packet = new byte[] { 0xcc, 0x68, 0x00, 0x27, 0x68, 0x58, 0x00, 0x00, 0x00, 0x00, 0x00, 0x5b, 0xc5 };

            //payload
            packet[9] = (byte)(height & 0xff);
            packet[10] = (byte)((height >> 8) & 0xff);

            SendPacket(packet);

            QueryMaxHeight();  //refresh
        }

        public void QueryUnk(int cmd)
        {
            var packet = new byte[] { 0xcc, 0x58, 0x00, 0x7c, 0x48, 0xff, 0x00, 0x06, 0x00, 0xe9, 0xb3 };
            packet[5] = (byte)cmd;
            SendPacket(packet);
        }

        public void QueryAttAngle()
        {
            var packet = new byte[] { 0xcc, 0x58, 0x00, 0x7c, 0x48, 0x59, 0x10, 0x06, 0x00, 0xe9, 0xb3 };
            SendPacket(packet);
        }

        public void QueryMaxHeight()
        {
            var packet = new byte[] { 0xcc, 0x58, 0x00, 0x7c, 0x48, 0x56, 0x10, 0x06, 0x00, 0xe9, 0xb3 };
            SendPacket(packet);
        }

        public void SetAttAngle(float angle)
        {
            //                                          crc    typ  cmdL  cmdH  seqL  seqH  ang1  ang2 ang3  ang4  crc   crc
            var packet = new byte[] { 0xcc, 0x78, 0x00, 0x27, 0x68, 0x58, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x5b, 0xc5 };

            //payload
            var bytes = BitConverter.GetBytes(angle);
            packet[9] = bytes[0];
            packet[10] = bytes[1];
            packet[11] = bytes[2];
            packet[12] = bytes[3];

            SendPacket(packet);

            QueryAttAngle();  //refresh
        }

        public void SetEis(int value)
        {
            //                                          crc    typ  cmdL  cmdH  seqL  seqH  valL  crc   crc
            var packet = new byte[] { 0xcc, 0x60, 0x00, 0x27, 0x68, 0x24, 0x00, 0x09, 0x00, 0x00, 0x5b, 0xc5 };

            packet[9] = (byte)(value & 0xff);

            SendPacket(packet);
        }

        public void DoFlip(int dir)
        {
            //                                          crc    typ  cmdL  cmdH  seqL  seqH  dirL  crc   crc
            var packet = new byte[] { 0xcc, 0x60, 0x00, 0x27, 0x70, 0x5c, 0x00, 0x09, 0x00, 0x00, 0x5b, 0xc5 };

            packet[9] = (byte)(dir & 0xff);

            SendPacket(packet);
        }

        public void SetJpgQuality(int quality)
        {
            //                                          crc    typ  cmdL  cmdH  seqL  seqH  quaL  crc   crc
            var packet = new byte[] { 0xcc, 0x60, 0x00, 0x27, 0x68, 0x37, 0x00, 0x09, 0x00, 0x00, 0x5b, 0xc5 };

            packet[9] = (byte)(quality & 0xff);

            SendPacket(packet);
        }

        public void SetEv(int ev)
        {
            //                                          crc    typ  cmdL  cmdH  seqL  seqH  evL  crc   crc
            var packet = new byte[] { 0xcc, 0x60, 0x00, 0x27, 0x68, 0x34, 0x00, 0x09, 0x00, 0x00, 0x5b, 0xc5 };

            packet[9] = (byte)(ev - 9); //Exposure goes from -9 to +9;

            SendPacket(packet);
        }

        public void SetVideoBitRate(int rate)
        {
            //                                          crc    typ  cmdL  cmdH  seqL  seqH  rateL  crc   crc
            var packet = new byte[] { 0xcc, 0x60, 0x00, 0x27, 0x68, 0x20, 0x00, 0x09, 0x00, 0x00, 0x5b, 0xc5 };

            packet[9] = (byte)rate;

            SendPacket(packet);
        }

        public void SetVideoDynRate(int rate)
        {
            //                                          crc    typ  cmdL  cmdH  seqL  seqH  rateL  crc   crc
            var packet = new byte[] { 0xcc, 0x60, 0x00, 0x27, 0x68, 0x21, 0x00, 0x09, 0x00, 0x00, 0x5b, 0xc5 };

            packet[9] = (byte)rate;

            SendPacket(packet);
        }
        public void SetVideoRecord(int n)
        {
            //                                          crc    typ  cmdL  cmdH  seqL  seqH  nL  crc   crc
            var packet = new byte[] { 0xcc, 0x60, 0x00, 0x27, 0x68, 0x32, 0x00, 0x09, 0x00, 0x00, 0x5b, 0xc5 };

            packet[9] = (byte)n;

            SendPacket(packet);
        }

        /*TELLO_CMD_SWITCH_PICTURE_VIDEO
	    49 0x31
	    0x68
	    switching video stream mode
        data: u8 (1=video, 0=photo)
        */
        public void SetPicVidMode(int mode)
        {
            //                                          crc    typ  cmdL  cmdH  seqL  seqH  modL  crc   crc
            var packet = new byte[] { 0xcc, 0x60, 0x00, 0x27, 0x68, 0x31, 0x00, 0x00, 0x00, 0x00, 0x5b, 0xc5 };

            //PicMode = mode;

            packet[9] = (byte)(mode & 0xff);

            SendPacket(packet);
        }

        public void TakePicture()
        {
            //                                          crc    typ  cmdL  cmdH  seqL  seqH  crc   crc
            var packet = new byte[] { 0xcc, 0x58, 0x00, 0x7c, 0x68, 0x30, 0x00, 0x06, 0x00, 0xe9, 0xb3 };
            SendPacket(packet);
            Console.WriteLine("PIC START");
        }
        public void SendAckFilePiece(byte endFlag, ushort fileId, uint pieceId)
        {
            //                                          crc    typ  cmdL  cmdH  seqL  seqH  byte  nL    nH    n2L                     crc   crc
            var packet = new byte[] { 0xcc, 0x90, 0x00, 0x27, 0x50, 0x63, 0x00, 0xf0, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x5b, 0xc5 };

            packet[9] = endFlag;
            packet[10] = (byte)(fileId & 0xff);
            packet[11] = (byte)((fileId >> 8) & 0xff);

            packet[12] = ((byte)(int)(0xFF & pieceId));
            packet[13] = ((byte)(int)(pieceId >> 8 & 0xFF));
            packet[14] = ((byte)(int)(pieceId >> 16 & 0xFF));
            packet[15] = ((byte)(int)(pieceId >> 24 & 0xFF));

            SendPacket(packet);
        }

        public void SendAckFileSize()
        {
            //                                          crc    typ  cmdL  cmdH  seqL  seqH  modL  crc   crc
            var packet = new byte[] { 0xcc, 0x60, 0x00, 0x27, 0x50, 0x62, 0x00, 0x00, 0x00, 0x00, 0x5b, 0xc5 };
            SendPacket(packet);
        }

        public void SendAckFileDone(int size)
        {
            //                                          crc    typ  cmdL  cmdH  seqL  seqH  fidL  fidH  size  size  size  size  crc   crc
            var packet = new byte[] { 0xcc, 0x88, 0x00, 0x24, 0x48, 0x64, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x5b, 0xc5 };

            packet[11] = (byte)(0xFF & size);
            packet[12] = (byte)(size >> 8 & 0xFF);
            packet[13] = (byte)(size >> 16 & 0xFF);
            packet[14] = (byte)(size >> 24 & 0xFF);

            SendPacket(packet);
        }

        public void SendAckLog(short cmd, ushort id)
        {
            //                                          crc    typ  cmdL  cmdH  seqL  seqH  unk   idL   idH   crc   crc
            var packet = new byte[] { 0xcc, 0x70, 0x00, 0x27, 0x50, 0x50, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x5b, 0xc5 };

            var ba = BitConverter.GetBytes(cmd);
            packet[5] = ba[0];
            packet[6] = ba[1];

            ba = BitConverter.GetBytes(id);
            packet[10] = ba[0];
            packet[11] = ba[1];

            SendPacket(packet);
        }

        //this might not be working right 
        public void SendAckLogConfig(short cmd, ushort id, int n2)
        {
            //                                          crc    typ  cmdL  cmdH  seqL  seqH  unk   idL   idH  n2L   n2H  n2L   n2H   crc   crc
            var packet = new byte[] { 0xcc, 0xd0, 0x00, 0x27, 0x88, 0x50, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x5b, 0xc5 };

            var ba = BitConverter.GetBytes(cmd);
            packet[5] = ba[0];
            packet[6] = ba[1];

            ba = BitConverter.GetBytes(id);
            packet[10] = ba[0];
            packet[11] = ba[1];

            packet[12] = (byte)(0xFF & n2);
            packet[13] = (byte)(n2 >> 8 & 0xFF);
            packet[14] = (byte)(n2 >> 16 & 0xFF);
            packet[15] = (byte)(n2 >> 24 & 0xFF);

            //ba = BitConverter.GetBytes(n2);
            //packet[12] = ba[0];
            //packet[13] = ba[1];
            //packet[14] = ba[2];
            //packet[15] = ba[3];

            SendPacket(packet);
        }

        private void SendPacket(byte[] packet)
        {
            SetPacketSequence(packet);
            SetPacketCrCs(packet);
            _client.Send(packet);
        }

        private static void SetPacketSequence(byte[] packet)
        {
            packet[7] = (byte)(_sequence & 0xff);
            packet[8] = (byte)((_sequence >> 8) & 0xff);
            _sequence++;
        }

        private static void SetPacketCrCs(byte[] packet)
        {
            Crc.CalcUCrc(packet, 4);
            Crc.CalcCrc(packet, packet.Length);
        }
    }
}
