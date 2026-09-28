import React, { useState, useEffect } from 'react';
import {
  Wrench,
  Smartphone,
  Monitor,
  Volume2,
  VolumeX,
  RotateCcw,
  Sparkles,
  Layers,
  Cpu,
  Activity,
  CheckCircle2,
  BookOpen,
  Code2
} from 'lucide-react';
import { BuildState, FocusZoneType } from './types';
import { Workstation3D } from './components/Workstation3D';
import { ContextualActionDrawer } from './components/ContextualActionDrawer';
import { POSTMonitor } from './components/POSTMonitor';
import { SimulationTestRunner } from './components/SimulationTestRunner';
import { CodebaseExplorer } from './components/CodebaseExplorer';
import { DocumentationViewer } from './components/DocumentationViewer';
import { audioSynth } from './audio/audioSynthesizer';

import { SimulationDebugPanel } from './components/SimulationDebugPanel';

const INITIAL_BUILD_STATE: BuildState = {
  sidePanelRemoved: false,
  cpuSocketOpen: false,
  cpuInstalled: false,
  cpuAlignedCorrectly: false,
  thermalPasteApplied: false,
  thermalPasteQuality: 0,
  coolerInstalled: false,
  coolerScrewsTightened: 0,
  coolerFanConnected: false,
  ramLatchOpen: false,
  ramInstalled: false,
  m2ScrewRemoved: false,
  m2Installed: false,
  m2Screwed: false,
  gpuBracketRemoved: false,
  gpuPcieLatchOpen: false,
  gpuInstalled: false,
  cable24PinConnected: false,
  cableCpuEpsConnected: false,
  cablePcieConnected: false,
  caseClosed: true,
  systemPowered: false,
  postState: 'PowerOff',
  faultCode: 'NONE',
  faultMessage: '',
};

export default function App() {
  const [activeTab, setActiveTab] = useState<'workstation' | 'inspector' | 'post' | 'tests' | 'code' | 'docs'>('workstation');
  const [currentZone, setCurrentZone] = useState<FocusZoneType>('Workshop');
  const [isMobileFrame, setIsMobileFrame] = useState(false);
  const [soundEnabled, setSoundEnabled] = useState(true);

  // Persistence: Rehydrate or initialize
  const [buildState, setBuildState] = useState<BuildState>(() => {
    try {
      const saved = localStorage.getItem('pctech_sim_build_v1');
      if (saved) return JSON.parse(saved);
    } catch {
      // fallback
    }
    return INITIAL_BUILD_STATE;
  });

  // Save changes locally
  useEffect(() => {
    try {
      localStorage.setItem('pctech_sim_build_v1', JSON.stringify(buildState));
    } catch {
      // ignore
    }
  }, [buildState]);

  const handleResetBuild = () => {
    if (confirm('Reset entire workstation to factory unbuilt state?')) {
      audioSynth.triggerHaptic('medium');
      setBuildState(INITIAL_BUILD_STATE);
      setCurrentZone('Workshop');
    }
  };

  const handleToggleSound = () => {
    const next = !soundEnabled;
    setSoundEnabled(next);
    audioSynth.setSoundEnabled(next);
    audioSynth.setHapticsEnabled(next);
  };

  return (
    <div className="flex flex-col h-screen w-screen bg-neutral-950 text-neutral-100 overflow-hidden select-none font-sans">
      {/* Top Bar Contract: Zone 1 (Brand) - Zone 2 (Nav) - Zone 3 (Actions) */}
      <header className="h-14 border-b border-neutral-800 bg-neutral-900/90 backdrop-blur-md px-4 flex items-center justify-between shrink-0 z-30">
        {/* Zone 1: Single text wordmark */}
        <div className="flex items-center gap-3">
          <span className="text-sm font-extrabold tracking-tight text-neutral-100 uppercase">
            PC Technician Simulator
          </span>
          <span className="text-[11px] font-mono text-neutral-500 hidden sm:inline">
            v0.1.0 · Stage 1 Foundation
          </span>
        </div>

        {/* Zone 2: Navigation Links */}
        <nav className="flex items-center gap-1 sm:gap-2 text-xs font-semibold">
          <button
            onClick={() => setActiveTab('workstation')}
            className={`px-3 py-1.5 rounded-lg transition-colors ${
              activeTab === 'workstation'
                ? 'bg-neutral-800 text-sky-400 font-bold'
                : 'text-neutral-400 hover:text-neutral-200'
            }`}
          >
            3D Workstation
          </button>
          <button
            onClick={() => setActiveTab('inspector')}
            className={`px-3 py-1.5 rounded-lg transition-colors ${
              activeTab === 'inspector'
                ? 'bg-neutral-800 text-amber-400 font-bold'
                : 'text-neutral-400 hover:text-neutral-200'
            }`}
          >
            Simulation Inspector
          </button>
          <button
            onClick={() => setActiveTab('post')}
            className={`px-3 py-1.5 rounded-lg transition-colors ${
              activeTab === 'post'
                ? 'bg-neutral-800 text-sky-400 font-bold'
                : 'text-neutral-400 hover:text-neutral-200'
            }`}
          >
            Diagnostics & POST
          </button>
          <button
            onClick={() => setActiveTab('tests')}
            className={`px-3 py-1.5 rounded-lg transition-colors ${
              activeTab === 'tests'
                ? 'bg-neutral-800 text-sky-400 font-bold'
                : 'text-neutral-400 hover:text-neutral-200'
            }`}
          >
            Automated Tests
          </button>
          <button
            onClick={() => setActiveTab('code')}
            className={`px-3 py-1.5 rounded-lg transition-colors hidden md:block ${
              activeTab === 'code'
                ? 'bg-neutral-800 text-sky-400 font-bold'
                : 'text-neutral-400 hover:text-neutral-200'
            }`}
          >
            C# Codebase
          </button>
          <button
            onClick={() => setActiveTab('docs')}
            className={`px-3 py-1.5 rounded-lg transition-colors hidden sm:block ${
              activeTab === 'docs'
                ? 'bg-neutral-800 text-sky-400 font-bold'
                : 'text-neutral-400 hover:text-neutral-200'
            }`}
          >
            Documentation
          </button>
        </nav>

        {/* Zone 3: Primary Actions */}
        <div className="flex items-center gap-2">
          {/* Mobile Aspect Ratio Frame Toggle */}
          <button
            onClick={() => setIsMobileFrame(!isMobileFrame)}
            className={`p-2 rounded-lg border text-xs font-medium transition-all ${
              isMobileFrame
                ? 'bg-sky-950 border-sky-700 text-sky-300'
                : 'bg-neutral-800/80 border-neutral-700 text-neutral-400 hover:text-neutral-200'
            }`}
            title="Toggle Android Device Touch Viewport"
          >
            {isMobileFrame ? <Smartphone className="w-4 h-4" /> : <Monitor className="w-4 h-4" />}
          </button>

          {/* Sound & Haptics Toggle */}
          <button
            onClick={handleToggleSound}
            className="p-2 rounded-lg bg-neutral-800/80 border border-neutral-700 text-neutral-400 hover:text-neutral-200 text-xs font-medium"
            title="Audio & Haptics Toggle"
          >
            {soundEnabled ? <Volume2 className="w-4 h-4 text-emerald-400" /> : <VolumeX className="w-4 h-4 text-neutral-500" />}
          </button>

          {/* Reset Build */}
          <button
            onClick={handleResetBuild}
            className="p-2 rounded-lg bg-neutral-800/80 border border-neutral-700 text-neutral-400 hover:text-rose-400 text-xs font-medium"
            title="Reset Workstation"
          >
            <RotateCcw className="w-4 h-4" />
          </button>
        </div>
      </header>

      {/* Main Content Body */}
      <main className="flex-1 relative overflow-hidden flex items-center justify-center bg-neutral-950">
        {activeTab === 'workstation' && (
          <div
            className={`w-full h-full flex flex-col transition-all duration-300 ${
              isMobileFrame
                ? 'max-w-[440px] max-h-[92vh] my-auto rounded-3xl border-4 border-neutral-800 shadow-2xl overflow-hidden'
                : ''
            }`}
          >
            {/* 3D Viewport */}
            <div className="flex-1 relative overflow-hidden">
              <Workstation3D
                currentZone={currentZone}
                buildState={buildState}
                onZoneSelect={setCurrentZone}
              />
            </div>

            {/* Contextual Action Drawer */}
            <ContextualActionDrawer
              currentZone={currentZone}
              buildState={buildState}
              onUpdateBuild={setBuildState}
              onZoneSelect={setCurrentZone}
            />
          </div>
        )}

        {activeTab === 'inspector' && (
          <div className="w-full max-w-4xl p-6 overflow-y-auto max-h-[90vh]">
            <SimulationDebugPanel buildState={buildState} />
          </div>
        )}

        {activeTab === 'post' && (
          <div className="w-full max-w-2xl p-6 overflow-y-auto max-h-[90vh]">
            <POSTMonitor buildState={buildState} onUpdateBuild={setBuildState} />
          </div>
        )}

        {activeTab === 'tests' && (
          <div className="w-full max-w-3xl p-6 overflow-y-auto max-h-[90vh]">
            <SimulationTestRunner />
          </div>
        )}

        {activeTab === 'code' && (
          <div className="w-full max-w-5xl p-6 overflow-y-auto max-h-[90vh]">
            <CodebaseExplorer />
          </div>
        )}

        {activeTab === 'docs' && (
          <div className="w-full max-w-4xl p-6 overflow-y-auto max-h-[90vh]">
            <DocumentationViewer />
          </div>
        )}
      </main>
    </div>
  );
}
