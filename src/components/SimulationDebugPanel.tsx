import React from 'react';
import { Zap, Flame, Cpu, ShieldCheck, AlertCircle, Wrench } from 'lucide-react';
import { BuildState } from '../types';
import { SimulationEngineTS } from '../simulation/simulationEngine';

interface SimulationDebugPanelProps {
  buildState: BuildState;
}

export const SimulationDebugPanel: React.FC<SimulationDebugPanelProps> = ({ buildState }) => {
  const evalResult = SimulationEngineTS.evaluateBuild(buildState);
  const { load, psuResult, thermals, faults, canPost } = evalResult;

  return (
    <div className="bg-neutral-900/95 border border-neutral-800 rounded-2xl p-5 shadow-2xl space-y-5 text-xs text-neutral-300">
      <div className="flex items-center justify-between border-b border-neutral-800 pb-3">
        <div className="flex items-center gap-2">
          <span className="w-2 h-2 rounded-full bg-amber-400 animate-pulse" />
          <h3 className="text-sm font-bold text-neutral-100 uppercase tracking-wide">
            Development Simulation Inspector (Stage 2)
          </h3>
        </div>
        <span className="text-[11px] font-mono text-neutral-500">
          Deterministic Pure C# Simulator Mirror
        </span>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        {/* =========================================
            POWER INSPECTION BLOCK (Section 26)
            ========================================= */}
        <div className="bg-neutral-950 p-4 rounded-xl border border-neutral-800/80 space-y-2.5">
          <div className="flex items-center justify-between text-neutral-200 font-semibold border-b border-neutral-800/60 pb-1.5">
            <span className="flex items-center gap-1.5 text-amber-400">
              <Zap className="w-4 h-4" /> POWER
            </span>
            <span
              className={`px-2 py-0.5 rounded text-[10px] font-mono font-bold ${
                psuResult.status === 'Healthy'
                  ? 'bg-emerald-950 text-emerald-400 border border-emerald-800/50'
                  : psuResult.status === 'Marginal'
                  ? 'bg-amber-950 text-amber-400 border border-amber-800/50'
                  : 'bg-rose-950 text-rose-400 border border-rose-800/50'
              }`}
            >
              {psuResult.status}
            </span>
          </div>

          <div className="space-y-1.5 font-mono text-[11px]">
            <div className="flex justify-between">
              <span className="text-neutral-500">PSU Rated:</span>
              <span className="text-neutral-200">{psuResult.ratedWattage}W (VoltEdge Bronze)</span>
            </div>
            <div className="flex justify-between">
              <span className="text-neutral-500">Continuous Load:</span>
              <span className="text-neutral-200">{load.continuousLoadWatts.toFixed(0)}W</span>
            </div>
            <div className="flex justify-between">
              <span className="text-neutral-500">Peak Load:</span>
              <span className="text-neutral-200">{load.peakLoadWatts.toFixed(0)}W</span>
            </div>
            <div className="flex justify-between">
              <span className="text-neutral-500">Transient Load:</span>
              <span className="text-neutral-200">{load.transientLoadWatts.toFixed(0)}W</span>
            </div>
            <div className="flex justify-between pt-1 border-t border-neutral-900">
              <span className="text-neutral-500">Headroom:</span>
              <span
                className={
                  psuResult.continuousHeadroomWatts > 100
                    ? 'text-emerald-400'
                    : 'text-amber-400'
                }
              >
                +{psuResult.continuousHeadroomWatts.toFixed(0)}W
              </span>
            </div>
          </div>

          {/* Rail Connection Indicators */}
          <div className="pt-2 border-t border-neutral-900 space-y-1 text-[10px]">
            <div className="flex items-center justify-between">
              <span className="text-neutral-500">24-Pin ATX Rail:</span>
              <span className={buildState.cable24PinConnected ? 'text-emerald-400' : 'text-neutral-600'}>
                {buildState.cable24PinConnected ? 'CONNECTED' : 'DISCONNECTED'}
              </span>
            </div>
            <div className="flex items-center justify-between">
              <span className="text-neutral-500">CPU EPS 8-Pin Rail:</span>
              <span className={buildState.cableCpuEpsConnected ? 'text-emerald-400' : 'text-neutral-600'}>
                {buildState.cableCpuEpsConnected ? 'CONNECTED' : 'DISCONNECTED'}
              </span>
            </div>
            <div className="flex items-center justify-between">
              <span className="text-neutral-500">GPU PCIe 8-Pin Rail:</span>
              <span className={buildState.cablePcieConnected ? 'text-emerald-400' : 'text-neutral-600'}>
                {buildState.cablePcieConnected ? 'CONNECTED' : 'DISCONNECTED'}
              </span>
            </div>
          </div>
        </div>

        {/* =========================================
            THERMAL INSPECTION BLOCK (Section 26)
            ========================================= */}
        <div className="bg-neutral-950 p-4 rounded-xl border border-neutral-800/80 space-y-2.5">
          <div className="flex items-center justify-between text-neutral-200 font-semibold border-b border-neutral-800/60 pb-1.5">
            <span className="flex items-center gap-1.5 text-rose-400">
              <Flame className="w-4 h-4" /> THERMAL
            </span>
            <span
              className={`px-2 py-0.5 rounded text-[10px] font-mono font-bold ${
                thermals.state === 'Normal' || thermals.state === 'Warm'
                  ? 'bg-emerald-950 text-emerald-400 border border-emerald-800/50'
                  : thermals.state === 'Throttling'
                  ? 'bg-amber-950 text-amber-400 border border-amber-800/50'
                  : 'bg-rose-950 text-rose-400 border border-rose-800/50'
              }`}
            >
              {thermals.state}
            </span>
          </div>

          <div className="space-y-1.5 font-mono text-[11px]">
            <div className="flex justify-between">
              <span className="text-neutral-500">Ambient Temp:</span>
              <span className="text-neutral-200">22.0°C</span>
            </div>
            <div className="flex justify-between">
              <span className="text-neutral-500">CPU Power:</span>
              <span className="text-neutral-200">105W (NovaCore N7)</span>
            </div>
            <div className="flex justify-between">
              <span className="text-neutral-500">Cooler Capacity:</span>
              <span className="text-neutral-200">
                {buildState.coolerInstalled ? '180W (ArcticBreeze)' : '0W (None)'}
              </span>
            </div>
            <div className="flex justify-between">
              <span className="text-neutral-500">Paste Quality:</span>
              <span className="text-neutral-200">
                {(thermals.pasteEfficiency * 100).toFixed(0)}% (amount {buildState.thermalPasteQuality.toFixed(2)})
              </span>
            </div>
            <div className="flex justify-between">
              <span className="text-neutral-500">Mount Quality:</span>
              <span className="text-neutral-200">
                {(thermals.mountQuality * 100).toFixed(0)}% ({buildState.coolerScrewsTightened}/4 screws)
              </span>
            </div>
            <div className="flex justify-between pt-1 border-t border-neutral-900 font-bold">
              <span className="text-neutral-400">CPU Temperature:</span>
              <span
                className={
                  thermals.temperatureCelsius < 70
                    ? 'text-emerald-400'
                    : thermals.temperatureCelsius < 88
                    ? 'text-amber-400'
                    : 'text-rose-400'
                }
              >
                {thermals.temperatureCelsius.toFixed(1)}°C
              </span>
            </div>
            <div className="flex justify-between text-[10px]">
              <span className="text-neutral-500">Clock Multiplier:</span>
              <span className="text-sky-300">
                {Math.round(thermals.performanceMultiplier * 100)}%
              </span>
            </div>
          </div>
        </div>

        {/* =========================================
            POST & HARDWARE EVALUATION (Section 26)
            ========================================= */}
        <div className="bg-neutral-950 p-4 rounded-xl border border-neutral-800/80 space-y-2.5">
          <div className="flex items-center justify-between text-neutral-200 font-semibold border-b border-neutral-800/60 pb-1.5">
            <span className="flex items-center gap-1.5 text-sky-400">
              <Cpu className="w-4 h-4" /> POST STATUS
            </span>
            <span
              className={`px-2 py-0.5 rounded text-[10px] font-mono font-bold ${
                canPost
                  ? 'bg-emerald-950 text-emerald-400 border border-emerald-800/50'
                  : 'bg-rose-950 text-rose-400 border border-rose-800/50'
              }`}
            >
              {canPost ? 'BOOT READY' : 'HALT'}
            </span>
          </div>

          <div className="space-y-1.5 font-mono text-[11px]">
            <div className="flex justify-between">
              <span className="text-neutral-500">CPU:</span>
              <span className={buildState.cpuInstalled ? 'text-emerald-400' : 'text-rose-400'}>
                {buildState.cpuInstalled ? 'OK (AM5 Seated)' : 'MISSING'}
              </span>
            </div>
            <div className="flex justify-between">
              <span className="text-neutral-500">RAM:</span>
              <span className={buildState.ramInstalled ? 'text-emerald-400' : 'text-rose-400'}>
                {buildState.ramInstalled ? 'OK (DDR5-6000)' : 'MISSING'}
              </span>
            </div>
            <div className="flex justify-between">
              <span className="text-neutral-500">GPU:</span>
              <span className={buildState.gpuInstalled ? 'text-emerald-400' : 'text-neutral-500'}>
                {buildState.gpuInstalled ? 'OK (PCIe 5.0)' : 'NONE (iGPU)'}
              </span>
            </div>
            <div className="flex justify-between">
              <span className="text-neutral-500">Storage:</span>
              <span className={buildState.m2Installed ? 'text-emerald-400' : 'text-neutral-500'}>
                {buildState.m2Installed ? 'OK (NVMe 2TB)' : 'EMPTY'}
              </span>
            </div>
            <div className="flex justify-between">
              <span className="text-neutral-500">Power Rails:</span>
              <span
                className={
                  buildState.cable24PinConnected && buildState.cableCpuEpsConnected
                    ? 'text-emerald-400'
                    : 'text-rose-400'
                }
              >
                {buildState.cable24PinConnected && buildState.cableCpuEpsConnected ? 'VALID' : 'FAULT'}
              </span>
            </div>
            <div className="flex justify-between pt-1 border-t border-neutral-900 font-bold">
              <span className="text-neutral-400">Final Result:</span>
              <span className={canPost ? 'text-emerald-400' : 'text-rose-400'}>
                {canPost ? 'POST SUCCESS' : `${faults.length} FAULT(S)`}
              </span>
            </div>
          </div>
        </div>
      </div>

      {/* Active Fault List */}
      {faults.length > 0 && (
        <div className="p-3 bg-rose-950/40 rounded-xl border border-rose-900/60 space-y-1.5">
          <div className="flex items-center gap-1.5 font-bold text-rose-400 text-xs">
            <AlertCircle className="w-3.5 h-3.5" />
            <span>Active Simulation Diagnostic Faults:</span>
          </div>
          <div className="space-y-1 text-[11px]">
            {faults.map((f, i) => (
              <div key={i} className="flex items-center justify-between text-rose-300">
                <span>
                  • [{f.subsystem}] {f.faultCode}: {f.message}
                </span>
                <span className="text-neutral-500 font-mono text-[10px]">
                  Target: {f.suggestedInspectionTarget}
                </span>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
};
