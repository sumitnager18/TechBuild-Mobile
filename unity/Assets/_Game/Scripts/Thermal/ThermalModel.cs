using System;

namespace PCTechnician.Thermal
{
    /// <summary>
    /// Deterministic gameplay thermal model.
    /// This is a tunable simulation model, not a physically validated thermal solver.
    /// </summary>
    public static class ThermalModel
    {
        public const float DEFAULT_AMBIENT_TEMP = ThermalModelParameters.DefaultAmbientTempCelsius;
        public const float CRITICAL_TEMP_THRESHOLD = ThermalModelParameters.DefaultMaxSafeTempCelsius;
        public const float THROTTLING_TEMP_THRESHOLD = ThermalModelParameters.DefaultMaxSafeTempCelsius - ThermalModelParameters.DefaultThrottlingOffsetCelsius;

        public static ThermalCalculationResult CalculateCPUThermals(
            float cpuPowerWatts,
            bool coolerInstalled,
            float coolerTdpRating,
            int coolerScrewsTightened,
            int totalScrewsRequired,
            bool fanConnected,
            float pasteAmount,
            float caseAirflowFactor = 1.0f,
            float ambientTemp = ThermalModelParameters.DefaultAmbientTempCelsius,
            float maxSafeTempCelsius = ThermalModelParameters.DefaultMaxSafeTempCelsius)
        {
            maxSafeTempCelsius = Math.Max(50.0f, maxSafeTempCelsius);
            float throttlingThreshold = Math.Max(40.0f, maxSafeTempCelsius - ThermalModelParameters.DefaultThrottlingOffsetCelsius);

            if (!coolerInstalled)
            {
                float emergencyTemp = maxSafeTempCelsius + 10.0f;
                return new ThermalCalculationResult(
                    emergencyTemp,
                    ThermalState.Critical,
                    0.0f,
                    0.0f,
                    0.0f,
                    $"CRITICAL FAULT: CPU cooler is not installed. Estimated temperature exceeds the {maxSafeTempCelsius:F0}°C CPU safety limit."
                );
            }

            float screwRatio = totalScrewsRequired > 0
                ? Math.Clamp((float)coolerScrewsTightened / totalScrewsRequired, 0f, 1f)
                : 1f;

            float mountQuality = Math.Max(ThermalModelParameters.MinimumMountQuality, screwRatio);
            float activeCoolerCapacity = fanConnected
                ? coolerTdpRating
                : coolerTdpRating * ThermalModelParameters.DisconnectedFanCapacityFactor;

            float pasteEfficiency = ThermalPasteEvaluation.CalculateEfficiency(pasteAmount);
            float rCooler = (ThermalModelParameters.CoolerResistanceNumerator /
                             Math.Max(ThermalModelParameters.MinimumCoolerCapacityWatts, activeCoolerCapacity)) *
                            ThermalModelParameters.CoolerResistanceScale;
            float rContact = ThermalModelParameters.ContactResistanceScale /
                             (Math.Max(ThermalModelParameters.MinimumPasteEfficiency, pasteEfficiency) *
                              Math.Max(ThermalModelParameters.MinimumMountQuality, mountQuality));

            float effectiveResistance = (rCooler + rContact) /
                                        Math.Max(ThermalModelParameters.MinimumAirflowFactor, caseAirflowFactor);
            float tempRise = cpuPowerWatts * effectiveResistance;
            float finalTemp = ambientTemp + tempRise;

            ThermalState state;
            float perfMultiplier;
            string message;

            if (finalTemp > maxSafeTempCelsius)
            {
                state = ThermalState.Critical;
                perfMultiplier = 0.0f;
                message = $"CPU temperature {finalTemp:F1}°C exceeds the configured {maxSafeTempCelsius:F1}°C safe threshold. Emergency thermal shutdown.";
            }
            else if (finalTemp >= throttlingThreshold)
            {
                state = ThermalState.Throttling;
                float range = Math.Max(1.0f, maxSafeTempCelsius - throttlingThreshold);
                float tRatio = (finalTemp - throttlingThreshold) / range;
                perfMultiplier = Math.Clamp(1.0f - (tRatio * 0.40f), 0.60f, 1.0f);
                message = $"CPU is thermal throttling at {finalTemp:F1}°C ({perfMultiplier * 100:F0}% performance).";
            }
            else if (finalTemp >= ThermalModelParameters.HotThresholdCelsius)
            {
                state = ThermalState.Hot;
                perfMultiplier = 1.0f;
                message = $"CPU operating hot at {finalTemp:F1}°C under sustained load.";
            }
            else if (finalTemp >= ThermalModelParameters.WarmThresholdCelsius)
            {
                state = ThermalState.Warm;
                perfMultiplier = 1.0f;
                message = $"CPU operating in normal warm range ({finalTemp:F1}°C).";
            }
            else if (finalTemp >= ThermalModelParameters.NormalThresholdCelsius)
            {
                state = ThermalState.Normal;
                perfMultiplier = 1.0f;
                message = $"CPU operating at optimal temperature ({finalTemp:F1}°C).";
            }
            else
            {
                state = ThermalState.Cold;
                perfMultiplier = 1.0f;
                message = $"CPU is idling near room temperature ({finalTemp:F1}°C).";
            }

            return new ThermalCalculationResult(finalTemp, state, perfMultiplier, pasteEfficiency, mountQuality, message);
        }
    }
}