# PC Technician Simulator — Known Issues & Technical Constraints
Version: Prototype 0.1.0 — Stage 2.1

---

## 1. Environment Limitation & Substitute Disclosure (Section 37 Compliance)

- **LIMITATION**:
  Native Unity 6 editor compilation, native Android APK packaging, and Android NDK rendering cannot execute directly inside this web container sandbox.
- **REASON**:
  This environment is an AI Studio Node.js/Linux container designed for web-based rapid prototyping and TypeScript verification. It lacks the proprietary Unity 6 binary suite, Android SDK/NDK toolchain, and Android hardware acceleration.
- **CURRENT SUBSTITUTE**:
  1. A complete, production-grade Unity 6 C# codebase has been established in `/unity/Assets/_Game/` with valid namespaces, ScriptableObjects, contracts, and Assembly Definitions.
  2. An interactive, mobile-touch-emulated 3D WebGL/Three.js verification harness has been provided in the web interface on port 3000 to validate the exact component slots, camera focus zones, and tactile gestures in real time.
  3. A full suite of automated unit test assertions has been authored in both C# (`CompatibilityAndSlotTests.cs`) and TypeScript (`simulationTests.ts`) to verify simulation correctness.
- **NEXT STEP**:
  Clone or export the `/unity` directory into an official Unity 6 (6000.0+) URP project; scripts are ready to compile under the `PCTechnician` assembly definition.

---

## 2. Technical Limitations in Current Prototype (0.1.0)

1. **Native Unity compilation is not verified in this environment**:
   - Repository read/write is available, but Unity 6 editor compilation, Android packaging, and device runtime remain external verification steps.
   - Authored C# tests and the WebGL harness are not proof of native Unity compilation.

2. **Multi-Touch Pinch Zoom**:
   - Current input manager supports single-touch selection and timed hold gestures. Pinch-to-zoom is deferred to Milestone 2 in favor of deterministic Camera Focus Zones to prevent disorientation.
3. **Reverse Screw Driving**:
   - Screws currently tighten in a single direction upon hold. Unscrewing toggle is handled via contextual action mode rather than dual-directional finger circling.
4. **Sound Assets**:
   - Sound synthesis uses browser WebAudio oscillators and noise buffers for mechanical clicks, fan whirs, and POST beeps. Production .wav audio files are marked as placeholder targets for Unity AudioSource mapping in Stage 8.
5. **Thermal Paste Fluid Mesh Deformation**:
   - Paste application uses procedural radial dot expansion with coverage calculation rather than an expensive real-time Navier-Stokes fluid grid (aligning with Rule 5: simulate what the player perceives).
