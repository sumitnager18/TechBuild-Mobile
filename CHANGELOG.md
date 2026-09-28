# PC Technician Simulator — Changelog

All notable changes to the PC Technician Simulator architecture and codebase are documented in this file.

---

## [0.2.0] - Prototype 0.1.0 Stage 2: Core Simulation, Power Graph & Thermal Foundation

### Added
- **Directed Power Graph (`PCTechnician.Power`)**:
  - `PowerConnectorType`: Distinct pinouts (`ATX24Pin`, `CPUEPS8Pin`, `PCIe8Pin`, `PCIe12VHPWR`, etc.).
  - `PowerNode`: Conceptual power endpoint with max/required watts, connection state, and error message.
  - `PowerGraph`: Directed network validating connector pinouts and continuous/peak power rails.
  - `PowerLoadProfile`: Deterministic continuous, peak, and transient load calculation.
  - `PSUEvaluationResult`: Categorizes PSU status into `Healthy`, `Marginal`, `Insufficient`, and `MissingConnector`.
- **Deterministic Thermal Engine (`PCTechnician.Thermal`)**:
  - `ThermalState`: Discrete states (`Cold`, `Normal`, `Warm`, `Hot`, `Throttling`, `Critical`).
  - `ThermalPasteEvaluation`: Empirical efficiency curve modeling bare metal, dry hotspots, optimal pea dot, and excessive paste insulation.
  - `ThermalModel`: Calculates thermal resistance ($R_{cooler} + R_{contact}$) and delta-T based on TDP, cooler CFM/screws, and paste quality.
  - Dynamic performance throttling from 100% down to 60%, with critical emergency shutdown at >98°C.
- **Diagnostics & Simulation Evaluator (`PCTechnician.Diagnostics` & `PCTechnician.Simulation`)**:
  - `DiagnosticFaultCode`: Distinct codes for power, thermals, assembly, and memory.
  - `FaultSeverity`: `Info`, `Warning`, `Error`, `Critical`.
  - `DiagnosticResult`: Structured fault model with subsystem tagging and suggested inspection target.
  - `SimulationSnapshot`: Decoupled, serializable POCO containing stable hardware IDs and assembly states without GameObject references.
  - `SimulationEvaluator`: Pure centralized validator evaluating assembly, power rails, thermal safety, and overall POST viability.
- **Expanded Hardware Data Fields**:
  - `CPUData`: Peak power watts, thermal design limits, performance ratings, integrated graphics flag.
  - `GPUData`: PCIe generation, peak transient watts, thermal output, performance ratings.
  - `PSUData`: 12V rail capacity, transient excursion percentages.
- **Automated Tests (`PCTechnician.Tests`)**:
  - `PowerAndThermalTests.cs`: Implemented TC-11 through TC-22.
- **Interactive Verification Layer**:
  - Added **Simulation Inspector** tab to web applet displaying real-time Power, Thermal, and POST states.
  - Integrated TC-11 through TC-22 into `SimulationTestRunner`.

---

## [0.1.0] - Prototype 0.1.0 Stage 1: Engineering Foundation
- Core interfaces, ScriptableObjects, ComponentSlot state machine, touch interaction buffers, and camera focus zones.
