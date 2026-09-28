using System;

namespace PCTechnician.Thermal
{
    [Serializable]
    public struct ThermalCalculationResult
    {
        public float TemperatureCelsius { get; set; }
        public ThermalState State { get; set; }
        public float PerformanceMultiplier { get; set; } // 1.0 = 100%
        public float PasteEfficiency { get; set; }
        public float MountQuality { get; set; }
        public bool IsEmergencyShutdown => State == ThermalState.Critical;
        public string DiagnosticMessage { get; set; }

        public ThermalCalculationResult(float temp, ThermalState state, float perfMult, float pasteEff, float mountQual, string message)
        {
            TemperatureCelsius = temp;
            State = state;
            PerformanceMultiplier = perfMult;
            PasteEfficiency = pasteEff;
            MountQuality = mountQual;
            DiagnosticMessage = message;
        }
    }
}
