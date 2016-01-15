using System;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.Studio.Device
{
    static public class RandExtension
    {
        static public Random CreateByTask()
        {
            int seed = Task.CurrentId ?? 0;
            seed ^= (int)DateTime.Now.ToFileTime();

            return new Random(seed);
        }

        static public bool TestProb(this Random rand, double prob)
        {
            return rand.Next() < int.MaxValue * prob;
        }

        static public double Next(this Random rand, double min, double max)
        {
            return rand.NextDouble() * (max - min) + min;
        }

        static public double Next(this Random rand, Range boundary)
        {
            return rand.Next(boundary.Min, boundary.Max);
        }
    }
}
