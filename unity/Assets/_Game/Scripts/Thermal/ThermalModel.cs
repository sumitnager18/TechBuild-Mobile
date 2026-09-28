using System;

namespace PCTechnician.Thermal
{
    /// <summary>
    /// Deterministic, lightweight thermal model.
    /// Pure C# class independent of Unity visual GameObjects and physics.
    /// </summary>
    public static class ThermalModel
    {
        public const float DEFAULT_AMBIENT_TEMP = 22.0f;
        public const float CRITICAL_TEMP_THRESHOLD = 98.0f;
        public const float THROTTLING_TEMP_THRESHOLD = 88.0f;

        /// <summary>
        /// Calculates CPU temperature and thermal state deterministically.
        /// </summary>
        public static ThermalCalculationResult CalculateCPUThermals(
            float cpuPowerWatts,
            bool coolerInstalled,
            float coolerTdpRating,
            int coolerScrewsTightened,
            int totalScrewsRequired,
            bool fanConnected,
            float pasteAmount,
            float caseAirflowFactor = 1.0f,
            float ambientTemp = DEFAULT_AMBIENT_TEMP)
        {
            // Case 1: No cooler mounted at all -> Immediate thermal trip
            if (!coolerInstalled)
            {
                return new ThermalCalculationResult(
                    105.0f,
                    ThermalState.Critical,
                    0.0f,
                    0.0f,
                    0.0f,
                    "CRITICAL FAULT: CPU cooler is not installed. Emergency thermal halt."
                );
            }

            // Case 2: Cooler mount quality (based on screw tightening)
            float screwRatio = totalScrewsRequired > 0 
                ? Math.Clamp((float)coolerScrewsTightened / totalScrewsRequired, 0f, 1f) 
                : 1f;
            
            // Loose cooler has poor contact pressure
            float mountQuality = Math.Max(0.15f, screwRatio);

            // Fan connection penalty (fan disconnected reduces cooler capacity by 65%)
            float activeCoolerCapacity = fanConnected ? coolerTdpRating : (coolerTdpRating * 0.35f);

            // Thermal paste efficiency
            float pasteEfficiency = ThermalPasteEvaluation.CalculateEfficiency(pasteAmount);

            // Effective thermal resistance formula (documented in architecture)
            // Cooler base resistance
            float rCooler = (150.0f / Math.Max(50.0f, activeCoolerCapacity)) * 0.24f;
            // Contact interface resistance (penalized by poor paste or uneven screw torque)
            float rContact = 0.22f / (Math.Max(0.08f, pasteEfficiency) * Math.Max(0.15f, mountQuality));
            
            float effectiveResistance = (rCooler + rContact) / Math.Max(0.5f, caseAirflowFactor);
            float tempRise = cpuPowerWatts * effectiveResistance;
            float finalTemp = ambientTemp + tempRise;

            // Determine thermal state and performance multiplier
            ThermalState state;
            float perfMultiplier;
            string message;

            if (finalTemp > CRITICAL_TEMP_THRESHOLD)
            {
                state = ThermalState.Critical;
                perfMultiplier = 0.0f;
                message = $"CPU temperature {finalTemp:F1}°C exceeds 98°C safe threshold. Emergency thermal shutdown.";
            }
            else if (finalTemp >= THROTTLING_TEMP_THRESHOLD)
            {
                state = ThermalState.Throttling;
                // Linear drop from 1.0 down to 0.60
                float tRatio = (finalTemp - THROTTLING_TEMP_THRESHOLD) / (CRITICAL_TEMP_THRESHOLD - THROTTLING_TEMP_THRESHOLD);
                perfMultiplier = Math.Clamp(1.0f - (tRatio * 0.40f), 0.60f, 1.0f);
                message = $"CPU is thermal throttling at {finalTemp:F1}°C ({perfMultiplier * 100:F0}% performance).";
            }
            else if (finalTemp >= 78.0f)
            {
                state = ThermalState.Hot;
                perfMultiplier = 1.0f;
                message = $"CPU operating hot at {finalTemp:F1}°C under sustained load.";
            }
            else if (finalTemp >= 65.0f)
            {
                state = ThermalState.Warm;
                perfMultiplier = 1.0f;
                message = $"CPU operating in normal warm range ({finalTemp:F1}°C).";
            }
            else if (finalTemp >= 35.0f)
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
