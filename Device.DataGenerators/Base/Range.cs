namespace Microsoft.Azure.IoT.Studio.Device
{
    public class Range
    {
        public double Max { get; private set; }
        public double Min { get; private set; }

        public Range(double max, double min)
        {
            Max = max;
            Min = min;
        }

        public bool IsInRange(double v)
        {
            return v <= Max && v >= Min;
        }

        public double Clip(double v)
        {
            if (v < Min)
            {
                return Min;
            }
            else if (v > Max)
            {
                return Max;
            }
            else
            {
                return v;
            }
        }

        public double Size
        {
            get { return Max - Min; }
        }
    }
}
