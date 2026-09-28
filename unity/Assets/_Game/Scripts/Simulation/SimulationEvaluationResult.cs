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
        public bool CanPostSucceed { get; set; }
        public bool IsAssembledCorrectly { get; set; }
        public bool IsPoweredCorrectly { get; set; }
        public bool IsCpuThermallySafe { get; set; }
        public bool IsGpuPowered { get; set; }

        public PowerLoadProfile EstimatedLoad { get; set; }
        public PSUEvaluationResult PSUStatusResult { get; set; }
        public ThermalCalculationResult CPUThermalResult { get; set; }

        public List<DiagnosticResult> DiagnosticFaults { get; set; } = new List<DiagnosticResult>();

        public DiagnosticResult PrimaryFault => DiagnosticFaults.Count > 0 
            ? DiagnosticFaults[0] 
            : DiagnosticResult.Ok();
    }
}
