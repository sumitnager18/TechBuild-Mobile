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
        public PSUStatus Status;
        public float RatedWattage;
        public float ContinuousHeadroomWatts;
        public float PeakHeadroomWatts;
        public float TransientHeadroomWatts;
        public string Message;

        public bool CanSafelyPowerContinuous => Status == PSUStatus.Healthy || Status == PSUStatus.Marginal;

        public PSUEvaluationResult(PSUStatus status, float rated, float continuousHeadroom, float peakHeadroom, float transientHeadroom, string message)
        {
            Status = status;
            RatedWattage = rated;
            ContinuousHeadroomWatts = continuousHeadroom;
            PeakHeadroomWatts = peakHeadroom;
            TransientHeadroomWatts = transientHeadroom;
            Message = message;
        }
    }
}