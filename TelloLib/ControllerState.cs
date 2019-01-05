namespace TelloLib
{
    public class ControllerState
    {
        public float Rx { get; private set; }
        public float Ry { get; private set; }
        public float Lx { get; private set; }
        public float Ly { get; private set; }

        public int Speed { get; private set; }
        //private double _deadBand = 0.15;

        public void SetAxis(float lx, float ly, float rx, float ry)
        {
            //var deadBand = 0.15f;
            //this.rx = Math.Abs(rx) < deadBand ? 0.0f : rx;
            //this.ry = Math.Abs(ry) < deadBand ? 0.0f : ry;
            //this.lx = Math.Abs(lx) < deadBand ? 0.0f : lx;
            //this.ly = Math.Abs(ly) < deadBand ? 0.0f : ly;

            this.Rx = rx;
            this.Ry = ry;
            this.Lx = lx;
            this.Ly = ly;

            //Console.WriteLine(rx + " " + ry + " " + lx + " " + ly + " SP:" + speed);
        }
        public void SetSpeedMode(int mode)
        {
            Speed = mode;

            //Console.WriteLine(rx + " " + ry + " " + lx + " " + ly + " SP:" + speed);
        }
    }
}