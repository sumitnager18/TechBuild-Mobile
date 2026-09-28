using System;
using UnityEngine;

namespace PCTechnician.Core
{
    /// <summary>
    /// Contract for hardware objects that can be rotated, zoomed, and examined in detail.
    /// </summary>
    public interface IInspectable
    {
        string GetInspectionTitle();
        string GetTechnicalSummary();
        Transform GetInspectionAnchor();
        void BeginInspection();
        void EndInspection();
    }
}
