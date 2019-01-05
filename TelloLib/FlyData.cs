using System;
using System.Text;

namespace TelloLib
{
    public class LogData
    {
        public float VelX { get; set; }
        public float VelY { get; set; }
        public float VelZ { get; set; }

        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public float PosUncertainty { get; set; }

        public float VelN { get; set; }
        public float VelE { get; set; }
        public float VelD { get; set; }

        public float QuatX { get; set; }
        public float QuatY { get; set; }
        public float QuatZ { get; set; }
        public float QuatW { get; set; }
    }

    public class FlyData
    {
        public int FlyMode { get; private set; }
        public int Height { get; private set; }
        public int VerticalSpeed { get; private set; }
        public int FlySpeed { get; private set; }
        public int EastSpeed { get; private set; }
        public int NorthSpeed { get; private set; }
        public int FlyTime { get; private set; }
        public int MaxHeight { get; set; }

        public bool Flying { get; private set; }

        public bool DownVisualState { get; private set; }
        public bool DroneHover { get; private set; }
        public bool EmOpen { get; private set; }
        public bool OnGround { get; private set; }
        public bool PressureState { get; private set; }

        public int BatteryPercentage { get; private set; }
        public bool BatteryLow { get; private set; }
        public bool BatteryLower { get; private set; }
        public bool BatteryState { get; private set; }
        public bool PowerState { get; private set; }
        public int DroneBatteryLeft { get; private set; }
        public int DroneFlyTimeLeft { get; private set; }

        public int CameraState { get; private set; }
        public int ElectricalMachineryState { get; private set; }
        public bool FactoryMode { get; private set; }
        public bool FrontIn { get; private set; }
        public bool FrontLsc { get; private set; }
        public bool FrontOut { get; private set; }
        public bool GravityState { get; private set; }
        public int ImuCalibrationState { get; private set; }
        public bool ImuState { get; private set; }
        public bool OutageRecording { get; private set; }
        public int TemperatureHeight { get; private set; }
        public int ThrowFlyTimer { get; private set; }
        
        public bool WindState { get; private set; }
        public int WifiStrength { get; set; }

        //public int LightStrength { get; private set; }
        //public int WifiDisturb { get; private set; }
        //public int SmartVideoExitMode { get; private set; }

        private readonly LogData _logData = new LogData();

        public void Set(byte[] data)
        {
            var index = 0;
            Height = (short)(data[index] | (data[index + 1] << 8)); index += 2;
            NorthSpeed = (short)(data[index] | (data[index + 1] << 8)); index += 2;
            EastSpeed = (short)(data[index] | (data[index + 1] << 8)); index += 2;
            FlySpeed = ((int)Math.Sqrt(Math.Pow(NorthSpeed, 2.0D) + Math.Pow(EastSpeed, 2.0D)));
            VerticalSpeed = (short)(data[index] | (data[index + 1] << 8)); index += 2;  // ah.a(paramArrayOfByte[6], paramArrayOfByte[7]);
            FlyTime = data[index] | (data[index + 1] << 8); index += 2; // ah.a(paramArrayOfByte[8], paramArrayOfByte[9]);

            ImuState = (data[index] >> 0 & 0x1) == 1;
            PressureState = (data[index] >> 1 & 0x1) == 1;
            DownVisualState = (data[index] >> 2 & 0x1) == 1;
            PowerState = (data[index] >> 3 & 0x1) == 1;
            BatteryState = (data[index] >> 4 & 0x1) == 1;
            GravityState = (data[index] >> 5 & 0x1) == 1;
            WindState = (data[index] >> 7 & 0x1) == 1;
            index += 1;

            //if (paramArrayOfByte.length < 19) { }
            ImuCalibrationState = data[index]; index += 1;
            BatteryPercentage = data[index]; index += 1;
            DroneFlyTimeLeft = data[index] | (data[index + 1] << 8); index += 2;
            DroneBatteryLeft = data[index] | (data[index + 1] << 8); index += 2;

            //index 17
            Flying = (data[index] >> 0 & 0x1) == 1;
            OnGround = (data[index] >> 1 & 0x1) == 1;
            EmOpen = (data[index] >> 2 & 0x1) == 1;
            DroneHover = (data[index] >> 3 & 0x1) == 1;
            OutageRecording = (data[index] >> 4 & 0x1) == 1;
            BatteryLow = (data[index] >> 5 & 0x1) == 1;
            BatteryLower = (data[index] >> 6 & 0x1) == 1;
            FactoryMode = (data[index] >> 7 & 0x1) == 1;
            index += 1;

            FlyMode = data[index]; index += 1;
            ThrowFlyTimer = data[index]; index += 1;
            CameraState = data[index]; index += 1;

            //if (paramArrayOfByte.length >= 22)
            ElectricalMachineryState = data[index]; index += 1; //(paramArrayOfByte[21] & 0xFF);

            //if (paramArrayOfByte.length >= 23)
            FrontIn = (data[index] >> 0 & 0x1) == 1;
            FrontOut = (data[index] >> 1 & 0x1) == 1;
            FrontLsc = (data[index] >> 2 & 0x1) == 1;
            index += 1;
            TemperatureHeight = data[index] >> 0 & 0x1; //23

            //WifiStrength = _wifiStrength;   //Wifi str comes in a cmd.
        }

        //Parse some of the interesting info from the tello log stream
        public void ParseLog(byte[] data)
        {
            var pos = 0;

            //A packet can contain more than one record.
            while (pos < data.Length - 2)   //-2 for CRC bytes at end of packet.
            {
                if (data[pos] != 'U')//Check magic byte
                {
                    //Console.WriteLine("PARSE ERROR!!!");
                    break;
                }

                var len = data[pos + 1];
                if (data[pos + 2] != 0)//Should always be zero (so far)
                {
                    //Console.WriteLine("SIZE OVERFLOW!!!");
                    break;
                }

                var crc = data[pos + 3];
                var id = BitConverter.ToUInt16(data, pos + 4);
                var xorBuf = new byte[256];
                var xorValue = data[pos + 6];
                switch (id)
                {
                    case 0x1d://29 new_mvo
                        for (var i = 0; i < len; i++)//Decrypt payload.
                            xorBuf[i] = (byte)(data[pos + i] ^ xorValue);
                        var index = 10;//start of the velocity and pos data.
                        var observationCount = BitConverter.ToUInt16(xorBuf, index); index += 2;
                        _logData.VelX = BitConverter.ToInt16(xorBuf, index); index += 2;
                        _logData.VelY = BitConverter.ToInt16(xorBuf, index); index += 2;
                        _logData.VelZ = BitConverter.ToInt16(xorBuf, index); index += 2;
                        _logData.PosX = BitConverter.ToSingle(xorBuf, index); index += 4;
                        _logData.PosY = BitConverter.ToSingle(xorBuf, index); index += 4;
                        _logData.PosZ = BitConverter.ToSingle(xorBuf, index); index += 4;
                        _logData.PosUncertainty = BitConverter.ToSingle(xorBuf, index) * 10000.0f; index += 4;
                        //Console.WriteLine(observationCount + " " + posX + " " + posY + " " + posZ);
                        break;
                    case 0x0800://2048 imu
                        for (var i = 0; i < len; i++)//Decrypt payload.
                            xorBuf[i] = (byte)(data[pos + i] ^ xorValue);
                        var index2 = 10 + 48;//44 is the start of the quat data.
                        _logData.QuatW = BitConverter.ToSingle(xorBuf, index2); index2 += 4;
                        _logData.QuatX = BitConverter.ToSingle(xorBuf, index2); index2 += 4;
                        _logData.QuatY = BitConverter.ToSingle(xorBuf, index2); index2 += 4;
                        _logData.QuatZ = BitConverter.ToSingle(xorBuf, index2); index2 += 4;
                        //Console.WriteLine("qx:" + qX + " qy:" + qY+ "qz:" + qZ);

                        //var eular = toEuler(quatX, quatY, quatZ, quatW);
                        //Console.WriteLine(" Pitch:"+eular[0] * (180 / 3.141592) + " Roll:" + eular[1] * (180 / 3.141592) + " Yaw:" + eular[2] * (180 / 3.141592));

                        index2 = 10 + 76;//Start of relative velocity
                        _logData.VelN = BitConverter.ToSingle(xorBuf, index2); index2 += 4;
                        _logData.VelE = BitConverter.ToSingle(xorBuf, index2); index2 += 4;
                        _logData.VelD = BitConverter.ToSingle(xorBuf, index2); index2 += 4;
                        //Console.WriteLine(vN + " " + vE + " " + vD);

                        break;
                }

                pos += len;
            }
        }

        private double[] ToEuler(LogData logData)
        {
            var qX = logData.QuatX;
            var qY = logData.QuatY;
            var qZ = logData.QuatZ;
            var qW = logData.QuatW;

            double sqW = qW * qW;
            double sqX = qX * qX;
            double sqY = qY * qY;
            double sqZ = qZ * qZ;
            double yaw;
            double roll;
            double pitch;
            var retv = new double[3];
            var unit = sqX + sqY + sqZ + sqW;    // if normalized is one, otherwise
                                                 // is correction factor
            double test = qW * qX + qY * qZ;
            if (test > 0.499 * unit)
            { // singularity at north pole
                yaw = 2 * Math.Atan2(qY, qW);
                pitch = Math.PI / 2;
                roll = 0;
            }

            else if (test < -0.499 * unit)
            { // singularity at south pole
                yaw = -2 * Math.Atan2(qY, qW);
                pitch = -Math.PI / 2;
                roll = 0;
            }
            else
            {
                yaw = Math.Atan2(2.0 * (qW * qZ - qX * qY),
                        1.0 - 2.0 * (sqZ + sqX));
                roll = Math.Asin(2.0 * test / unit);
                pitch = Math.Atan2(2.0 * (qW * qY - qX * qZ),
                        1.0 - 2.0 * (sqY + sqX));
            }
            retv[0] = pitch;
            retv[1] = roll;
            retv[2] = yaw;
            return retv;
        }

        //For saving out state info.
        public string GetLogHeader()
        {
            var sb = new StringBuilder();
            foreach (var property in GetType().GetFields())
            {
                sb.Append(property.Name);
                sb.Append(",");
            }

            sb.AppendLine();
            return sb.ToString();
        }

        public string GetLogLine()
        {
            var sb = new StringBuilder();
            foreach (var property in GetType().GetFields())
            {
                if (property.FieldType == typeof(bool))
                {
                    sb.Append((bool)property.GetValue(this) ? "1" : "0");
                }
                else
                {
                    sb.Append(property.GetValue(this));
                }

                sb.Append(",");
            }
            sb.AppendLine();
            return sb.ToString();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            var count = 0;
            foreach (var property in GetType().GetFields())
            {
                sb.Append(property.Name);
                sb.Append(": ");
                sb.Append(property.GetValue(this));
                sb.Append(count++ % 2 == 1 ? Environment.NewLine : "      ");
            }

            return sb.ToString();
        }
    }
}
 