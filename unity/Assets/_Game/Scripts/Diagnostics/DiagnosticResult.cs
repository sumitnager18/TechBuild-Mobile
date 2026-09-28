using System;

namespace PCTechnician.Diagnostics
{
    [Serializable]
    public struct DiagnosticResult
    {
        public DiagnosticFaultCode FaultCode;
        public FaultSeverity Severity;
        public DiagnosticSubsystem Subsystem;
        public string Message;
        public string SuggestedInspectionTarget;

        public bool IsFatal => Severity == FaultSeverity.Error || Severity == FaultSeverity.Critical;

        public DiagnosticResult(DiagnosticFaultCode code, FaultSeverity severity, DiagnosticSubsystem subsystem, string message, string target)
        {
            FaultCode = code;
            Severity = severity;
            Subsystem = subsystem;
            Message = message;
            SuggestedInspectionTarget = target;
        }

        public static DiagnosticResult Ok()
        {
            return new DiagnosticResult(DiagnosticFaultCode.NONE, FaultSeverity.Info, DiagnosticSubsystem.General, "System checks normal.", string.Empty);
        }

        public static DiagnosticResult Error(DiagnosticFaultCode code, DiagnosticSubsystem subsystem, string message, string target)
        {
            return new DiagnosticResult(code, FaultSeverity.Error, subsystem, message, target);
        }

        public static DiagnosticResult Critical(DiagnosticFaultCode code, DiagnosticSubsystem subsystem, string message, string target)
        {
            return new DiagnosticResult(code, FaultSeverity.Critical, subsystem, message, target);
        }

        public static DiagnosticResult Warning(DiagnosticFaultCode code, DiagnosticSubsystem subsystem, string message, string target)
        {
            return new DiagnosticResult(code, FaultSeverity.Warning, subsystem, message, target);
        }
    }
}