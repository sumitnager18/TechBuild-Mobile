# PC Technician Simulator — System Architecture Document
Version: 1.1.0 (Stage 2: Core Simulation, Power Graph & Thermal Foundation)
Engine Target: Unity 6 (URP) | Language: C# | Target Platform: Android (ARM64)
Prototype Web Verification Layer: Three.js / WebGL / React 19 / TypeScript

---

## 1. Architectural Overview & Separation of Concerns

PC Technician Simulator strictly decouples hardware simulation from visual presentation:
1. **Simulation Layer (Pure Logic & State)**:
   - Contains ZERO dependencies on `UnityEngine.GameObject`, `MeshRenderer`, `Transform`, `Collider`, `Animator`, or `Camera`.
   - Executes deterministically in pure C# memory space.
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
- Each `PowerNode` specifies its strict `PowerConnectorType` (ATX24Pin, CPUEPS8Pin, PCIe8Pin).
- Incompatible pinouts (e.g. plugging a CPU EPS cable into a PCIe 8-pin port) are rejected deterministically with structured diagnostic faults.

### Power Load Formulas
- **Continuous Load ($W_{cont}$)**:
  $$W_{cont} = \text{TDP}_{cpu} + \text{TDP}_{gpu} + W_{base}$$
  where $W_{base} = 60\text{W}$ (motherboard, RAM, NVMe, fans).
- **Peak Load ($W_{peak}$)**:
  $$W_{peak} = P_{cpu,peak} + (\text{TDP}_{gpu} \times 1.15) + W_{base} + 25\text{W}$$
- **Transient Load ($W_{trans}$)**:
  $$W_{trans} = P_{cpu,peak} + P_{gpu,transient} + W_{base} + 40\text{W}$$

### PSU Evaluation Model
- `Healthy`: Headroom $\ge 15\%$ of rated capacity.
- `Marginal`: Headroom $< 15\%$ but continuous load within limits.
- `Insufficient`: Headroom $< 0$ (load exceeds PSU capacity).
- `MissingConnector`: Available modular cables < required device inputs.

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
- $T_{cpu} < 35^\circ\text{C}$: `Cold` (100% clock)
- $35^\circ\text{C} \le T_{cpu} < 65^\circ\text{C}$: `Normal` (100% clock)
- $65^\circ\text{C} \le T_{cpu} < 78^\circ\text{C}$: `Warm` (100% clock)
- $78^\circ\text{C} \le T_{cpu} < 88^\circ\text{C}$: `Hot` (100% clock)
- $88^\circ\text{C} \le T_{cpu} \le 98^\circ\text{C}$: `Throttling` (clock scaled down from 100% to 60%)
- $T_{cpu} > 98^\circ\text{C}$: `Critical` (emergency shutdown, 0% clock)

---

## 4. Simulation Snapshot & Persistence

```json
{
  "version": 1,
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
  "powerSwitchOn": true
}
```
Contains ZERO GameObject instance references or scene memory pointers.
Rehydration produces 100% bit-exact simulation evaluation.
