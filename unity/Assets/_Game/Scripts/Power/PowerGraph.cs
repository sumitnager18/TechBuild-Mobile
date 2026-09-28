using System;
using System.Collections.Generic;

namespace PCTechnician.Power
{
    /// <summary>
    /// Directed source -> destination power graph.
    /// Source connectors are unique resources: one physical connector cannot power two destinations.
    /// </summary>
    public class PowerGraph
    {
        private readonly Dictionary<string, PowerNode> nodes = new Dictionary<string, PowerNode>();
        private readonly Dictionary<string, PowerSourceConnector> sourceConnectors = new Dictionary<string, PowerSourceConnector>();
        private readonly Dictionary<string, string> connectedSourceByNode = new Dictionary<string, string>();

        public IReadOnlyDictionary<string, PowerNode> Nodes => nodes;
        public IReadOnlyDictionary<string, PowerSourceConnector> SourceConnectors => sourceConnectors;

        public void Clear()
        {
            nodes.Clear();
            sourceConnectors.Clear();
            connectedSourceByNode.Clear();
        }

        public void AddNode(PowerNode node)
        {
            if (node == null || string.IsNullOrEmpty(node.NodeId)) return;
            nodes[node.NodeId] = node;
        }

        public void AddSourceConnector(PowerSourceConnector connector)
        {
            if (connector == null || string.IsNullOrEmpty(connector.ConnectorId)) return;
            sourceConnectors[connector.ConnectorId] = connector;
        }

        public bool ConnectRail(string nodeId, string sourceConnectorId, out string error)
        {
            if (!nodes.TryGetValue(nodeId, out var node))
            {
                error = $"Power destination node '{nodeId}' does not exist.";
                return false;
            }

            if (!sourceConnectors.TryGetValue(sourceConnectorId, out var source))
            {
                error = $"Power source connector '{sourceConnectorId}' does not exist.";
                node.SetFault(error);
                return false;
            }

            if (source.IsConnected)
            {
                error = $"Power source connector '{sourceConnectorId}' is already connected.";
                node.SetFault(error);
                return false;
            }

            if (!string.Equals(source.SourceDeviceId, node.SourceDeviceId, StringComparison.OrdinalIgnoreCase))
            {
                error = $"Power source '{source.SourceDeviceId}' cannot feed node '{node.NodeId}' from expected source '{node.SourceDeviceId}'.";
                node.SetFault(error);
                return false;
            }

            if (source.ConnectorType != node.ConnectorType)
            {
                error = $"Connector mismatch: Node '{nodeId}' requires '{node.ConnectorType}', but source '{sourceConnectorId}' provides '{source.ConnectorType}'.";
                node.SetFault(error);
                return false;
            }

            if (node.RequiredPowerWatts > source.MaxPowerWatts)
            {
                error = $"Source connector '{sourceConnectorId}' supports {source.MaxPowerWatts:F0}W but destination requires {node.RequiredPowerWatts:F0}W.";
                node.SetFault(error);
                return false;
            }

            source.IsConnected = true;
            connectedSourceByNode[nodeId] = sourceConnectorId;
            node.Connect();
            error = string.Empty;
            return true;
        }

        // Backwards-compatible helper: automatically selects one unused source connector of the requested type.
        public bool ConnectRail(string nodeId, PowerConnectorType providedConnector, out string error)
        {
            if (sourceConnectors.Count == 0)
            {
                if (!nodes.TryGetValue(nodeId, out var legacyNode))
                {
                    error = $"Power destination node '{nodeId}' does not exist.";
                    return false;
                }
                if (legacyNode.ConnectorType != providedConnector)
                {
                    error = $"Connector mismatch: Node '{nodeId}' requires '{legacyNode.ConnectorType}', but provided '{providedConnector}'.";
                    legacyNode.SetFault(error);
                    return false;
                }
                legacyNode.Connect();
                error = string.Empty;
                return true;
            }

            foreach (var pair in sourceConnectors)
            {
                if (!pair.Value.IsConnected && pair.Value.ConnectorType == providedConnector)
                    return ConnectRail(nodeId, pair.Key, out error);
            }

            if (!nodes.TryGetValue(nodeId, out var node))
            {
                error = $"Power destination node '{nodeId}' does not exist.";
                return false;
            }

            error = $"No unused PSU connector of type '{providedConnector}' is available.";
            node.SetFault(error);
            return false;
        }

        public void DisconnectRail(string nodeId)
        {
            if (!nodes.TryGetValue(nodeId, out var node)) return;

            if (connectedSourceByNode.TryGetValue(nodeId, out var sourceConnectorId) &&
                sourceConnectors.TryGetValue(sourceConnectorId, out var source))
            {
                source.IsConnected = false;
            }

            connectedSourceByNode.Remove(nodeId);
            node.Disconnect();
        }

        public bool IsRailConnected(string nodeId)
        {
            return nodes.TryGetValue(nodeId, out var node) && node.IsConnected && !node.HasFault;
        }

        public PSUEvaluationResult EvaluatePSU(
            float psuWattage,
            float transientExcursionPercentage,
            PowerLoadProfile load,
            int availablePcie8Pin,
            int requiredPcie8Pin)
        {
            if (psuWattage <= 0)
            {
                return new PSUEvaluationResult(
                    PSUStatus.Insufficient, 0,
                    -load.ContinuousLoadWatts,
                    -load.PeakLoadWatts,
                    -load.TransientLoadWatts,
                    "PSU is absent or zero wattage."
                );
            }

            if (availablePcie8Pin < requiredPcie8Pin)
            {
                return new PSUEvaluationResult(
                    PSUStatus.MissingConnector,
                    psuWattage,
                    psuWattage - load.ContinuousLoadWatts,
                    psuWattage - load.PeakLoadWatts,
                    psuWattage - load.TransientLoadWatts,
                    $"PSU lacks required PCIe power connectors ({availablePcie8Pin} available, {requiredPcie8Pin} required)."
                );
            }

            float excursionFactor = Math.Max(1.0f, transientExcursionPercentage / 100.0f);
            float transientCapacity = psuWattage * excursionFactor;
            float continuousHeadroom = psuWattage - load.ContinuousLoadWatts;
            float peakHeadroom = psuWattage - load.PeakLoadWatts;
            float transientHeadroom = transientCapacity - load.TransientLoadWatts;

            if (continuousHeadroom < 0 || peakHeadroom < 0 || transientHeadroom < 0)
            {
                return new PSUEvaluationResult(
                    PSUStatus.Insufficient,
                    psuWattage,
                    continuousHeadroom,
                    peakHeadroom,
                    transientHeadroom,
                    $"PSU capacity is insufficient: {load.ContinuousLoadWatts:F0}W continuous, {load.PeakLoadWatts:F0}W peak, {load.TransientLoadWatts:F0}W transient versus {psuWattage:F0}W rated / {transientCapacity:F0}W configured transient capacity."
                );
            }

            if (continuousHeadroom < psuWattage * 0.15f)
            {
                return new PSUEvaluationResult(
                    PSUStatus.Marginal,
                    psuWattage,
                    continuousHeadroom,
                    peakHeadroom,
                    transientHeadroom,
                    $"PSU wattage is marginal with low continuous headroom ({continuousHeadroom:F0}W remaining)."
                );
            }

            return new PSUEvaluationResult(
                PSUStatus.Healthy,
                psuWattage,
                continuousHeadroom,
                peakHeadroom,
                transientHeadroom,
                "PSU delivers adequate, stable power headroom."
            );
        }

        // Compatibility overload for existing callers; new simulation code must use PSU-configured transient excursion.
        public PSUEvaluationResult EvaluatePSU(float psuWattage, PowerLoadProfile load, int availablePcie8Pin, int requiredPcie8Pin)
        {
            return EvaluatePSU(psuWattage, 110.0f, load, availablePcie8Pin, requiredPcie8Pin);
        }
    }
}