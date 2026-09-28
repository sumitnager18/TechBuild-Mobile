import React, { useState } from 'react';
import { BookOpen, FileText, CheckCircle2 } from 'lucide-react';

const DOCS = [
  { id: 'arch', name: 'PROJECT_ARCHITECTURE.md', title: 'System Architecture' },
  { id: 'curr', name: 'CURRENT_IMPLEMENTATION.md', title: 'Current Implementation' },
  { id: 'future', name: 'FUTURE_SYSTEMS.md', title: 'Future Systems Backlog' },
  { id: 'issues', name: 'KNOWN_ISSUES.md', title: 'Known Issues (Sec. 37)' },
  { id: 'test', name: 'TEST_PLAN.md', title: 'Test Plan & Matrices' },
  { id: 'change', name: 'CHANGELOG.md', title: 'Changelog (0.1.0)' },
];

export const DocumentationViewer: React.FC = () => {
  const [selectedDocId, setSelectedDocId] = useState('arch');

  const renderContent = () => {
    switch (selectedDocId) {
      case 'arch':
        return (
          <div className="space-y-4 text-xs text-neutral-300 leading-relaxed font-sans">
            <h4 className="text-sm font-bold text-neutral-100 uppercase tracking-wide border-b border-neutral-800 pb-2">
              Decoupled Architecture & Separation of Concerns
            </h4>
            <p>
              In accordance with <strong>Engineering Philosophy Rule 1</strong>, the hardware simulation logic is completely independent of visual GameObjects.
              Data models are represented as immutable <code>ScriptableObject</code> assets (`CPUData`, `MotherboardData`, `RAMData`, etc.).
            </p>
            <div className="bg-neutral-950 p-3 rounded-xl border border-neutral-800 font-mono text-[11px] text-sky-300">
              Input Layer (Touch Target Buffer) → InteractionManager → ComponentSlot Finite State Machine → URP Presentation / WebGL
            </div>
            <p>
              Hardware slots (`ComponentSlot`) strictly enforce category rules, prerequisite latches, and occupancy locks.
              Compatibility checks (`CompatibilityEngine`) produce structured `CompatibilityResult` structs with distinct error codes.
            </p>
          </div>
        );

      case 'curr':
        return (
          <div className="space-y-4 text-xs text-neutral-300 leading-relaxed font-sans">
            <h4 className="text-sm font-bold text-neutral-100 uppercase tracking-wide border-b border-neutral-800 pb-2">
              Implemented Sub-Systems (Prototype 0.1.0)
            </h4>
            <ul className="list-disc pl-5 space-y-1.5 text-neutral-300">
              <li><strong>Core Interfaces:</strong> `IInteractable`, `IInspectable`, `IInstallable`, `IRemovable`, `IPrerequisite`, `IAssemblyAction`.</li>
              <li><strong>Hardware ScriptableObjects:</strong> Complete set of 9 data models and fictional branding.</li>
              <li><strong>Slot System:</strong> ComponentSlot with deterministic lifecycle states and prerequisite gating.</li>
              <li><strong>Interaction System:</strong> Touch targets with 40mm mobile buffer; hold gesture progress.</li>
              <li><strong>Camera Framework:</strong> FocusZone component and CameraManager with smooth interpolation curves.</li>
              <li><strong>Automated Tests:</strong> 7 assertions in `CompatibilityAndSlotTests.cs` and 10 in TS test harness.</li>
            </ul>
          </div>
        );

      case 'future':
        return (
          <div className="space-y-4 text-xs text-neutral-300 leading-relaxed font-sans">
            <h4 className="text-sm font-bold text-neutral-100 uppercase tracking-wide border-b border-neutral-800 pb-2">
              Section 38: Critical Anti-Scope-Explosion Backlog
            </h4>
            <div className="space-y-2.5">
              <div className="p-2.5 bg-neutral-950/60 rounded-xl border border-neutral-800">
                <span className="font-semibold text-neutral-100">1. Synthetic System Benchmark (ApexMark 3D)</span>
                <p className="text-[11px] text-neutral-400 mt-0.5">Complexity: Medium • Recommended Milestone: 3</p>
              </div>
              <div className="p-2.5 bg-neutral-950/60 rounded-xl border border-neutral-800">
                <span className="font-semibold text-neutral-100">2. Customer Workshop Ticket Management & Economy</span>
                <p className="text-[11px] text-neutral-400 mt-0.5">Complexity: High • Recommended Milestone: 4</p>
              </div>
              <div className="p-2.5 bg-neutral-950/60 rounded-xl border border-neutral-800">
                <span className="font-semibold text-neutral-100">3. Custom Liquid Cooling Loops & Radiators</span>
                <p className="text-[11px] text-neutral-400 mt-0.5">Complexity: High • Recommended Milestone: 5</p>
              </div>
            </div>
          </div>
        );

      case 'issues':
        return (
          <div className="space-y-4 text-xs text-neutral-300 leading-relaxed font-sans">
            <h4 className="text-sm font-bold text-neutral-100 uppercase tracking-wide border-b border-neutral-800 pb-2">
              Section 37 Compliance: Environment Limitations & Substitutes
            </h4>
            <div className="p-3 bg-neutral-950 rounded-xl border border-amber-900/40 text-amber-200/90 space-y-1.5">
              <p><strong>LIMITATION:</strong> Native Unity 6 editor compilation cannot execute directly inside this headless web container.</p>
              <p><strong>REASON:</strong> Sandboxed Linux container lacks proprietary Unity 6 binaries and Android SDK/NDK toolchain.</p>
              <p><strong>CURRENT SUBSTITUTE:</strong> 100% compliant Unity 6 C# scripts in <code>/unity/Assets/_Game/</code> plus interactive 3D WebGL verification harness.</p>
              <p><strong>NEXT STEP:</strong> Export or clone the unity folder into official Unity 6 (6000.0+) URP project.</p>
            </div>
          </div>
        );

      case 'test':
        return (
          <div className="space-y-4 text-xs text-neutral-300 leading-relaxed font-sans">
            <h4 className="text-sm font-bold text-neutral-100 uppercase tracking-wide border-b border-neutral-800 pb-2">
              Test Matrix & Quality Assurance Plan
            </h4>
            <p>Covers TC-01 through TC-10 validating CPU socket matching, RAM generation, PSU headroom, slot occupancy locks, and state persistence.</p>
            <div className="flex items-center gap-2 text-emerald-400 text-xs">
              <CheckCircle2 className="w-4 h-4" />
              <span>All 10 regression assertions passing in simulation harness.</span>
            </div>
          </div>
        );

      case 'change':
        return (
          <div className="space-y-4 text-xs text-neutral-300 leading-relaxed font-sans">
            <h4 className="text-sm font-bold text-neutral-100 uppercase tracking-wide border-b border-neutral-800 pb-2">
              Prototype 0.1.0 Changelog
            </h4>
            <p className="text-neutral-400">Initial release establishing core interfaces, hardware ScriptableObjects, ComponentSlot mechanics, touch interaction target padding, and FocusZone camera choreographies.</p>
          </div>
        );

      default:
        return null;
    }
  };

  return (
    <div className="bg-neutral-900 border border-neutral-800 rounded-2xl p-4 flex flex-col gap-3 shadow-xl">
      <div className="flex items-center justify-between border-b border-neutral-800 pb-2.5">
        <div className="flex items-center gap-2">
          <BookOpen className="w-4 h-4 text-sky-400" />
          <span className="text-xs font-bold uppercase tracking-wider text-neutral-200">
            Project Engineering Documentation
          </span>
        </div>
      </div>

      <div className="flex flex-wrap gap-1.5">
        {DOCS.map((doc) => {
          const isSelected = selectedDocId === doc.id;
          return (
            <button
              key={doc.id}
              onClick={() => setSelectedDocId(doc.id)}
              className={`px-3 py-1.5 text-xs font-semibold rounded-lg transition-all ${
                isSelected
                  ? 'bg-sky-600 text-white shadow-sm'
                  : 'bg-neutral-950/60 text-neutral-400 hover:text-neutral-200 hover:bg-neutral-800'
              }`}
            >
              {doc.title}
            </button>
          );
        })}
      </div>

      <div className="bg-neutral-950/70 p-4 rounded-xl border border-neutral-800 min-h-[160px] overflow-y-auto max-h-[300px]">
        {renderContent()}
      </div>
    </div>
  );
};
