using System;

namespace PCTechnician.Power
{
    /// <summary>
    /// Conceptual power node representation for the directed Power Graph.
    /// Pure C# model with no Unity visual dependencies.
    /// </summary>
    [Serializable]
    public class PowerNode
    {
        public string NodeId { get; set; }
        public string SourceDeviceId { get; set; }
        public string DestinationDeviceId { get; set; }
        public PowerConnectorType ConnectorType { get; set; }
        public float MaxSupportedPowerWatts { get; set; }
        public float RequiredPowerWatts { get; set; }
        public bool IsConnected { get; set; }
        public bool HasFault { get; set; }
        public string FaultMessage { get; set; }

        public PowerNode(string nodeId, string source, string destination, PowerConnectorType connector, float maxWatts, float reqWatts)
        {
            NodeId = nodeId;
            SourceDeviceId = source;
            DestinationDeviceId = destination;
            ConnectorType = connector;
            MaxSupportedPowerWatts = maxWatts;
            RequiredPowerWatts = reqWatts;
            IsConnected = false;
            HasFault = false;
            FaultMessage = string.Empty;
        }

        public void Connect()
        {
            IsConnected = true;
            HasFault = false;
            FaultMessage = string.Empty;
        }

        public void Disconnect()
        {
            IsConnected = false;
        }

        public void SetFault(string message)
        {
            HasFault = true;
            FaultMessage = message;
        }
    }
}
