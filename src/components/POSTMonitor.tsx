import React, { useEffect, useState } from 'react';
import {
  Power,
  RotateCcw,
  CheckCircle2,
  AlertTriangle,
  Cpu,
  HardDrive,
  Activity,
  ShieldCheck
} from 'lucide-react';
import { BuildState, FaultCode, POSTState } from '../types';
import { audioSynth } from '../audio/audioSynthesizer';

interface POSTMonitorProps {
  buildState: BuildState;
  onUpdateBuild: (updater: (prev: BuildState) => BuildState) => void;
}

export const POSTMonitor: React.FC<POSTMonitorProps> = ({ buildState, onUpdateBuild }) => {
  const [postStepIndex, setPostStepIndex] = useState(0);

  // Trigger POST sequence when Power Button is pressed
  const handlePowerButton = () => {
    if (buildState.systemPowered) {
      // Turn off
      audioSynth.triggerHaptic('medium');
      onUpdateBuild((b) => ({
        ...b,
        systemPowered: false,
        postState: 'PowerOff',
        faultCode: 'NONE',
        faultMessage: '',
      }));
      return;
    }

    // Turn ON
    audioSynth.playCableConnect();
    onUpdateBuild((b) => ({
      ...b,
      systemPowered: true,
      postState: 'PowerStarting',
      faultCode: 'NONE',
      faultMessage: '',
    }));

    // Step-by-step diagnostic evaluation
    setTimeout(() => {
      // 1. CPU Check
      onUpdateBuild((b) => ({ ...b, postState: 'CPUCheck' }));

      if (!buildState.cpuInstalled) {
        audioSynth.playPostErrorBeeps();
        onUpdateBuild((b) => ({
          ...b,
          postState: 'POSTFailure',
          faultCode: 'CPU_NOT_INSTALLED',
          faultMessage: 'POST Failure: No CPU detected in socket AM5.',
        }));
        return;
      }

      if (!buildState.cableCpuEpsConnected) {
        audioSynth.playPostErrorBeeps();
        onUpdateBuild((b) => ({
          ...b,
          postState: 'POSTFailure',
          faultCode: 'CPU_POWER_MISSING',
          faultMessage: 'POST Failure: CPU EPS 12V 8-Pin power cable is disconnected.',
        }));
        return;
      }

      if (!buildState.coolerInstalled) {
        audioSynth.playPostErrorBeeps();
        onUpdateBuild((b) => ({
          ...b,
          postState: 'POSTFailure',
          faultCode: 'COOLER_NOT_MOUNTED',
          faultMessage: 'Emergency Halt: CPU Cooler is not mounted. Thermal runaway prevented.',
        }));
        return;
      }

      // 2. Memory Check (after 600ms)
      setTimeout(() => {
        onUpdateBuild((b) => ({ ...b, postState: 'MemoryCheck' }));

        if (!buildState.ramInstalled) {
          audioSynth.playPostErrorBeeps();
          onUpdateBuild((b) => ({
            ...b,
            postState: 'POSTFailure',
            faultCode: 'DRAM_NOT_SEATED',
            faultMessage: 'POST Failure: No functional DDR5 memory detected in channel A2.',
          }));
          return;
        }

        // 3. GPU Check (after 600ms)
        setTimeout(() => {
          onUpdateBuild((b) => ({ ...b, postState: 'GPUCheck' }));

          if (buildState.gpuInstalled && !buildState.cablePcieConnected) {
            audioSynth.playPostErrorBeeps();
            onUpdateBuild((b) => ({
              ...b,
              postState: 'POSTFailure',
              faultCode: 'GPU_POWER_MISSING',
              faultMessage: 'POST Failure: VectorX VX-780 requires supplementary PCIe power.',
            }));
            return;
          }

          // 4. Storage & Boot Check (after 500ms)
          setTimeout(() => {
            onUpdateBuild((b) => ({ ...b, postState: 'StorageCheck' }));

            // 5. Final Boot Ready
            setTimeout(() => {
              audioSynth.playPostSuccessBeep();
              onUpdateBuild((b) => ({
                ...b,
                postState: 'BootReady',
                faultCode: 'NONE',
                faultMessage: 'POST Passed. Initializing UEFI BIOS.',
              }));
            }, 600);
          }, 600);
        }, 600);
      }, 600);
    }, 500);
  };

  return (
    <div className="bg-neutral-900 border border-neutral-800 rounded-2xl p-4 flex flex-col gap-3 shadow-xl">
      <div className="flex items-center justify-between border-b border-neutral-800 pb-2.5">
        <div className="flex items-center gap-2">
          <Activity className="w-4 h-4 text-emerald-400" />
          <span className="text-xs font-bold uppercase tracking-wider text-neutral-200">
            Diagnostics & POST Monitor
          </span>
        </div>

        {/* Chassis Power Switch Button */}
        <button
          onClick={handlePowerButton}
          className={`flex items-center gap-2 px-3.5 py-1.5 rounded-xl font-bold text-xs transition-all shadow-md ${
            buildState.systemPowered
              ? 'bg-rose-600 hover:bg-rose-500 text-white'
              : 'bg-emerald-600 hover:bg-emerald-500 text-white animate-pulse'
          }`}
        >
          <Power className="w-3.5 h-3.5" />
          {buildState.systemPowered ? 'Power Switch (OFF)' : 'Power Switch (ON)'}
        </button>
      </div>

      {/* Monitor Display Screen */}
      <div className="w-full bg-black rounded-xl p-4 border border-neutral-800 min-h-[160px] flex flex-col font-mono text-xs text-neutral-300">
        {!buildState.systemPowered ? (
          <div className="m-auto text-center text-neutral-600 flex flex-col items-center gap-1.5">
            <Power className="w-6 h-6 stroke-1" />
            <span>Chassis power is off. Press the power button to start POST.</span>
          </div>
        ) : buildState.postState === 'POSTFailure' ? (
          <div className="space-y-2">
            <div className="flex items-center gap-2 text-rose-500 font-bold border-b border-rose-900/50 pb-1">
              <AlertTriangle className="w-4 h-4" />
              <span>POST ERROR ENCOUNTERED — {buildState.faultCode}</span>
            </div>
            <p className="text-rose-300 leading-relaxed">{buildState.faultMessage}</p>
            <div className="pt-2 text-[11px] text-neutral-500">
              Motherboard Q-LED Status: Check illuminated diagnostic indicator on PCB.
            </div>
          </div>
        ) : buildState.postState === 'BootReady' ? (
          <div className="space-y-1.5 text-neutral-200 animate-fade-in">
            <div className="text-emerald-400 font-bold flex items-center justify-between border-b border-neutral-800 pb-1">
              <span>ApexForge UEFI BIOS Setup v1.02</span>
              <span className="text-[10px] text-neutral-400">AM5 Platform</span>
            </div>
            <div className="text-neutral-400 text-[11px] pt-1">
              CPU: <span className="text-neutral-200">NovaCore N7-7600 8-Core @ 3.80GHz</span>
            </div>
            <div className="text-neutral-400 text-[11px]">
              Memory: <span className="text-neutral-200">32768MB DDR5-6000 (Channel A2 Single)</span>
            </div>
            <div className="text-neutral-400 text-[11px]">
              Graphics: <span className="text-neutral-200">VectorX VX-780 PCIe 5.0 x16 (16GB GDDR6)</span>
            </div>
            <div className="text-neutral-400 text-[11px]">
              Storage: <span className="text-neutral-200">HyperDrive Gen5 2000GB NVMe (M.2_1)</span>
            </div>
            <div className="text-neutral-400 text-[11px] pt-1 border-t border-neutral-800 flex justify-between">
              <span>CPU Temp: 38°C</span>
              <span>VRM: 42°C</span>
              <span className="text-emerald-400 font-bold">STATUS: OK</span>
            </div>
          </div>
        ) : (
          <div className="space-y-2 m-auto text-center">
            <div className="flex items-center justify-center gap-2 text-sky-400 font-semibold animate-pulse">
              <Activity className="w-4 h-4 animate-spin" />
              <span>Running POST Check: {buildState.postState}...</span>
            </div>
            <div className="w-48 h-1.5 bg-neutral-800 rounded-full mx-auto overflow-hidden">
              <div className="h-full bg-sky-500 rounded-full animate-pulse w-3/4" />
            </div>
          </div>
        )}
      </div>

      {/* Motherboard Q-LED indicators simulation */}
      <div className="flex items-center justify-between text-[11px] text-neutral-400 px-2 py-1 bg-neutral-950/60 rounded-lg border border-neutral-800/80">
        <span className="font-semibold text-neutral-500">Q-LED:</span>
        <div className="flex items-center gap-3 font-mono">
          <span
            className={`flex items-center gap-1 ${
              buildState.postState === 'CPUCheck' ||
              (buildState.postState === 'POSTFailure' &&
                (buildState.faultCode === 'CPU_NOT_INSTALLED' || buildState.faultCode === 'CPU_POWER_MISSING'))
                ? 'text-rose-400 font-bold'
                : 'text-neutral-600'
            }`}
          >
            ● CPU
          </span>
          <span
            className={`flex items-center gap-1 ${
              buildState.postState === 'MemoryCheck' ||
              (buildState.postState === 'POSTFailure' && buildState.faultCode === 'DRAM_NOT_SEATED')
                ? 'text-amber-400 font-bold'
                : 'text-neutral-600'
            }`}
          >
            ● DRAM
          </span>
          <span
            className={`flex items-center gap-1 ${
              buildState.postState === 'GPUCheck' ||
              (buildState.postState === 'POSTFailure' && buildState.faultCode === 'GPU_POWER_MISSING')
                ? 'text-neutral-100 font-bold'
                : 'text-neutral-600'
            }`}
          >
            ● VGA
          </span>
          <span
            className={`flex items-center gap-1 ${
              buildState.postState === 'BootReady' ? 'text-emerald-400 font-bold' : 'text-neutral-600'
            }`}
          >
            ● BOOT
          </span>
        </div>
      </div>
    </div>
  );
};
