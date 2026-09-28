using System;
using System.Collections.Generic;
using PCTechnician.Diagnostics;
using PCTechnician.Power;
using PCTechnician.Thermal;

namespace PCTechnician.Simulation
{
    [Serializable]
    public class SimulationEvaluationResult
    {
        public bool CanPostSucceed;
        public bool IsAssembledCorrectly;
        public bool IsPoweredCorrectly;
        public bool IsCpuThermallySafe;
        public bool IsGpuPowered;

        public PowerLoadProfile EstimatedLoad;
        public PSUEvaluationResult PSUStatusResult;
        public ThermalCalculationResult CPUThermalResult;

        public List<DiagnosticResult> DiagnosticFaults = new List<DiagnosticResult>();

        public DiagnosticResult PrimaryFault => DiagnosticFaults.Count > 0
            ? DiagnosticFaults[0]
            : DiagnosticResult.Ok();
    }
}