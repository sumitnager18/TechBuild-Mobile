# PC Technician Simulator — System Architecture Document
Version: 1.2.0 (Stage 2.1 Hardening)
Engine Target: Unity 6 (URP) | Language: C# | Target Platform: Android (ARM64)
Prototype Web Verification Layer: Three.js / WebGL / React 19 / TypeScript

---

## 1. Architectural Overview & Separation of Concerns

PC Technician Simulator strictly decouples hardware simulation from visual presentation:
1. **Simulation Logic & State**:
   - Core evaluators, power graph, thermal model, diagnostics, and snapshot state have no GameObject/scene dependencies.
   - Hardware specification assets are intentionally Unity `ScriptableObject` data definitions; therefore the entire simulation assembly is not accurately described as a zero-UnityEngine pure-C# layer.
   - Deterministic evaluation is performed from explicit inputs and stable IDs.
   - Evaluates on meaningful state transitions (cable attached, screw torqued) rather than per-frame Update loops.
2. **Presentation Layer (Visuals & Animation)**:
   - Unity URP presentation / Three.js verification harness.
   - Listens to snapshot changes and drives mesh anchors, camera focus, and fan kinematics.
3. **Interaction & Input Layer (Mobile Touch First)**:
   - Decoupled touch volumes with 40mm mobile finger padding.
   - Timed hold gestures for screws and latches.
4. **Perception Systems (Audio & Haptics)**:
   - Centralized audio event dispatching and vibration feedback.

```
+-------------------------------------------------------------------------+
|                              USER INPUT                                 |
|          (Touch Raycasts, Contextual Action Drawer, Gestures)           |
+------------------------------------+------------------------------------+
                                     |
                                     v
+------------------------------------+------------------------------------+
|                         INTERACTION LAYER                               |
|        InteractionManager <--> TouchInteractionTarget <--> FocusZone    |
+------------------------------------+------------------------------------+
                                     |
                                     v
+------------------------------------+------------------------------------+
|                         SIMULATION LAYER                                |
|  - ComponentSlot & ComponentData (ScriptableObjects)                    |
|  - CompatibilityEngine (Sockets, RAM, PCIe, Severity)                   |
|  - Directed PowerGraph (PowerNode, ATX24, CPU EPS, PCIe, LoadProfile)  |
|  - ThermalModel (Deterministic delta-T, Paste Quality, Throttling)      |
|  - SimulationEvaluator (CanPostSucceed, IsAssembled, IsPowered)         |
|  - SimulationSnapshot (Serializable POCO without GameObject refs)       |
+------------------+----------------------------------+-------------------+
                   |                                  |
                   v                                  v
+------------------+-----------------+  +-------------+-------------------+
|       PRESENTATION & ANIMATION     |  |       PERCEPTION SYSTEMS        |
| - URP Shaders / Meshes / Transforms|  | - AudioManager (Synthesized SFX)|
| - CameraManager (FocusZones Lerp)  |  | - HapticManager (Android Vibration)
| - Screwdriver Tool Kinematics      |  | - Contextual HUD & POST Display |
+------------------------------------+  +---------------------------------+
```

---

## 2. Directed Power Graph (`PCTechnician.Power`)

The Power Graph is a directed network connecting the power source (`PSU`) to consumer nodes:
- `Rail_ATX_24Pin`: Connects PSU to Motherboard chipset and logic.
- `Rail_CPU_EPS`: Connects PSU 8-pin 12V rail to CPU VRMs.
- `Rail_PCIe_GPU`: Connects PSU PCIe 8-pin / 12V-2x6 rail to GPU VRMs.

### Connector Validation Invariants
- Each `PowerNode` specifies its strict `PowerConnectorType`.
- Each PSU connector is a unique `PowerSourceConnector` resource with an identity and capacity.
- A source connector cannot be consumed by two destinations.
- Exact source-to-rail mappings are persisted in `PowerConnectionState`; legacy rail-only snapshots are still accepted.

### Power Load Formulas
- **Continuous Load ($W_{cont}$)**:
  $$W_{cont} = \text{TDP}_{cpu} + \text{TDP}_{gpu} + W_{base}$$
  where $W_{base} = 60\text{W}$ (motherboard, RAM, NVMe, fans).
- **Peak Load ($W_{peak}$)**:
  $$W_{peak} = P_{cpu,peak} + (\text{TDP}_{gpu} \times 1.15) + W_{base} + 25\text{W}$$
- **Transient Load ($W_{trans}$)**:
  $$W_{trans} = P_{cpu,peak} + P_{gpu,transient} + W_{base} + 40\text{W}$$

### PSU Evaluation Model
- Continuous capacity is checked against the PSU's rated wattage.
- Peak load is checked against rated wattage.
- Transient load is checked against `Wattage × TransientExcursionPercentage / 100`; no unrelated hard-coded 10% allowance is used by the hardened evaluator.
- `Healthy`: at least 15% continuous headroom.
- `Marginal`: less than 15% continuous headroom while capacity remains sufficient.
- `Insufficient`: continuous, peak, or configured transient capacity is exceeded.
- `MissingConnector`: required PSU connector inventory is unavailable.

---

## 3. Deterministic Thermal Model (`PCTechnician.Thermal`)

Calculates component temperatures without fluid dynamics or CFD grids:

### Inputs
1. Ambient Temperature ($T_{ambient} = 22.0^\circ\text{C}$)
2. CPU Power Draw ($P_{cpu} = 105\text{W}$)
3. Cooler Rated TDP ($C_{cooler} = 180\text{W}$)
4. Cooler Mounting Screws ($N_{screws} = 0 \dots 4$)
5. Cooler Fan Connection ($F_{connected} \in \{0, 1\}$)
6. Thermal Paste Amount ($A_{paste} \in [0.0, 1.5]$)
7. Case Airflow Factor ($K_{airflow} = 1.0$)

### Formulations
1. **Thermal Paste Efficiency ($\eta_{paste}$)**:
   - $A \le 0.01$: $\eta = 0.08$ (bare metal air gap)
   - $0.01 < A < 0.45$: $\eta = 0.15 + \frac{A}{0.45} \times 0.65$
   - $0.45 \le A \le 0.70$: $\eta = 1.0 - (|A - 0.55| \times 0.35)$ (Optimal pea dot)
   - $0.70 < A \le 1.0$: $\eta = 0.92 - ((A - 0.70) \times 0.25)$
   - $A > 1.0$: $\eta = \max(0.68, 0.84 - ((A - 1.0) \times 0.28))$ (Excessive thickness penalty)
2. **Mount Quality ($Q_{mount}$)**:
   $$Q_{mount} = \max\left(0.15, \frac{N_{screws}}{4}\right)$$
3. **Active Cooler Capacity ($C_{active}$)**:
   $$C_{active} = \begin{cases} C_{cooler} & \text{if } F_{connected} = 1 \\ C_{cooler} \times 0.35 & \text{if } F_{connected} = 0 \text{ (passive only)} \end{cases}$$
4. **Effective Thermal Resistance ($R_{th}$)**:
   $$R_{cooler} = \frac{150}{\max(50, C_{active})} \times 0.24$$
   $$R_{contact} = \frac{0.22}{\max(0.08, \eta_{paste}) \times \max(0.15, Q_{mount})}$$
   $$R_{eff} = \frac{R_{cooler} + R_{contact}}{K_{airflow}}$$
5. **Final CPU Temperature ($T_{cpu}$)**:
   $$T_{cpu} = T_{ambient} + (P_{cpu} \times R_{eff})$$

### Thermal States & Throttling
- CPU safety limit comes from `CPUData.MaxSafeTempCelsius`.
- Throttling begins at `MaxSafeTempCelsius - 7°C` by default.
- Other state thresholds and thermal coefficients are centralized in `ThermalModelParameters`.
- The thermal model is explicitly a gameplay-tuning model, not a physically validated CFD/thermal solver.

---

## 4. Simulation Snapshot & Persistence

```json
{
  "version": 2,
  "timestamp": 1742080000,
  "motherboardId": "ApexForge_A870",
  "cpuId": "NovaCore_N7_7600",
  "coolerId": "ArcticBreeze_A4",
  "gpuId": "VectorX_VX_780",
  "psuId": "VoltEdge_750",
  "storageId": "HyperDrive_2TB",
  "ramModuleIds": ["TitanRAM_32"],
  "cpuSocketLatched": true,
  "thermalPasteAmount": 0.55,
  "coolerScrewsTightened": 4,
  "coolerFanConnected": true,
  "ramLatched": true,
  "gpuPcieLatched": true,
  "connectedPowerRails": ["Rail_ATX_24Pin", "Rail_CPU_EPS", "Rail_PCIe_GPU"],
  "connectedPowerConnections": [
    {"railId": "Rail_ATX_24Pin", "sourceConnectorId": "PSU_ATX24_1"},
    {"railId": "Rail_CPU_EPS", "sourceConnectorId": "PSU_EPS8_1"},
    {"railId": "Rail_PCIe_GPU", "sourceConnectorId": "PSU_PCIE8_1"}
  ],
  "powerSwitchOn": true
}
```
Contains no GameObject instance references or scene memory pointers.
Rehydration is expected to produce the same deterministic evaluation for identical inputs; native Unity runtime verification remains required.
