using System;

namespace PCTechnician.Thermal
{
    public static class ThermalPasteEvaluation
    {
        /// <summary>
        /// Evaluates thermal paste efficiency based on dispensed amount (0.0 to 1.5).
        /// 0.0 = No paste (bare metal dry contact).
        /// 0.1 - 0.4 = Insufficient coverage (hotspots).
        /// 0.45 - 0.65 = Optimal pea dot (maximum heat transfer).
        /// 0.7 - 0.9 = Acceptable coverage.
        /// > 1.0 = Excessive paste thickness causing slight thermal insulation.
        /// </summary>
        public static float CalculateEfficiency(float amount)
        {
            if (amount <= 0.01f)
            {
                return 0.08f; // Bare metal air gap: only ~8% heat transfer
            }

            if (amount < 0.45f)
            {
                // Too little: linear scale from 0.1 to 0.7
                return 0.15f + (amount / 0.45f) * 0.65f;
            }

            if (amount <= 0.70f)
            {
                // Optimal range (peak 1.0 at 0.55)
                float distFromPeak = Math.Abs(amount - 0.55f);
                return 1.0f - (distFromPeak * 0.35f);
            }

            if (amount <= 1.0f)
            {
                // Good but slightly thick
                return 0.92f - ((amount - 0.70f) * 0.25f);
            }

            // Excessive paste (> 1.0): thickness degrades performance down to ~0.72
            float excess = amount - 1.0f;
            return Math.Max(0.68f, 0.84f - (excess * 0.28f));
        }

        public static string GetQualityDescriptor(float amount)
        {
            if (amount <= 0.01f) return "None (Bare Metal)";
            if (amount < 0.40f) return "Insufficient (Dry Spots)";
            if (amount <= 0.70f) return "Optimal (Pea Dot)";
            if (amount <= 0.95f) return "Good";
            return "Excessive (Spill Risk)";
        }
    }
}
