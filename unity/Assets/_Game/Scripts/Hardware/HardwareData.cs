using UnityEngine;

namespace PCTechnician.Hardware
{
    /// <summary>
    /// Abstract base class for all hardware data definitions (ScriptableObjects).
    /// </summary>
    public abstract class HardwareData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string componentId;
        [SerializeField] private string modelName;
        [SerializeField] private string manufacturerName;
        [SerializeField] private ComponentCategory category;

        [Header("Physical Specs")]
        [SerializeField] private Vector3 dimensionsMm;
        [SerializeField] private float weightGrams;

        public string ComponentId => componentId;
        public string ModelName => modelName;
        public string ManufacturerName => manufacturerName;
        public ComponentCategory Category => category;
        public Vector3 DimensionsMm => dimensionsMm;
        public float WeightGrams => weightGrams;

        protected void InitializeBase(string id, string model, string manufacturer, ComponentCategory cat)
        {
            componentId = id;
            modelName = model;
            manufacturerName = manufacturer;
            category = cat;
        }
    }
}
