# PC Technician Simulator — Current Implementation Status
Version: Prototype 0.1.0 — Stage 2: Core Simulation, Power Graph & Thermal Foundation
Target Platform: Android (Unity 6 URP)
Verification Layer: Three.js / WebGL / TypeScript Test Harness

---

## 1. Executive Summary

Stage 2 completes the pure C# simulation engine, establishing the directed Power Graph, deterministic Thermal Model with thermal paste quality scoring, centralized Simulation Evaluator, serializable Simulation Snapshot, and automated test suite covering TC-11 through TC-22.

---

## 2. Implemented Sub-Systems

### A. Power Graph Architecture (`PCTechnician.Power`)
- `PowerConnectorType`: Distinct pinout enum (`ATX24Pin`, `CPUEPS8Pin`, `PCIe8Pin`, `PCIe12VHPWR`, etc.).
- `PowerNode`: Deterministic power endpoint tracking source, destination, connector constraints, and fault states.
- `PowerGraph`: Directed network validating connector compatibility and rail continuity.
- `PowerLoadProfile`: Deterministic calculation of continuous load, peak load, and transient excursion.
- `PSUEvaluationResult`: Evaluates PSU capacity into `Healthy`, `Marginal`, `Insufficient`, or `MissingConnector`.

### B. Deterministic Thermal Engine (`PCTechnician.Thermal`)
- `ThermalState`: Discrete states (`Cold`, `Normal`, `Warm`, `Hot`, `Throttling`, `Critical`).
- `ThermalPasteEvaluation`: Empirical efficiency curve modeling bare metal, dry hotspots, optimal pea dot, and excessive paste insulation.
- `ThermalCalculationResult`: Records temperature, performance multiplier (100% down to 60%), and emergency shutdown flags.
- `ThermalModel`: Calculates thermal resistance ($R_{cooler} + R_{contact}$) and delta-T based on TDP, cooler CFM/screws, and paste quality.

### C. Diagnostic & Evaluator System (`PCTechnician.Diagnostics` & `PCTechnician.Simulation`)
- `DiagnosticFaultCode`: Distinct codes for power, thermals, assembly, and memory.
- `FaultSeverity`: `Info`, `Warning`, `Error`, `Critical`.
- `DiagnosticResult`: Structured fault model with subsystem tagging and suggested inspection target.
- `SimulationSnapshot`: Decoupled, serializable POCO containing stable hardware IDs and assembly states without GameObject references.
- `SimulationEvaluator`: Pure centralized validator evaluating assembly, power rails, thermal safety, and overall POST viability.

### D. Automated Verification Suite (`PCTechnician.Tests`)
- `CompatibilityAndSlotTests.cs`: TC-01 to TC-10 (compatibility and slot rules).
- `PowerAndThermalTests.cs`: TC-11 to TC-22 (Power Graph, connector validation, thermal models, and determinism).

---

## 3. Directory Layout

```
unity/Assets/_Game/
├── Scripts/
│   ├── Core/
│   │   ├── GameBootstrap.cs
│   │   ├── IAssemblyAction.cs
│   │   ├── IInspectable.cs
│   │   ├── IInstallable.cs
│   │   ├── IInteractable.cs
│   │   ├── IPrerequisite.cs
│   │   └── IRemovable.cs
│   ├── Hardware/
│   │   ├── CableData.cs
│   │   ├── CaseData.cs
│   │   ├── CompatibilityEngine.cs
│   │   ├── CompatibilityResult.cs
│   │   ├── ComponentCategory.cs
│   │   ├── CoolerData.cs
│   │   ├── CPUData.cs
│   │   ├── GPUData.cs
│   │   ├── HardwareData.cs
│   │   ├── MotherboardData.cs
│   │   ├── PSUData.cs
│   │   ├── RAMData.cs
│   │   └── StorageData.cs
│   ├── Power/
│   │   ├── PowerConnectorType.cs
│   │   ├── PowerGraph.cs
│   │   ├── PowerLoadProfile.cs
│   │   ├── PowerNode.cs
│   │   └── PSUEvaluationResult.cs
│   ├── Thermal/
│   │   ├── ThermalCalculationResult.cs
│   │   ├── ThermalModel.cs
│   │   ├── ThermalPasteEvaluation.cs
│   │   └── ThermalState.cs
│   ├── Diagnostics/
│   │   ├── DiagnosticFaultCode.cs
│   │   ├── DiagnosticResult.cs
│   │   ├── DiagnosticSubsystem.cs
│   │   └── FaultSeverity.cs
│   ├── Simulation/
│   │   ├── SimulationEvaluationResult.cs
│   │   ├── SimulationEvaluator.cs
│   │   └── SimulationSnapshot.cs
│   ├── Assembly/
│   │   ├── ComponentSlot.cs
│   │   ├── ComponentState.cs
│   │   ├── HardwareInstance.cs
│   │   └── SlotPrerequisite.cs
│   ├── Interaction/
│   │   ├── InteractionManager.cs
│   │   ├── InteractionType.cs
│   │   └── TouchInteractionTarget.cs
│   ├── Camera/
│   │   ├── CameraManager.cs
│   │   ├── FocusZone.cs
│   │   └── FocusZoneType.cs
│   ├── Tests/
│   │   ├── CompatibilityAndSlotTests.cs
│   │   └── PowerAndThermalTests.cs
│   └── PCTechnician.asmdef
```
