# PC Technician Simulator — Future Systems Backlog
Version: Prototype 0.1.0

Per Section 38 (Critical Anti-Scope-Explosion Rule), systems identified as valuable but outside Stage 1/Vertical Slice 1 are scheduled below:

---

## 1. Benchmarking & Synthetic Workloads
- **Feature Name**: Synthetic System Benchmark (ApexMark 3D)
- **Purpose**: Simulate real-time CPU/GPU thermal dissipation, clock throttling, and 3D score generation based on thermal paste quality, cooler CFM, and airflow.
- **Dependencies**: ThermalSimulationEngine, PowerGraph, POSTController.
- **Estimated Complexity**: Medium (4-5 engineering days).
- **Recommended Milestone**: Milestone 3 (Performance Tuning & Diagnostics).

---

## 2. Customer Career & Workshop Ticket Management
- **Feature Name**: Customer Job Pipeline & Workshop Economy
- **Purpose**: Procedural customer repair requests ("Screen won't turn on", "Overheating during gaming", "Upgrade to 32GB RAM"), repair budgets, invoice calculation, and shop reputation.
- **Dependencies**: DiagnosticEngine, SaveManager, InventorySystem.
- **Estimated Complexity**: High (8-10 engineering days).
- **Recommended Milestone**: Milestone 4 (Career Mode & Progression).

---

## 3. Water Cooling & Custom Loops
- **Feature Name**: Custom Liquid Cooling (AIO & Custom Loop)
- **Purpose**: Flexible tube routing, radiator placement, pump speed control, coolant filling mini-game, and leak testing.
- **Dependencies**: CableRoutingSystem (extended to Bezier tube simulation), ThermalSimulationEngine.
- **Estimated Complexity**: High (10 engineering days).
- **Recommended Milestone**: Milestone 5 (Advanced Enthusiast Workshop).

---

## 4. Full BIOS Setup Utility Simulation
- **Feature Name**: Interactive BIOS / UEFI Setup Utility
- **Purpose**: Configurable fan curves, XMP/EXPO memory profile activation, boot drive priority, voltage offset tuning, and thermal trip temperature settings.
- **Dependencies**: POSTController, DiagnosticEngine.
- **Estimated Complexity**: Medium (3-4 engineering days).
- **Recommended Milestone**: Milestone 2 (POST & Boot Refinement).

---

## 5. Dynamic Cable Routing & Tie-Downs
- **Feature Name**: Back-Panel Cable Management & Zip-Ties
- **Purpose**: Route 24-pin and PCIe cables through chassis rubber grommets, cinch cables with velcro ties, and close rear case panel without bulge friction.
- **Dependencies**: CableRoutingSystem, ChassisColliderSystem.
- **Estimated Complexity**: Medium-High (6 engineering days).
- **Recommended Milestone**: Milestone 3 (Chassis & Cable Detail).

---

## 6. Procedural Component Wear & Dirt
- **Feature Name**: Dust Accumulation & Cleaning Canned Air Mini-Game
- **Purpose**: Simulate dirty heatsinks, dust-caked intake filters, dried thermal paste requiring isopropyl alcohol wipes.
- **Dependencies**: ThermalSimulationEngine, ToolData (`ThermalPasteCleaner`).
- **Estimated Complexity**: Medium (4 engineering days).
- **Recommended Milestone**: Milestone 4 (Repair & Diagnostics Depth).
