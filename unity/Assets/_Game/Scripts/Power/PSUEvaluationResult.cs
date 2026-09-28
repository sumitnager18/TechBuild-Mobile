using System;

namespace PCTechnician.Power
{
    public enum PSUStatus
    {
        Healthy,
        Marginal,
        Insufficient,
        MissingConnector
    }

    [Serializable]
    public struct PSUEvaluationResult
    {
        public PSUStatus Status { get; set; }
        public float RatedWattage { get; set; }
        public float ContinuousHeadroomWatts { get; set; }
        public float PeakHeadroomWatts { get; set; }
        public string Message { get; set; }

        public bool CanSafelyPowerContinuous => Status == PSUStatus.Healthy || Status == PSUStatus.Marginal;

        public PSUEvaluationResult(PSUStatus status, float rated, float continuousHeadroom, float peakHeadroom, string message)
        {
            Status = status;
            RatedWattage = rated;
            ContinuousHeadroomWatts = continuousHeadroom;
            PeakHeadroomWatts = peakHeadroom;
            Message = message;
        }
    }
}
