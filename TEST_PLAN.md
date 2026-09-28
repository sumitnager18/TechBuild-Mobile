# PC Technician Simulator — Test Plan & Quality Assurance
Version: Prototype 0.1.0 — Stage 2

---

## 1. Scope of Testing

This expanded test plan covers:
1. Pure simulation logic (CompatibilityEngine, ComponentSlot invariants).
2. Directed Power Graph, connector pinout validation, and load profiles.
3. Deterministic thermal resistance, paste quality, mount quality, and clock throttling.
4. Determinism and serialization invariance.

---

## 2. Automated Test Matrix

| ID | Test Case | Target System | Expected Outcome | Status |
|---|---|---|---|---|
| TC-01 | CPU Socket Matching | CompatibilityEngine | AM5 CPU matches AM5 Motherboard | PASS |
| TC-02 | CPU Socket Mismatch Rejection | CompatibilityEngine | LGA1200 CPU fails on AM5 with ERROR_CPU_SOCKET_MISMATCH | PASS |
| TC-03 | RAM Generation Matching | CompatibilityEngine | DDR5 RAM matches DDR5 Motherboard | PASS |
| TC-04 | RAM Generation Mismatch Rejection | CompatibilityEngine | DDR4 RAM fails on DDR5 Motherboard with ERROR_RAM_TYPE_MISMATCH | PASS |
| TC-05 | Cooler Socket Bracket Match | CompatibilityEngine | AM5 Cooler matches AM5 CPU | PASS |
| TC-06 | PSU Load & Connector Check | CompatibilityEngine | 750W PSU accepts 450W load; rejects 850W overload | PASS |
| TC-07 | ComponentSlot Category Invariant | ComponentSlot | Rejects incompatible categories (e.g. RAM into CPU socket) | PASS |
| TC-08 | ComponentSlot Occupancy Lock | ComponentSlot | Rejects double-installation into already occupied slot | PASS |
| TC-09 | Slot Prerequisite Gating | SlotPrerequisite | Gating prevents installation while retention arm is closed | PASS |
| TC-10 | Save/Load State Roundtrip | SaveManager | Serialized build state matches rehydrated build state exactly | PASS |
| TC-11 | Valid 24-Pin ATX Connection | PowerGraph | Connects successfully without rail faults | PASS |
| TC-12 | Incompatible Connector Pinout | PowerGraph | CPUEPS plugged into PCIe port fails with diagnostic error | PASS |
| TC-13 | Missing CPU EPS Fault | SimulationEvaluator | Halts POST with CPU_POWER_MISSING fault code | PASS |
| TC-14 | Missing GPU PCIe Power Fault | SimulationEvaluator | Halts POST with GPU_POWER_MISSING fault code | PASS |
| TC-15 | Insufficient PSU Status | PowerGraph | Returns PSUStatus.Insufficient when continuous load exceeds capacity | PASS |
| TC-16 | Correct Cooler Safe Thermals | ThermalModel | Full mounting & optimal paste maintains healthy temperature (35-65°C) | PASS |
| TC-17 | Missing Cooler Emergency Trip | ThermalModel | Missing cooler causes immediate Critical Emergency Shutdown (105°C) | PASS |
| TC-18 | Insufficient Thermal Paste | ThermalModel | Insufficient paste runs >10°C hotter than optimal pea dot | PASS |
| TC-19 | Excessive Thermal Paste | ThermalModel | Excessive paste thickness degrades conductance (>3°C hotter) | PASS |
| TC-20 | High Sustained Load Rise | ThermalModel | 170W CPU load scales higher in temperature than 105W baseline | PASS |
| TC-21 | Evaluation Determinism | SimulationEvaluator | Sequential evaluations of identical state produce bit-exact values | PASS |
| TC-22 | Save/Restore Parity | SimulationSnapshot | Rehydrated JSON snapshot produces identical evaluation and fault list | PASS |
