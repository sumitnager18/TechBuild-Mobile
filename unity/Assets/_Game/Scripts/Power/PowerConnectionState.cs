using System;

namespace PCTechnician.Power
{
    [Serializable]
    public class PowerConnectionState
    {
        public string RailId;
        public string SourceConnectorId;

        public PowerConnectionState() { }

        public PowerConnectionState(string railId, string sourceConnectorId)
        {
            RailId = railId;
            SourceConnectorId = sourceConnectorId;
        }
    }
}