using System;

namespace Microsoft.Azure.IoT.Studio.Device.DataGenerator
{
    internal class VendingMachine:Base
    {
        [DataGeneratorParameter]
        public Range Boundary { get; private set; }

        [DataGeneratorParameter]
        public int Period { get; private set; }

        [DataGeneratorParameter]
        public Range Fluctuating { get; private set; }

        private double _t;
        private double _delta;
        private double _k;
        private double _b;

        public VendingMachine()
        {
            Boundary = new Range(max: 55.0, min: 35.0);
            Period = 30;
        }

        public override void OnInitialize()
        {
            _t = 0;
            _delta = 2 * Math.PI / Period;
            _k = Boundary.Size / 2;
            _b = Boundary.Max - _k;

            Range = Boundary;
        }

        public override Data Read()
        {
            _t += _delta;

            var output = new Data();
            output.Add("Temp", (Math.Sin(_t) * _k + _b) * _rand.Next(Fluctuating));
            output.Add("Volt", 200 + 20 * _rand.Next(Fluctuating)); //200 +- 20 V
            return output;
        }
    }
}
