using System;

namespace PCTechnician.Power
{
    [Serializable]
    public class PowerNode
    {
        public string NodeId;
        public string SourceDeviceId;
        public string DestinationDeviceId;
        public PowerConnectorType ConnectorType;
        public float MaxSupportedPowerWatts;
        public float RequiredPowerWatts;
        public bool IsConnected;
        public bool HasFault;
        public string FaultMessage;

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
            IsConnected = false;
        }
    }
}