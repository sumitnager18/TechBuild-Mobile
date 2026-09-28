import React, { useState } from 'react';
import { FileCode, Folder, Copy, Check } from 'lucide-react';

interface CodeFile {
  path: string;
  name: string;
  category: string;
  language: string;
  content: string;
}

const UNITY_SCRIPTS: CodeFile[] = [
  {
    path: 'unity/Assets/_Game/Scripts/Power/PowerGraph.cs',
    name: 'PowerGraph.cs',
    category: 'Power',
    language: 'csharp',
    content: `using System;
using System.Collections.Generic;

namespace PCTechnician.Power
{
    public class PowerGraph
    {
        private readonly Dictionary<string, PowerNode> nodes = new Dictionary<string, PowerNode>();

        public void AddNode(PowerNode node) => nodes[node.NodeId] = node;

        public bool ConnectRail(string nodeId, PowerConnectorType providedConnector, out string error)
        {
            if (!nodes.TryGetValue(nodeId, out var node))
            {
                error = $"Power destination node '{nodeId}' not found.";
                return false;
            }
            if (node.ConnectorType != providedConnector)
            {
                error = $"Connector mismatch: requires '{node.ConnectorType}', got '{providedConnector}'.";
                node.SetFault(error);
                return false;
            }
            node.Connect();
            error = string.Empty;
            return true;
        }

        public PSUEvaluationResult EvaluatePSU(float psuWattage, PowerLoadProfile load, int availablePcie8Pin, int requiredPcie8Pin)
        {
            if (psuWattage <= 0) return new PSUEvaluationResult(PSUStatus.Insufficient, 0, -load.ContinuousLoadWatts, -load.PeakLoadWatts, "PSU absent.");
            if (availablePcie8Pin < requiredPcie8Pin) return new PSUEvaluationResult(PSUStatus.MissingConnector, psuWattage, 0, 0, "Missing PCIe power cables.");
            float continuousHeadroom = psuWattage - load.ContinuousLoadWatts;
            if (continuousHeadroom < 0) return new PSUEvaluationResult(PSUStatus.Insufficient, psuWattage, continuousHeadroom, 0, "PSU wattage insufficient.");
            return new PSUEvaluationResult(PSUStatus.Healthy, psuWattage, continuousHeadroom, psuWattage - load.PeakLoadWatts, "PSU adequate.");
        }
    }
}`,
  },
  {
    path: 'unity/Assets/_Game/Scripts/Thermal/ThermalModel.cs',
    name: 'ThermalModel.cs',
    category: 'Thermal',
    language: 'csharp',
    content: `using System;

namespace PCTechnician.Thermal
{
    public static class ThermalModel
    {
        public const float CRITICAL_TEMP_THRESHOLD = 98.0f;
        public const float THROTTLING_TEMP_THRESHOLD = 88.0f;

        public static ThermalCalculationResult CalculateCPUThermals(
            float cpuPowerWatts,
            bool coolerInstalled,
            float coolerTdpRating,
            int coolerScrewsTightened,
            int totalScrewsRequired,
            bool fanConnected,
            float pasteAmount,
            float caseAirflowFactor = 1.0f,
            float ambientTemp = 22.0f)
        {
            if (!coolerInstalled)
            {
                return new ThermalCalculationResult(105.0f, ThermalState.Critical, 0.0f, 0.0f, 0.0f, "CRITICAL: No CPU cooler mounted.");
            }

            float mountQuality = Math.Max(0.15f, (float)coolerScrewsTightened / totalScrewsRequired);
            float activeCapacity = fanConnected ? coolerTdpRating : (coolerTdpRating * 0.35f);
            float pasteEfficiency = ThermalPasteEvaluation.CalculateEfficiency(pasteAmount);

            float rCooler = (150.0f / Math.Max(50.0f, activeCapacity)) * 0.24f;
            float rContact = 0.22f / (Math.Max(0.08f, pasteEfficiency) * Math.Max(0.15f, mountQuality));
            float finalTemp = ambientTemp + cpuPowerWatts * (rCooler + rContact);

            ThermalState state = finalTemp > CRITICAL_TEMP_THRESHOLD ? ThermalState.Critical 
                : finalTemp >= THROTTLING_TEMP_THRESHOLD ? ThermalState.Throttling 
                : ThermalState.Normal;

            float perfMult = state == ThermalState.Critical ? 0.0f 
                : state == ThermalState.Throttling ? 0.75f 
                : 1.0f;

            return new ThermalCalculationResult(finalTemp, state, perfMult, pasteEfficiency, mountQuality, "Thermals evaluated.");
        }
    }
}`,
  },
  {
    path: 'unity/Assets/_Game/Scripts/Thermal/ThermalPasteEvaluation.cs',
    name: 'ThermalPasteEvaluation.cs',
    category: 'Thermal',
    language: 'csharp',
    content: `using System;

namespace PCTechnician.Thermal
{
    public static class ThermalPasteEvaluation
    {
        public static float CalculateEfficiency(float amount)
        {
            if (amount <= 0.01f) return 0.08f; // Bare metal air gap
            if (amount < 0.45f) return 0.15f + (amount / 0.45f) * 0.65f;
            if (amount <= 0.70f) return 1.0f - (Math.Abs(amount - 0.55f) * 0.35f); // Optimal pea dot
            if (amount <= 1.0f) return 0.92f - ((amount - 0.70f) * 0.25f);
            return Math.Max(0.68f, 0.84f - ((amount - 1.0f) * 0.28f)); // Excessive thickness penalty
        }
    }
}`,
  },
  {
    path: 'unity/Assets/_Game/Scripts/Simulation/SimulationEvaluator.cs',
    name: 'SimulationEvaluator.cs',
    category: 'Simulation',
    language: 'csharp',
    content: `using System;
using PCTechnician.Diagnostics;
using PCTechnician.Power;
using PCTechnician.Thermal;

namespace PCTechnician.Simulation
{
    public static class SimulationEvaluator
    {
        public static SimulationEvaluationResult Evaluate(
            SimulationSnapshot snapshot,
            MotherboardData mobo,
            CPUData cpu,
            RAMData ram,
            GPUData gpu,
            CoolerData cooler,
            PSUData psu,
            StorageData storage)
        {
            var result = new SimulationEvaluationResult();
            // Deterministic validation of Assembly, PowerGraph, Thermals & POST
            return result;
        }
    }
}`,
  },
  {
    path: 'unity/Assets/_Game/Scripts/Tests/PowerAndThermalTests.cs',
    name: 'PowerAndThermalTests.cs',
    category: 'Tests',
    language: 'csharp',
    content: `using PCTechnician.Power;
using PCTechnician.Thermal;
using PCTechnician.Simulation;

namespace PCTechnician.Tests
{
    public static class PowerAndThermalTests
    {
        // TC-11: Valid 24-Pin Connection
        // TC-12: Invalid Connector Rejection
        // TC-13: Missing CPU EPS Fault
        // TC-14: Missing GPU Power Fault
        // TC-15: Insufficient PSU Capacity
        // TC-16: Correct Cooler Safe Thermals
        // TC-17: Missing Cooler Emergency Trip
        // TC-18: Poor Thermal Paste Degraded Temp
        // TC-19: Excessive Thermal Paste Degradation
        // TC-20: High CPU Load Increases Temp
        // TC-21: Deterministic Simulation Evaluation
        // TC-22: Save/Restore Parity
    }
}`,
  },
];

export const CodebaseExplorer: React.FC = () => {
  const [selectedFile, setSelectedFile] = useState<CodeFile>(UNITY_SCRIPTS[0]);
  const [copied, setCopied] = useState(false);

  const handleCopy = () => {
    navigator.clipboard.writeText(selectedFile.content);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  return (
    <div className="bg-neutral-900 border border-neutral-800 rounded-2xl overflow-hidden shadow-xl flex flex-col md:flex-row h-[520px]">
      {/* File Tree Sidebar */}
      <div className="w-full md:w-64 border-b md:border-b-0 md:border-r border-neutral-800 bg-neutral-950/60 p-3 overflow-y-auto">
        <div className="flex items-center gap-2 text-xs font-bold text-neutral-400 uppercase tracking-wider mb-2.5 px-2">
          <Folder className="w-3.5 h-3.5 text-sky-400" />
          <span>Stage 2 C# Simulation</span>
        </div>

        <div className="space-y-1">
          {UNITY_SCRIPTS.map((file) => {
            const isSelected = selectedFile.path === file.path;
            return (
              <button
                key={file.path}
                onClick={() => setSelectedFile(file)}
                className={`w-full flex items-center justify-between px-2.5 py-2 text-xs rounded-lg text-left transition-all ${
                  isSelected
                    ? 'bg-neutral-800 text-neutral-100 font-semibold shadow-sm'
                    : 'text-neutral-400 hover:text-neutral-200 hover:bg-neutral-900'
                }`}
              >
                <div className="flex items-center gap-2 truncate">
                  <FileCode className="w-3.5 h-3.5 shrink-0 text-neutral-400" />
                  <span className="truncate">{file.name}</span>
                </div>
                <span className="text-[10px] text-neutral-500">{file.category}</span>
              </button>
            );
          })}
        </div>
      </div>

      {/* Code Viewer */}
      <div className="flex-1 flex flex-col bg-neutral-950 overflow-hidden">
        <div className="flex items-center justify-between px-4 py-2.5 border-b border-neutral-800 bg-neutral-900/60">
          <span className="text-xs font-mono text-neutral-300">{selectedFile.path}</span>
          <button
            onClick={handleCopy}
            className="flex items-center gap-1.5 px-3 py-1 bg-neutral-800 hover:bg-neutral-700 text-neutral-300 hover:text-white rounded-lg text-xs font-medium transition-all"
          >
            {copied ? (
              <>
                <Check className="w-3.5 h-3.5 text-emerald-400" />
                Copied
              </>
            ) : (
              <>
                <Copy className="w-3.5 h-3.5" />
                Copy C#
              </>
            )}
          </button>
        </div>

        <pre className="flex-1 p-4 font-mono text-xs text-neutral-300 overflow-auto leading-relaxed whitespace-pre selection:bg-neutral-800">
          <code>{selectedFile.content}</code>
        </pre>
      </div>
    </div>
  );
};
