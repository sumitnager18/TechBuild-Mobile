using System;
using System.Collections.Generic;

namespace PCTechnician.Power
{
    /// <summary>
    /// Directed Power Graph for deterministic power routing and load validation.
    /// Pure C# class with zero dependencies on visual GameObjects or MonoBehaviours.
    /// </summary>
    public class PowerGraph
    {
        private readonly Dictionary<string, PowerNode> nodes = new Dictionary<string, PowerNode>();

        public IReadOnlyDictionary<string, PowerNode> Nodes => nodes;

        public void Clear()
        {
            nodes.Clear();
        }

        public void AddNode(PowerNode node)
        {
            if (node == null || string.IsNullOrEmpty(node.NodeId)) return;
            nodes[node.NodeId] = node;
        }

        public bool ConnectRail(string nodeId, PowerConnectorType providedConnector, out string error)
        {
            if (!nodes.TryGetValue(nodeId, out var node))
            {
                error = $"Power destination node '{nodeId}' does not exist.";
                return false;
            }

            // Connector Validation (Section 10)
            if (node.ConnectorType != providedConnector)
            {
                error = $"Connector mismatch: Node '{nodeId}' requires '{node.ConnectorType}', but provided '{providedConnector}'.";
                node.SetFault(error);
                return false;
            }

            node.Connect();
            error = string.Empty;
            return true;
        }

        public void DisconnectRail(string nodeId)
        {
            if (nodes.TryGetValue(nodeId, out var node))
            {
                node.Disconnect();
            }
        }

        public bool IsRailConnected(string nodeId)
        {
            return nodes.TryGetValue(nodeId, out var node) && node.IsConnected && !node.HasFault;
        }

        public PSUEvaluationResult EvaluatePSU(float psuWattage, PowerLoadProfile load, int availablePcie8Pin, int requiredPcie8Pin)
        {
            if (psuWattage <= 0)
            {
                return new PSUEvaluationResult(PSUStatus.Insufficient, 0, -load.ContinuousLoadWatts, -load.PeakLoadWatts, "PSU is absent or zero wattage.");
            }

            if (availablePcie8Pin < requiredPcie8Pin)
            {
                return new PSUEvaluationResult(
                    PSUStatus.MissingConnector,
                    psuWattage,
                    psuWattage - load.ContinuousLoadWatts,
                    psuWattage - load.PeakLoadWatts,
                    $"PSU lacks required PCIe power connectors ({availablePcie8Pin} available, {requiredPcie8Pin} required)."
                );
            }

            float continuousHeadroom = psuWattage - load.ContinuousLoadWatts;
            float peakHeadroom = (psuWattage * 1.10f) - load.PeakLoadWatts; // 10% transient allowance

            if (continuousHeadroom < 0 || peakHeadroom < 0)
            {
                return new PSUEvaluationResult(
                    PSUStatus.Insufficient,
                    psuWattage,
                    continuousHeadroom,
                    peakHeadroom,
                    $"PSU wattage ({psuWattage}W) is insufficient for estimated load ({load.ContinuousLoadWatts}W continuous, {load.PeakLoadWatts}W peak)."
                );
            }

            if (continuousHeadroom < psuWattage * 0.15f)
            {
                return new PSUEvaluationResult(
                    PSUStatus.Marginal,
                    psuWattage,
                    continuousHeadroom,
                    peakHeadroom,
                    $"PSU wattage is marginal with low headroom ({continuousHeadroom:F0}W remaining)."
                );
            }

            return new PSUEvaluationResult(
                PSUStatus.Healthy,
                psuWattage,
                continuousHeadroom,
                peakHeadroom,
                "PSU delivers adequate, stable power headroom."
            );
        }
    }
}
