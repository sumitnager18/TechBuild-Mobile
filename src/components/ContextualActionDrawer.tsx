import React, { useState } from 'react';
import {
  Wrench,
  CheckCircle2,
  AlertTriangle,
  RotateCcw,
  Zap,
  Layers,
  Sparkles,
  Maximize2
} from 'lucide-react';
import { BuildState, FocusZoneType } from '../types';
import { audioSynth } from '../audio/audioSynthesizer';

interface ContextualActionDrawerProps {
  currentZone: FocusZoneType;
  buildState: BuildState;
  onUpdateBuild: (updater: (prev: BuildState) => BuildState) => void;
  onZoneSelect: (zone: FocusZoneType) => void;
}

export const ContextualActionDrawer: React.FC<ContextualActionDrawerProps> = ({
  currentZone,
  buildState,
  onUpdateBuild,
  onZoneSelect,
}) => {
  const [holdingAction, setHoldingAction] = useState<string | null>(null);
  const [holdProgress, setHoldProgress] = useState(0);

  // Thermal paste mini-game size accumulator
  const [pasteHolding, setPasteHolding] = useState(false);
  const [pasteAmount, setPasteAmount] = useState(0.5);

  const startHold = (actionId: string, durationMs: number, onComplete: () => void) => {
    setHoldingAction(actionId);
    setHoldProgress(0);
    const start = performance.now();
    const interval = window.setInterval(() => {
      const elapsed = performance.now() - start;
      const progress = Math.min(1, elapsed / durationMs);
      setHoldProgress(progress);
      if (progress >= 1) {
        clearInterval(interval);
        setHoldingAction(null);
        setHoldProgress(0);
        onComplete();
      }
    }, 20);

    const cleanup = () => {
      clearInterval(interval);
      setHoldingAction(null);
      setHoldProgress(0);
      window.removeEventListener('pointerup', cleanup);
      window.removeEventListener('pointercancel', cleanup);
    };

    window.addEventListener('pointerup', cleanup, { once: true });
    window.addEventListener('pointercancel', cleanup, { once: true });
  };

  const renderActions = () => {
    switch (currentZone) {
      case 'Workshop':
      case 'Assembly':
        return (
          <div className="flex flex-wrap items-center gap-3">
            {!buildState.sidePanelRemoved ? (
              <button
                onClick={() => {
                  audioSynth.playLatchClick();
                  onUpdateBuild((b) => ({ ...b, sidePanelRemoved: true, caseClosed: false }));
                  onZoneSelect('Assembly');
                }}
                className="flex items-center gap-2 px-4 py-2.5 bg-sky-600 hover:bg-sky-500 text-white text-sm font-semibold rounded-xl shadow-lg transition-all"
              >
                <Layers className="w-4 h-4" />
                Remove Tempered Glass Panel
              </button>
            ) : (
              <button
                onClick={() => {
                  audioSynth.playLatchClick();
                  onUpdateBuild((b) => ({ ...b, sidePanelRemoved: false, caseClosed: true }));
                  onZoneSelect('Workshop');
                }}
                className="flex items-center gap-2 px-4 py-2.5 bg-neutral-800 hover:bg-neutral-700 text-neutral-200 text-sm font-semibold rounded-xl border border-neutral-700 shadow-md transition-all"
              >
                <Layers className="w-4 h-4" />
                Re-attach Glass Side Panel
              </button>
            )}

            <button
              onClick={() => onZoneSelect('CPUSocket')}
              className="px-3.5 py-2 bg-neutral-800/80 hover:bg-neutral-700 text-neutral-200 text-xs font-medium rounded-lg border border-neutral-700"
            >
              Focus CPU Socket
            </button>
            <button
              onClick={() => onZoneSelect('RAMSlots')}
              className="px-3.5 py-2 bg-neutral-800/80 hover:bg-neutral-700 text-neutral-200 text-xs font-medium rounded-lg border border-neutral-700"
            >
              Focus RAM Slots
            </button>
            <button
              onClick={() => onZoneSelect('PCIeSlot')}
              className="px-3.5 py-2 bg-neutral-800/80 hover:bg-neutral-700 text-neutral-200 text-xs font-medium rounded-lg border border-neutral-700"
            >
              Focus PCIe Slot
            </button>
            <button
              onClick={() => onZoneSelect('PowerArea')}
              className="px-3.5 py-2 bg-neutral-800/80 hover:bg-neutral-700 text-neutral-200 text-xs font-medium rounded-lg border border-neutral-700"
            >
              Focus Power Area
            </button>
          </div>
        );

      case 'CPUSocket':
        return (
          <div className="flex flex-wrap items-center gap-3">
            {/* Latch toggle */}
            {!buildState.cpuInstalled ? (
              <button
                onClick={() => {
                  audioSynth.playLatchClick();
                  onUpdateBuild((b) => ({ ...b, cpuSocketOpen: !b.cpuSocketOpen }));
                }}
                className={`flex items-center gap-2 px-4 py-2 text-xs font-semibold rounded-xl transition-all ${
                  buildState.cpuSocketOpen
                    ? 'bg-amber-600/90 text-white'
                    : 'bg-neutral-800 text-neutral-200 border border-neutral-700'
                }`}
              >
                {buildState.cpuSocketOpen ? 'Retention Arm is Open' : 'Open Socket Retention Arm'}
              </button>
            ) : null}

            {/* Install CPU */}
            {!buildState.cpuInstalled && (
              <button
                disabled={!buildState.cpuSocketOpen}
                onClick={() => {
                  if (!buildState.cpuSocketOpen) return;
                  audioSynth.playLatchClick();
                  onUpdateBuild((b) => ({
                    ...b,
                    cpuInstalled: true,
                    cpuAlignedCorrectly: true,
                    cpuSocketOpen: false, // snaps shut
                  }));
                }}
                className={`flex items-center gap-2 px-4 py-2 text-xs font-semibold rounded-xl transition-all ${
                  buildState.cpuSocketOpen
                    ? 'bg-emerald-600 hover:bg-emerald-500 text-white shadow-md'
                    : 'bg-neutral-800/50 text-neutral-500 cursor-not-allowed border border-neutral-800'
                }`}
              >
                <CheckCircle2 className="w-4 h-4" />
                Align & Insert NovaCore N7-7600
              </button>
            )}

            {/* Thermal Paste Mini-game */}
            {buildState.cpuInstalled && !buildState.coolerInstalled && (
              <div className="flex items-center gap-2 bg-neutral-800/90 px-3 py-1.5 rounded-xl border border-neutral-700">
                <span className="text-xs text-neutral-300 font-medium">Syringe (Pea):</span>
                <button
                  onPointerDown={() => {
                    setPasteHolding(true);
                    audioSynth.triggerHaptic('light');
                  }}
                  onPointerUp={() => {
                    if (pasteHolding) {
                      setPasteHolding(false);
                      audioSynth.playCableConnect();
                      onUpdateBuild((b) => ({
                        ...b,
                        thermalPasteApplied: true,
                        thermalPasteQuality: pasteAmount,
                      }));
                    }
                  }}
                  className={`px-3 py-1 text-xs font-semibold rounded-lg transition-all ${
                    buildState.thermalPasteApplied
                      ? 'bg-emerald-700/80 text-white'
                      : 'bg-sky-600 hover:bg-sky-500 text-white shadow-sm'
                  }`}
                >
                  {buildState.thermalPasteApplied
                    ? `Paste Applied (${Math.round(buildState.thermalPasteQuality * 100)}% Pea)`
                    : 'Hold to Dispense Pea Dot'}
                </button>
                {buildState.thermalPasteApplied && (
                  <button
                    onClick={() => {
                      audioSynth.triggerHaptic('light');
                      onUpdateBuild((b) => ({
                        ...b,
                        thermalPasteApplied: false,
                        thermalPasteQuality: 0,
                      }));
                    }}
                    className="text-neutral-400 hover:text-rose-400 text-xs"
                    title="Clean Paste"
                  >
                    <RotateCcw className="w-3.5 h-3.5" />
                  </button>
                )}
              </div>
            )}

            {/* Install Air Cooler */}
            {buildState.cpuInstalled && (
              <button
                disabled={buildState.coolerInstalled}
                onClick={() => {
                  audioSynth.playScrewTurn();
                  onUpdateBuild((b) => ({
                    ...b,
                    coolerInstalled: true,
                    coolerScrewsTightened: 4,
                    coolerFanConnected: true,
                  }));
                }}
                className={`flex items-center gap-2 px-4 py-2 text-xs font-semibold rounded-xl transition-all ${
                  !buildState.coolerInstalled
                    ? 'bg-sky-600 hover:bg-sky-500 text-white shadow-md'
                    : 'bg-neutral-800 text-neutral-400 border border-neutral-700'
                }`}
              >
                <Wrench className="w-4 h-4" />
                {buildState.coolerInstalled ? 'Cooler Secured (4/4 Screws)' : 'Mount ArcticBreeze A-4 Cooler'}
              </button>
            )}

            {buildState.cpuInstalled && buildState.coolerInstalled && (
              <button
                onClick={() => onZoneSelect('RAMSlots')}
                className="px-3 py-1.5 bg-neutral-700 hover:bg-neutral-600 text-white text-xs font-semibold rounded-lg ml-auto"
              >
                Next: RAM Slots →
              </button>
            )}
          </div>
        );

      case 'RAMSlots':
        return (
          <div className="flex flex-wrap items-center gap-3">
            {!buildState.ramInstalled ? (
              <>
                <button
                  onClick={() => {
                    audioSynth.playLatchClick();
                    onUpdateBuild((b) => ({ ...b, ramLatchOpen: !b.ramLatchOpen }));
                  }}
                  className="px-3.5 py-2 bg-neutral-800 text-neutral-200 border border-neutral-700 text-xs font-medium rounded-xl"
                >
                  {buildState.ramLatchOpen ? 'DIMM Slot 2 Latch: OPEN' : 'Open DIMM Slot 2 Latch'}
                </button>

                <button
                  disabled={!buildState.ramLatchOpen}
                  onClick={() => {
                    audioSynth.playRamSnap();
                    onUpdateBuild((b) => ({
                      ...b,
                      ramInstalled: true,
                      ramLatchOpen: false,
                    }));
                  }}
                  className={`flex items-center gap-2 px-4 py-2 text-xs font-semibold rounded-xl transition-all ${
                    buildState.ramLatchOpen
                      ? 'bg-emerald-600 hover:bg-emerald-500 text-white shadow-md'
                      : 'bg-neutral-800/50 text-neutral-500 cursor-not-allowed border border-neutral-800'
                  }`}
                >
                  <CheckCircle2 className="w-4 h-4" />
                  Align Notch & Snap TitanRAM DDR5-32
                </button>
              </>
            ) : (
              <div className="flex items-center gap-2 text-xs text-emerald-400 font-medium bg-emerald-950/40 border border-emerald-800/60 px-3 py-2 rounded-xl">
                <CheckCircle2 className="w-4 h-4" />
                TitanRAM DDR5-32 Seated in Channel A2
                <button
                  onClick={() => {
                    audioSynth.playLatchClick();
                    onUpdateBuild((b) => ({ ...b, ramInstalled: false, ramLatchOpen: true }));
                  }}
                  className="ml-3 text-neutral-400 hover:text-rose-400 text-xs underline"
                >
                  Remove
                </button>
              </div>
            )}

            <button
              onClick={() => onZoneSelect('PCIeSlot')}
              className="px-3 py-1.5 bg-neutral-700 hover:bg-neutral-600 text-white text-xs font-semibold rounded-lg ml-auto"
            >
              Next: GPU Slot →
            </button>
          </div>
        );

      case 'PCIeSlot':
        return (
          <div className="flex flex-wrap items-center gap-3">
            {!buildState.gpuInstalled ? (
              <button
                onClick={() => {
                  audioSynth.playGpuSnap();
                  onUpdateBuild((b) => ({
                    ...b,
                    gpuBracketRemoved: true,
                    gpuPcieLatchOpen: false,
                    gpuInstalled: true,
                  }));
                }}
                className="flex items-center gap-2 px-4 py-2 bg-emerald-600 hover:bg-emerald-500 text-white text-xs font-semibold rounded-xl shadow-md transition-all"
              >
                <CheckCircle2 className="w-4 h-4" />
                Align & Seat VectorX VX-780 GPU
              </button>
            ) : (
              <div className="flex items-center gap-2 text-xs text-emerald-400 font-medium bg-emerald-950/40 border border-emerald-800/60 px-3 py-2 rounded-xl">
                <CheckCircle2 className="w-4 h-4" />
                VectorX VX-780 PCIe 5.0 x16 Latched
                <button
                  onClick={() => {
                    audioSynth.playLatchClick();
                    onUpdateBuild((b) => ({ ...b, gpuInstalled: false }));
                  }}
                  className="ml-3 text-neutral-400 hover:text-rose-400 text-xs underline"
                >
                  Remove GPU
                </button>
              </div>
            )}

            <button
              onClick={() => onZoneSelect('M2Slot')}
              className="px-3 py-1.5 bg-neutral-800 hover:bg-neutral-700 text-neutral-200 text-xs font-medium rounded-lg border border-neutral-700"
            >
              Inspect M.2 Slot
            </button>

            <button
              onClick={() => onZoneSelect('PowerArea')}
              className="px-3 py-1.5 bg-neutral-700 hover:bg-neutral-600 text-white text-xs font-semibold rounded-lg ml-auto"
            >
              Next: Power Cables →
            </button>
          </div>
        );

      case 'M2Slot':
        return (
          <div className="flex flex-wrap items-center gap-3">
            {!buildState.m2Installed ? (
              <button
                onClick={() => {
                  audioSynth.playScrewTurn();
                  onUpdateBuild((b) => ({
                    ...b,
                    m2ScrewRemoved: true,
                    m2Installed: true,
                    m2Screwed: true,
                  }));
                }}
                className="flex items-center gap-2 px-4 py-2 bg-emerald-600 hover:bg-emerald-500 text-white text-xs font-semibold rounded-xl shadow-md transition-all"
              >
                <CheckCircle2 className="w-4 h-4" />
                Insert HyperDrive Gen5 M.2 SSD & Fasten Standoff Screw
              </button>
            ) : (
              <div className="flex items-center gap-2 text-xs text-emerald-400 font-medium bg-emerald-950/40 border border-emerald-800/60 px-3 py-2 rounded-xl">
                <CheckCircle2 className="w-4 h-4" />
                HyperDrive 2TB NVMe Fastened
              </div>
            )}
            <button
              onClick={() => onZoneSelect('Assembly')}
              className="px-3 py-1.5 bg-neutral-700 hover:bg-neutral-600 text-white text-xs font-semibold rounded-lg ml-auto"
            >
              Back to Assembly
            </button>
          </div>
        );

      case 'PowerArea':
      case 'PSUBasement':
        return (
          <div className="flex flex-wrap items-center gap-3">
            {/* 24-Pin ATX */}
            <button
              onClick={() => {
                audioSynth.playCableConnect();
                onUpdateBuild((b) => ({ ...b, cable24PinConnected: !b.cable24PinConnected }));
              }}
              className={`flex items-center gap-2 px-3.5 py-2 text-xs font-semibold rounded-xl transition-all ${
                buildState.cable24PinConnected
                  ? 'bg-emerald-700/80 text-white'
                  : 'bg-neutral-800 text-neutral-300 hover:bg-neutral-700 border border-neutral-700'
              }`}
            >
              <Zap className="w-3.5 h-3.5" />
              {buildState.cable24PinConnected ? '24-Pin ATX: CONNECTED' : 'Route 24-Pin ATX Cable'}
            </button>

            {/* CPU EPS 8-Pin */}
            <button
              onClick={() => {
                audioSynth.playCableConnect();
                onUpdateBuild((b) => ({ ...b, cableCpuEpsConnected: !b.cableCpuEpsConnected }));
              }}
              className={`flex items-center gap-2 px-3.5 py-2 text-xs font-semibold rounded-xl transition-all ${
                buildState.cableCpuEpsConnected
                  ? 'bg-emerald-700/80 text-white'
                  : 'bg-neutral-800 text-neutral-300 hover:bg-neutral-700 border border-neutral-700'
              }`}
            >
              <Zap className="w-3.5 h-3.5" />
              {buildState.cableCpuEpsConnected ? 'CPU EPS 8-Pin: CONNECTED' : 'Route CPU EPS 8-Pin'}
            </button>

            {/* PCIe GPU Power */}
            <button
              onClick={() => {
                audioSynth.playCableConnect();
                onUpdateBuild((b) => ({ ...b, cablePcieConnected: !b.cablePcieConnected }));
              }}
              className={`flex items-center gap-2 px-3.5 py-2 text-xs font-semibold rounded-xl transition-all ${
                buildState.cablePcieConnected
                  ? 'bg-emerald-700/80 text-white'
                  : 'bg-neutral-800 text-neutral-300 hover:bg-neutral-700 border border-neutral-700'
              }`}
            >
              <Zap className="w-3.5 h-3.5" />
              {buildState.cablePcieConnected ? 'GPU PCIe 8-Pin: CONNECTED' : 'Route GPU PCIe 8-Pin'}
            </button>

            <button
              onClick={() => onZoneSelect('Workshop')}
              className="px-3 py-1.5 bg-neutral-700 hover:bg-neutral-600 text-white text-xs font-semibold rounded-lg ml-auto"
            >
              Ready to Close Case →
            </button>
          </div>
        );

      default:
        return null;
    }
  };

  return (
    <div className="w-full bg-neutral-900/95 backdrop-blur-md border-t border-neutral-800 px-5 py-3.5 flex flex-col md:flex-row items-start md:items-center justify-between gap-3 shadow-2xl z-20">
      <div className="flex items-center gap-2 text-xs text-neutral-400 shrink-0">
        <span className="font-semibold text-neutral-200 uppercase tracking-wider text-[11px]">
          {currentZone}
        </span>
        <span aria-hidden="true">·</span>
        <span>Contextual Actions</span>
      </div>

      <div className="flex-1 w-full md:w-auto overflow-x-auto pb-1 md:pb-0">
        {renderActions()}
      </div>
    </div>
  );
};
