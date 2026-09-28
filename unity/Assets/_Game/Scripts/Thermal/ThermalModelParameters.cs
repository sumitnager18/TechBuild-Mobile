namespace PCTechnician.Thermal
{
    /// <summary>
    /// Central gameplay-tuning constants for the deterministic thermal model.
    /// These values are intentionally a game model, not a physical CFD solver.
    /// </summary>
    public static class ThermalModelParameters
    {
        public const float DefaultAmbientTempCelsius = 22.0f;
        public const float DefaultMaxSafeTempCelsius = 95.0f;
        public const float DefaultThrottlingOffsetCelsius = 7.0f;

        public const float CoolerResistanceNumerator = 150.0f;
        public const float CoolerResistanceScale = 0.24f;
        public const float ContactResistanceScale = 0.22f;
        public const float MinimumPasteEfficiency = 0.08f;
        public const float MinimumMountQuality = 0.15f;
        public const float DisconnectedFanCapacityFactor = 0.35f;
        public const float MinimumCoolerCapacityWatts = 50.0f;
        public const float MinimumAirflowFactor = 0.5f;

        public const float HotThresholdCelsius = 78.0f;
        public const float WarmThresholdCelsius = 65.0f;
        public const float NormalThresholdCelsius = 35.0f;
    }
}