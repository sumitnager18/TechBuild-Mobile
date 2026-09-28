using UnityEngine;

namespace PCTechnician.Hardware
{
    [CreateAssetMenu(fileName = "Cable_Data", menuName = "PC Technician/Hardware/Cable Data")]
    public class CableData : HardwareData
    {
        [Header("Cable Routing")]
        [SerializeField] private string connectorOrigin = "PSU_Modular";
        [SerializeField] private string connectorDestination = "ATX_24Pin";
        [SerializeField] private int wireCount = 24;
        [SerializeField] private float cableLengthMm = 600f;

        public string ConnectorOrigin => connectorOrigin;
        public string ConnectorDestination => connectorDestination;
        public int WireCount => wireCount;
        public float CableLengthMm => cableLengthMm;

        public void SetRuntimeValues(string id, string model, string origin, string dest)
        {
            InitializeBase(id, model, "VoltEdge", ComponentCategory.Cable);
            connectorOrigin = origin;
            connectorDestination = dest;
        }
    }
}
