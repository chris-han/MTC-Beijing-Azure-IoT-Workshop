using System;

namespace Microsoft.Azure.IoT.Studio.Device.DataGenerator
{
    internal class PeriodicMotorTemperature : Base
    {
        //Alias: AmbientTemp = Boundary.Min
        //Alias: UpperBound = Boundary.Max
        [DataGeneratorParameter]
        public Range Boundary { get; private set; }

        [DataGeneratorParameter]
        public double UpDecayMult { get; private set; }

        [DataGeneratorParameter]
        public double InitMult { get; private set; }

        [DataGeneratorParameter]
        public double DownDecayMult { get; private set; }

        [DataGeneratorParameter]
        public int StepsThreshold { get; private set; }

        [DataGeneratorParameter]
        public double HeatingProb { get; private set; }

        [DataGeneratorParameter]
        public double CoolingFailureProb { get; private set; }

        [DataGeneratorParameter]
        public double SensorFailureProb { get; private set; }

        [DataGeneratorParameter]
        public Range SensorFailureBoundary { get; private set; }

        [DataGeneratorParameter]
        public Range Fluctuating { get; private set; }

        private bool _up;
        private double _temperature;
        private double _multiplier;
        private int _runningSteps;
        private int _stepThreshold;

        public PeriodicMotorTemperature()
        {
            Boundary = new Range(max: 110.0, min: 35.0);
            UpDecayMult = 10.0 / 9;
            InitMult = 0.1;
            DownDecayMult = 1.0 / 36;
            StepsThreshold = 5;
            HeatingProb = 0.1;
            CoolingFailureProb = 1 - Math.Pow(0.9, 10.0 / 10);
            SensorFailureProb = 1 - Math.Pow(0.9, 1.0 / 10);
            SensorFailureBoundary = new Range(max: 300.0, min: 0.0);
            Fluctuating = new Range(max: 1.02, min: 0.98);
        }

        public override void OnInitialize()
        {
            _up = true;
            _temperature = Boundary.Min * (1 + InitMult);
            _multiplier = InitMult * UpDecayMult;
            _runningSteps = 0;
            _stepThreshold = StepsThreshold;

            Range = new Range(max: SensorFailureProb > 0 ? 300 : 100, min: 0);
        }

        public override Data Read()
        {
            double value = _temperature;

            if (_up)
            {
                double delta = _temperature * _multiplier;
                _temperature += delta;
                _multiplier /= UpDecayMult;

                //Turn on "cooling" if over-heat and "cooling failure" was not raised
                if (++_runningSteps >= _stepThreshold || _temperature >= Boundary.Max)
                {
                    _up = false;
                }
            }
            else
            {
                double delta = _temperature * DownDecayMult;
                _temperature -= delta;
                if (_temperature <= Boundary.Min)
                {
                    _temperature = Boundary.Min;

                    //Turn off "cooling" if fully cool-down after waiting for a random period
                    if (_rand.TestProb(HeatingProb))
                    {
                        _up = true;
                        _temperature = Boundary.Min * (1 + InitMult);
                        _multiplier = InitMult * UpDecayMult;
                        _runningSteps = 0;
                        _stepThreshold = _rand.TestProb(CoolingFailureProb) ? int.MaxValue : StepsThreshold;
                    }
                }
            }

            var output = new Data();

            if (_rand.TestProb(SensorFailureProb))
            {
                output.Add("Anomaly", true);
                output.Add("Temperature", _rand.Next(SensorFailureBoundary));
            }
            else
            {
                output.Add("Anomaly", _stepThreshold == int.MaxValue && _temperature > Boundary.Min);
                output.Add("Temperature", value * _rand.Next(Fluctuating));
            }

            return output;
        }
    }
}
