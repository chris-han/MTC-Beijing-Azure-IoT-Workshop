using System;

namespace Microsoft.Azure.IoT.Studio.Device.DataGenerator
{
    internal class VendingMachine:Base
    {
        [DataGeneratorParameter]
        public Range VoltdropBoundary { get; private set; }

        [DataGeneratorParameter]
        public Range PowerdrawBoundary { get; private set; }

        [DataGeneratorParameter]
        public Range DutycycleBoundary { get; private set; }

        [DataGeneratorParameter]
        public Range VibrationBoundary { get; private set; }

        [DataGeneratorParameter]
        public Range TemperaturBoundary { get; private set; }

        private double _voltdrop;
        private double _powerdraw;
        private double _dutycycle;
        private double _vibration;
        private double _temperature;

        public VendingMachine()
        {
            VoltdropBoundary = new Range(max: 55, min: 30);
            PowerdrawBoundary = new Range(max: 50, min: 5);
            DutycycleBoundary = new Range(max: 1, min: 0); //(0~10)/10
            VibrationBoundary = new Range(max: 390, min: 30); //(0~12)*30+30
            TemperaturBoundary = new Range(max: 41, min: 35);

        }

        public override void OnInitialize()
        {
            _voltdrop= VoltdropBoundary.Min;
            _powerdraw= PowerdrawBoundary.Min;
            _dutycycle= DutycycleBoundary.Min;
            _vibration= VibrationBoundary.Min;
            _temperature= TemperaturBoundary.Min;


        }

        public override Data Read()
        {
            var output = new Data();
            output.Add("voltdrop", _rand.Next(VoltdropBoundary));
            output.Add("powerdraw", _rand.Next(PowerdrawBoundary));
            output.Add("dutycycle", _rand.Next(DutycycleBoundary));
            output.Add("vibration", _rand.Next(VibrationBoundary));
            output.Add("temperature", _rand.Next(TemperaturBoundary));
            return output;
        }
    }
}
