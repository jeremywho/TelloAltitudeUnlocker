using System;

namespace TelloLib
{
    public class Controller
    {
        private readonly Action<byte[]> _sendPacket;
        private static readonly ControllerState ControllerState = new ControllerState();
        private static readonly ControllerState AutoPilotControllerState = new ControllerState();

        public Controller(Action<byte[]> sendPacket)
        {
            _sendPacket = sendPacket;
        }

        //Create joystick packet from floating point axis.
        //Center = 0.0. 
        //Up/Right =1.0. 
        //Down/Left=-1.0. 
        private static byte[] CreateJoyPacket(float fRx, float fRy, float fLx, float fLy, float speed)
        {
            //template joy packet.
            var packet = new byte[] { 0xcc, 0xb0, 0x00, 0x7f, 0x60, 0x50, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x12, 0x16, 0x01, 0x0e, 0x00, 0x25, 0x54 };

            var axis1 = (short)(660.0F * fRx + 1024.0F);    //RightX center=1024 left =364 right =-364
            var axis2 = (short)(660.0F * fRy + 1024.0F);    //RightY down =364 up =-364
            var axis3 = (short)(660.0F * fLy + 1024.0F);    //LeftY down =364 up =-364
            var axis4 = (short)(660.0F * fLx + 1024.0F);    //LeftX left =364 right =-364
            var axis5 = (short)(660.0F * speed + 1024.0F);  //Speed. 

            if (speed > 0.1f)
                axis5 = 0x7fff;

            var packedAxis = ((long)axis1 & 0x7FF) | (((long)axis2 & 0x7FF) << 11) | ((0x7FF & (long)axis3) << 22) | ((0x7FF & (long)axis4) << 33) | ((long)axis5 << 44);
            packet[9] = ((byte)(int)(0xFF & packedAxis));
            packet[10] = ((byte)(int)(packedAxis >> 8 & 0xFF));
            packet[11] = ((byte)(int)(packedAxis >> 16 & 0xFF));
            packet[12] = ((byte)(int)(packedAxis >> 24 & 0xFF));
            packet[13] = ((byte)(int)(packedAxis >> 32 & 0xFF));
            packet[14] = ((byte)(int)(packedAxis >> 40 & 0xFF));

            //Add time info.		
            var now = DateTime.Now;
            packet[15] = (byte)now.Hour;
            packet[16] = (byte)now.Minute;
            packet[17] = (byte)now.Second;
            packet[18] = (byte)(now.Millisecond & 0xff);
            packet[19] = (byte)(now.Millisecond >> 8);

            Crc.CalcUCrc(packet, 4);//Not really needed.

            //calc crc for packet. 
            Crc.CalcCrc(packet, packet.Length);

            return packet;
        }

        public static float Clamp(float value, float min, float max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public void SendControllerUpdate()
        {
            var boost = 0.0f;
            if (ControllerState.Speed > 0)
                boost = 1.0f;

            //var limit = 1.0f;//Slow down while testing.
            //rx = rx * limit;
            //ry = ry * limit;
            var rx = ControllerState.Rx;
            var ry = ControllerState.Ry;
            var lx = ControllerState.Lx;
            var ly = ControllerState.Ly;
            if (true)   //Combine autopilot sticks.
            {
                rx = Clamp(rx + AutoPilotControllerState.Rx, -1.0f, 1.0f);
                ry = Clamp(ry + AutoPilotControllerState.Ry, -1.0f, 1.0f);
                lx = Clamp(lx + AutoPilotControllerState.Lx, -1.0f, 1.0f);
                ly = Clamp(ly + AutoPilotControllerState.Ly, -1.0f, 1.0f);
            }
            //Console.WriteLine(controllerState.rx + " " + controllerState.ry + " " + controllerState.lx + " " + controllerState.ly + " SP:"+boost);
            var packet = CreateJoyPacket(rx, ry, lx, ly, boost);
            try
            {
                _sendPacket?.Invoke(packet);
            }
            catch
            {
                // ignored
            }
        }
    }
}