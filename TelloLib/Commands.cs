namespace TelloLib
{
    public static class Commands
    {
        //https://bitbucket.org/PingguSoft/pytello/src/8ecc0037f05f16cb494aa29859a04d834f9c58c2/tello.py?at=master&fileviewer=file-view-default

        // TELLO COMMAND
        public const int TELLO_CMD_CONN = 1;
        public const int TELLO_CMD_CONN_ACK = 2;
        public const int TELLO_CMD_SSID = 17;                       // pt48
        public const int TELLO_CMD_SET_SSID = 18;                   // pt68
        public const int TELLO_CMD_SSID_PASS = 19;                  // pt48
        public const int TELLO_CMD_SET_SSID_PASS = 20;              // pt68
        public const int TELLO_CMD_REGION = 21;                     // pt48
        public const int TELLO_CMD_SET_REGION = 22;                 // pt68
        public const int TELLO_CMD_REQ_VIDEO_SPS_PPS = 37;          // pt60
        public const int TELLO_CMD_TAKE_PICTURE = 48;               // pt68
        public const int TELLO_CMD_SWITCH_PICTURE_VIDEO = 49;       // pt68
        public const int TELLO_CMD_START_RECORDING = 50;            // pt68
        public const int TELLO_CMD_SET_EV = 52;                     // pt48
        public const int TELLO_CMD_DATE_TIME = 70;                  // pt50
        public const int TELLO_CMD_STICK = 80;                      // pt60
        public const int TELLO_CMD_LOG_HEADER_WRITE = 4176;         // pt50
        public const int TELLO_CMD_LOG_DATA_WRITE = 4177;           // RX_O
        public const int TELLO_CMD_LOG_CONFIGURATION = 4178;        // pt50
        public const int TELLO_CMD_WIFI_SIGNAL = 26;                // RX_O
        public const int TELLO_CMD_VIDEO_BIT_RATE = 40;             // pt48
        public const int TELLO_CMD_LIGHT_STRENGTH = 53;             // RX_O
        public const int TELLO_CMD_VERSION_STRING = 69;             // pt48
        public const int TELLO_CMD_ACTIVATION_TIME = 71;            // pt48
        public const int TELLO_CMD_LOADER_VERSION = 73;             // pt48
        public const int TELLO_CMD_STATUS = 86;                     // RX_O
        public const int TELLO_CMD_ALT_LIMIT = 4182;                // pt48
        public const int TELLO_CMD_LOW_BATT_THRESHOLD = 4183;       // pt48
        public const int TELLO_CMD_ATT_ANGLE = 4185;                // pt48
        public const int TELLO_CMD_SET_JPEG_QUALITY = 55;           // pt68
        public const int TELLO_CMD_TAKEOFF = 84;                    // pt68
        public const int TELLO_CMD_LANDING = 85;                    // pt68
        public const int TELLO_CMD_SET_ALT_LIMIT = 88;              // pt68
        public const int TELLO_CMD_FLIP = 92;                       // pt70
        public const int TELLO_CMD_THROW_FLY = 93;                  // pt48
        public const int TELLO_CMD_PALM_LANDING = 94;               // pt48
        public const int TELLO_CMD_PLANE_CALIBRATION = 4180;        // pt68
        public const int TELLO_CMD_SET_LOW_BATTERY_THRESHOLD = 4181;// pt68
        public const int TELLO_CMD_SET_ATTITUDE_ANGLE = 4184;       // pt68
        public const int TELLO_CMD_ERROR1 = 67;                     // RX_O
        public const int TELLO_CMD_ERROR2 = 68;                     // RX_O
        public const int TELLO_CMD_FILE_SIZE = 98;                  // pt50
        public const int TELLO_CMD_FILE_DATA = 99;                  // pt50
        public const int TELLO_CMD_FILE_COMPLETE = 100;             // pt48
        public const int TELLO_CMD_HANDLE_IMU_ANGLE = 90;           // pt48
        public const int TELLO_CMD_SET_VIDEO_BIT_RATE = 32;         // pt68
        public const int TELLO_CMD_SET_DYN_ADJ_RATE = 33;           // pt68
        public const int TELLO_CMD_SET_EIS = 36;                    // pt68
        public const int TELLO_CMD_SMART_VIDEO_START = 128;         // pt68
        public const int TELLO_CMD_SMART_VIDEO_STATUS = 129;        // pt50
        public const int TELLO_CMD_BOUNCE = 4179;                   // pt68


        // Smart Video
        public const int TELLO_SMART_VIDEO_STOP = 0x00;
        public const int TELLO_SMART_VIDEO_START = 0x01;
        public const int TELLO_SMART_VIDEO_360 = 0x01;
        public const int TELLO_SMART_VIDEO_CIRCLE = 0x02;
        public const int TELLO_SMART_VIDEO_UP_OUT = 0x03;
    }
}