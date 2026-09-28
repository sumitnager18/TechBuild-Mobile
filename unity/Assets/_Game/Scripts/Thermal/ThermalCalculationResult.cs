using System;

namespace PCTechnician.Thermal
{
    [Serializable]
    public struct ThermalCalculationResult
    {
        public float TemperatureCelsius;
        public ThermalState State;
        public float PerformanceMultiplier;
        public float PasteEfficiency;
        public float MountQuality;
        public bool IsEmergencyShutdown => State == ThermalState.Critical;
        public string DiagnosticMessage;

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