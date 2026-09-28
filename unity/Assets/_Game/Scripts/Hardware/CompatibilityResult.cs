using PCTechnician.Diagnostics;

namespace PCTechnician.Hardware
{
    public readonly struct CompatibilityResult
    {
        public bool IsCompatible { get; }
        public string ErrorCode { get; }
        public string UserMessage { get; }
        public FaultSeverity Severity { get; }

        public CompatibilityResult(bool isCompatible, string errorCode, string userMessage, FaultSeverity severity = FaultSeverity.Error)
        {
            IsCompatible = isCompatible;
            ErrorCode = errorCode;
            UserMessage = userMessage;
            Severity = isCompatible ? FaultSeverity.Info : severity;
        }

        public static CompatibilityResult Success()
        {
            return new CompatibilityResult(true, string.Empty, "Compatible.", FaultSeverity.Info);
        }

        public static CompatibilityResult Fail(string errorCode, string message, FaultSeverity severity = FaultSeverity.Error)
        {
            return new CompatibilityResult(false, errorCode, message, severity);
        }
    }
}
