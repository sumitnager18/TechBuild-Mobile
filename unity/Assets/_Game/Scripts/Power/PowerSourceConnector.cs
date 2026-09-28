using System;

namespace PCTechnician.Power
{
    [Serializable]
    public sealed class PowerSourceConnector
    {
        public string ConnectorId;
        public string SourceDeviceId;
        public PowerConnectorType ConnectorType;
        public float MaxPowerWatts;
        public bool IsConnected;

        public PowerSourceConnector(string connectorId, string sourceDeviceId, PowerConnectorType connectorType, float maxPowerWatts)
        {
            ConnectorId = connectorId;
            SourceDeviceId = sourceDeviceId;
            ConnectorType = connectorType;
            MaxPowerWatts = maxPowerWatts;
            IsConnected = false;
        }
    }
}