using System;

namespace PCTechnician.Power
{
    [Serializable]
    public struct PowerLoadProfile
    {
        public float ContinuousLoadWatts { get; set; }
        public float PeakLoadWatts { get; set; }
        public float TransientLoadWatts { get; set; }

        public PowerLoadProfile(float continuous, float peak, float transient)
        {
            ContinuousLoadWatts = continuous;
            PeakLoadWatts = peak;
            TransientLoadWatts = transient;
        }

        public static PowerLoadProfile Calculate(int cpuTdp, int cpuPeak, int gpuTdp, int gpuTransient, int baseSystemWatts = 60)
        {
            float continuous = cpuTdp + gpuTdp + baseSystemWatts;
            float peak = cpuPeak + (gpuTdp * 1.15f) + baseSystemWatts + 25f;
            float transient = cpuPeak + gpuTransient + baseSystemWatts + 40f;
            return new PowerLoadProfile(continuous, peak, transient);
        }
    }
}
