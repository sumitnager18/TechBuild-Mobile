using System;

namespace PCTechnician.Thermal
{
    /// <summary>
    /// Gameplay tuning curve for thermal paste application.
    /// Amount is a normalized dispense quantity from 0.0 to 1.5.
    /// </summary>
    public static class ThermalPasteEvaluation
    {
        public static float CalculateEfficiency(float amount)
        {
            if (amount <= 0.01f)
                return ThermalModelParameters.MinimumPasteEfficiency;

            if (amount < 0.45f)
                return 0.15f + (amount / 0.45f) * 0.65f;

            if (amount <= 0.70f)
            {
                float distFromPeak = Math.Abs(amount - 0.55f);
                return 1.0f - (distFromPeak * 0.35f);
            }

            if (amount <= 1.0f)
                return 0.92f - ((amount - 0.70f) * 0.25f);

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