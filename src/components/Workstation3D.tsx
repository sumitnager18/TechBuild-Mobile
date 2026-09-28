import React, { useEffect, useRef } from 'react';
import * as THREE from 'three';
import { BuildState, FocusZoneType } from '../types';

interface Workstation3DProps {
  currentZone: FocusZoneType;
  buildState: BuildState;
  onZoneSelect: (zone: FocusZoneType) => void;
}

const ZONE_CAMERA_TARGETS: Record<
  FocusZoneType,
  { pos: [number, number, number]; lookAt: [number, number, number]; fov: number }
> = {
  Workshop: { pos: [0.35, 0.85, 1.9], lookAt: [0, 0.35, 0], fov: 46 },
  Assembly: { pos: [0.05, 0.46, 1.15], lookAt: [0, 0.36, 0], fov: 38 },
  CPUSocket: { pos: [-0.06, 0.47, 0.44], lookAt: [-0.06, 0.45, 0.06], fov: 24 },
  RAMSlots: { pos: [0.12, 0.49, 0.45], lookAt: [0.09, 0.46, 0.06], fov: 22 },
  PCIeSlot: { pos: [0.02, 0.26, 0.54], lookAt: [0.01, 0.24, 0.06], fov: 26 },
  M2Slot: { pos: [0.03, 0.33, 0.38], lookAt: [0.03, 0.32, 0.06], fov: 20 },
  PowerArea: { pos: [0.22, 0.38, 0.46], lookAt: [0.17, 0.36, 0.06], fov: 24 },
  PSUBasement: { pos: [0.02, 0.08, 0.58], lookAt: [0, 0.08, 0], fov: 28 },
};

export const Workstation3D: React.FC<Workstation3DProps> = ({
  currentZone,
  buildState,
  onZoneSelect,
}) => {
  const containerRef = useRef<HTMLDivElement>(null);
  const sceneRef = useRef<THREE.Scene | null>(null);
  const cameraRef = useRef<THREE.PerspectiveCamera | null>(null);
  const rendererRef = useRef<THREE.WebGLRenderer | null>(null);

  // References to animated 3D meshes
  const sidePanelMeshRef = useRef<THREE.Mesh | null>(null);
  const cpuMeshRef = useRef<THREE.Group | null>(null);
  const cpuSocketLeverRef = useRef<THREE.Group | null>(null);
  const thermalPasteMeshRef = useRef<THREE.Mesh | null>(null);
  const coolerMeshRef = useRef<THREE.Group | null>(null);
  const ramMeshRef = useRef<THREE.Group | null>(null);
  const m2MeshRef = useRef<THREE.Group | null>(null);
  const gpuMeshRef = useRef<THREE.Group | null>(null);
  const cable24PinRef = useRef<THREE.Group | null>(null);
  const cableCpuEpsRef = useRef<THREE.Group | null>(null);
  const cablePcieRef = useRef<THREE.Group | null>(null);
  const fanBladesRef = useRef<THREE.Group[]>([]);

  // Debug LEDs
  const ledCpuRef = useRef<THREE.Mesh | null>(null);
  const ledDramRef = useRef<THREE.Mesh | null>(null);
  const ledVgaRef = useRef<THREE.Mesh | null>(null);
  const ledBootRef = useRef<THREE.Mesh | null>(null);

  // Camera animation target values
  const cameraTarget = useRef({
    currentPos: new THREE.Vector3(0.35, 0.85, 1.9),
    targetPos: new THREE.Vector3(0.35, 0.85, 1.9),
    currentLookAt: new THREE.Vector3(0, 0.35, 0),
    targetLookAt: new THREE.Vector3(0, 0.35, 0),
    currentFov: 46,
    targetFov: 46,
  });

  // Update camera target when focus zone changes
  useEffect(() => {
    const config = ZONE_CAMERA_TARGETS[currentZone];
    if (config) {
      cameraTarget.current.targetPos.set(...config.pos);
      cameraTarget.current.targetLookAt.set(...config.lookAt);
      cameraTarget.current.targetFov = config.fov;
    }
  }, [currentZone]);

  useEffect(() => {
    if (!containerRef.current) return;

    // SCENE SETUP
    const scene = new THREE.Scene();
    sceneRef.current = scene;
    scene.background = new THREE.Color(0x0a0c10);
    scene.fog = new THREE.FogExp2(0x0a0c10, 0.28);

    // CAMERA SETUP
    const width = containerRef.current.clientWidth;
    const height = containerRef.current.clientHeight;
    const camera = new THREE.PerspectiveCamera(46, width / height, 0.05, 20);
    camera.position.set(0.35, 0.85, 1.9);
    camera.lookAt(0, 0.35, 0);
    cameraRef.current = camera;

    // RENDERER SETUP
    const renderer = new THREE.WebGLRenderer({ antialias: true, alpha: false, powerPreference: 'high-performance' });
    renderer.setSize(width, height);
    renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
    renderer.shadowMap.enabled = true;
    renderer.shadowMap.type = THREE.PCFSoftShadowMap;
    rendererRef.current = renderer;
    containerRef.current.appendChild(renderer.domElement);

    // LIGHTING
    const ambientLight = new THREE.AmbientLight(0xdde5f0, 0.9);
    scene.add(ambientLight);

    const keyLight = new THREE.DirectionalLight(0xfff6ea, 2.2);
    keyLight.position.set(1.4, 2.2, 1.5);
    keyLight.castShadow = true;
    keyLight.shadow.mapSize.width = 1024;
    keyLight.shadow.mapSize.height = 1024;
    keyLight.shadow.bias = -0.0005;
    scene.add(keyLight);

    const rimLight = new THREE.DirectionalLight(0x70a0ff, 1.4);
    rimLight.position.set(-1.8, 1.5, -1.2);
    scene.add(rimLight);

    const deskLight = new THREE.PointLight(0xffeedd, 1.5, 3.5);
    deskLight.position.set(0, 1.1, 0.4);
    scene.add(deskLight);

    // WORKBENCH & DESK
    const tableGeo = new THREE.BoxGeometry(2.4, 0.08, 1.6);
    const tableMat = new THREE.MeshStandardMaterial({
      color: 0x1f242d,
      roughness: 0.7,
      metalness: 0.15,
    });
    const tableMesh = new THREE.Mesh(tableGeo, tableMat);
    tableMesh.position.set(0, -0.04, 0);
    tableMesh.receiveShadow = true;
    scene.add(tableMesh);

    // Anti-static work mat
    const matGeo = new THREE.BoxGeometry(1.4, 0.005, 0.9);
    const matMat = new THREE.MeshStandardMaterial({
      color: 0x1a2e3b, // Tech blue silicone mat
      roughness: 0.85,
      metalness: 0.05,
    });
    const matMesh = new THREE.Mesh(matGeo, matMat);
    matMesh.position.set(0, 0.002, 0.05);
    matMesh.receiveShadow = true;
    scene.add(matMesh);

    // ==========================================
    // CHASSIS (ApexForge Obsidian 500 ATX)
    // ==========================================
    const chassisGroup = new THREE.Group();
    chassisGroup.position.set(0, 0.005, 0);

    const caseFrameMat = new THREE.MeshStandardMaterial({
      color: 0x14161a,
      roughness: 0.5,
      metalness: 0.85,
    });

    // Rear Wall
    const rearWall = new THREE.Mesh(new THREE.BoxGeometry(0.02, 0.54, 0.48), caseFrameMat);
    rearWall.position.set(-0.21, 0.27, 0);
    rearWall.castShadow = true;
    chassisGroup.add(rearWall);

    // Motherboard Tray (Backplate)
    const tray = new THREE.Mesh(new THREE.BoxGeometry(0.42, 0.54, 0.02), caseFrameMat);
    tray.position.set(0, 0.27, -0.21);
    tray.receiveShadow = true;
    chassisGroup.add(tray);

    // Top Roof
    const roof = new THREE.Mesh(new THREE.BoxGeometry(0.44, 0.02, 0.44), caseFrameMat);
    roof.position.set(0, 0.54, 0);
    roof.castShadow = true;
    chassisGroup.add(roof);

    // PSU Shroud (Basement)
    const shroudMat = new THREE.MeshStandardMaterial({
      color: 0x181a1f,
      roughness: 0.6,
      metalness: 0.7,
    });
    const shroud = new THREE.Mesh(new THREE.BoxGeometry(0.42, 0.12, 0.42), shroudMat);
    shroud.position.set(0, 0.06, 0);
    shroud.receiveShadow = true;
    chassisGroup.add(shroud);

    // Front Panel with Mesh cutouts
    const frontPanel = new THREE.Mesh(new THREE.BoxGeometry(0.02, 0.54, 0.46), caseFrameMat);
    frontPanel.position.set(0.21, 0.27, 0);
    chassisGroup.add(frontPanel);

    // Tempered Glass Side Panel (Removable)
    const glassMat = new THREE.MeshPhysicalMaterial({
      color: 0x222a35,
      transparent: true,
      opacity: 0.45,
      roughness: 0.1,
      metalness: 0.1,
      transmission: 0.6,
      ior: 1.5,
    });
    const glassPanel = new THREE.Mesh(new THREE.BoxGeometry(0.44, 0.42, 0.01), glassMat);
    glassPanel.position.set(0, 0.33, 0.21);
    sidePanelMeshRef.current = glassPanel;
    chassisGroup.add(glassPanel);

    // ==========================================
    // MOTHERBOARD (ApexForge A870-M ATX)
    // ==========================================
    const moboGroup = new THREE.Group();
    moboGroup.position.set(0, 0.34, -0.19); // Mounted onto tray

    // Matte Black PCB
    const pcbMat = new THREE.MeshStandardMaterial({
      color: 0x121417,
      roughness: 0.6,
      metalness: 0.2,
    });
    const moboPCB = new THREE.Mesh(new THREE.BoxGeometry(0.305, 0.244, 0.004), pcbMat);
    moboPCB.receiveShadow = true;
    moboGroup.add(moboPCB);

    // Brushed Aluminum VRM Heatsinks
    const heatsinkMat = new THREE.MeshStandardMaterial({
      color: 0x2b2e35,
      roughness: 0.35,
      metalness: 0.9,
    });
    const vrmTop = new THREE.Mesh(new THREE.BoxGeometry(0.12, 0.03, 0.02), heatsinkMat);
    vrmTop.position.set(-0.06, 0.1, 0.012);
    moboGroup.add(vrmTop);

    const vrmLeft = new THREE.Mesh(new THREE.BoxGeometry(0.035, 0.11, 0.02), heatsinkMat);
    vrmLeft.position.set(-0.12, 0.04, 0.012);
    moboGroup.add(vrmLeft);

    // Chipset Heatsink
    const chipset = new THREE.Mesh(new THREE.BoxGeometry(0.05, 0.05, 0.015), heatsinkMat);
    chipset.position.set(0.09, -0.07, 0.01);
    moboGroup.add(chipset);

    // CPU Socket Area (LGA base)
    const socketBase = new THREE.Mesh(
      new THREE.BoxGeometry(0.056, 0.056, 0.005),
      new THREE.MeshStandardMaterial({ color: 0x24272c, roughness: 0.7, metalness: 0.5 })
    );
    socketBase.position.set(-0.05, 0.04, 0.005);
    moboGroup.add(socketBase);

    // Socket Retention Lever & Plate
    const leverGroup = new THREE.Group();
    leverGroup.position.set(-0.02, 0.04, 0.008);
    const leverBar = new THREE.Mesh(
      new THREE.CylinderGeometry(0.0015, 0.0015, 0.055),
      new THREE.MeshStandardMaterial({ color: 0xcccccc, metalness: 0.95, roughness: 0.2 })
    );
    leverBar.rotation.z = Math.PI / 2;
    leverGroup.add(leverBar);
    cpuSocketLeverRef.current = leverGroup;
    moboGroup.add(leverGroup);

    // CPU (NovaCore N7-7600)
    const cpuGroup = new THREE.Group();
    cpuGroup.position.set(-0.05, 0.04, 0.008);

    const cpuPcb = new THREE.Mesh(
      new THREE.BoxGeometry(0.045, 0.045, 0.002),
      new THREE.MeshStandardMaterial({ color: 0x0f552b, roughness: 0.5 }) // Green substrate
    );
    cpuGroup.add(cpuPcb);

    const cpuIhs = new THREE.Mesh(
      new THREE.BoxGeometry(0.038, 0.038, 0.003),
      new THREE.MeshStandardMaterial({ color: 0xd0d5dd, metalness: 0.95, roughness: 0.15 }) // Nickel plated copper
    );
    cpuIhs.position.set(0, 0, 0.0025);
    cpuGroup.add(cpuIhs);

    // Alignment Triangle Gold Dot
    const tri = new THREE.Mesh(
      new THREE.BoxGeometry(0.003, 0.003, 0.001),
      new THREE.MeshStandardMaterial({ color: 0xffd700, metalness: 0.9 })
    );
    tri.position.set(-0.017, -0.017, 0.004);
    cpuGroup.add(tri);
    cpuMeshRef.current = cpuGroup;
    moboGroup.add(cpuGroup);

    // Thermal Paste Dot
    const pasteMat = new THREE.MeshStandardMaterial({
      color: 0x8a929e, // Thermal compound grey
      roughness: 0.9,
      metalness: 0.1,
    });
    const pasteMesh = new THREE.Mesh(new THREE.CylinderGeometry(0.007, 0.008, 0.003, 16), pasteMat);
    pasteMesh.position.set(-0.05, 0.04, 0.013);
    pasteMesh.rotation.x = Math.PI / 2;
    pasteMesh.visible = false;
    thermalPasteMeshRef.current = pasteMesh;
    moboGroup.add(pasteMesh);

    // CPU Cooler (ArcticBreeze A-4)
    const coolerGroup = new THREE.Group();
    coolerGroup.position.set(-0.05, 0.04, 0.015);

    // Aluminum Fin Stack
    const finStack = new THREE.Mesh(
      new THREE.BoxGeometry(0.08, 0.08, 0.07),
      new THREE.MeshStandardMaterial({ color: 0xb5bcc7, metalness: 0.85, roughness: 0.3 })
    );
    finStack.position.set(0, 0, 0.04);
    finStack.castShadow = true;
    coolerGroup.add(finStack);

    // 120mm Black Fan with Rotatable Blades
    const fanFrame = new THREE.Mesh(
      new THREE.BoxGeometry(0.082, 0.082, 0.015),
      new THREE.MeshStandardMaterial({ color: 0x111315, roughness: 0.6 })
    );
    fanFrame.position.set(0, 0, 0.08);
    coolerGroup.add(fanFrame);

    const bladesGroup = new THREE.Group();
    bladesGroup.position.set(0, 0, 0.08);
    for (let i = 0; i < 7; i++) {
      const blade = new THREE.Mesh(
        new THREE.BoxGeometry(0.03, 0.008, 0.002),
        new THREE.MeshStandardMaterial({ color: 0x22252a, roughness: 0.5 })
      );
      blade.rotation.z = (i * Math.PI * 2) / 7;
      bladesGroup.add(blade);
    }
    coolerGroup.add(bladesGroup);
    fanBladesRef.current.push(bladesGroup);
    coolerMeshRef.current = coolerGroup;
    coolerGroup.visible = false;
    moboGroup.add(coolerGroup);

    // 4x DDR5 Slots
    const slotDdrMat = new THREE.MeshStandardMaterial({ color: 0x181a1f, roughness: 0.7 });
    for (let i = 0; i < 4; i++) {
      const ddrSlot = new THREE.Mesh(new THREE.BoxGeometry(0.006, 0.13, 0.006), slotDdrMat);
      ddrSlot.position.set(0.035 + i * 0.012, 0.04, 0.005);
      moboGroup.add(ddrSlot);
    }

    // TitanRAM DDR5 Stick (In Slot 2)
    const ramGroup = new THREE.Group();
    ramGroup.position.set(0.047, 0.04, 0.012);

    const ramPcb = new THREE.Mesh(
      new THREE.BoxGeometry(0.003, 0.13, 0.032),
      new THREE.MeshStandardMaterial({ color: 0x1a1c22, roughness: 0.4, metalness: 0.8 })
    );
    ramGroup.add(ramPcb);

    // RGB Diffuser Bar
    const rgbBar = new THREE.Mesh(
      new THREE.BoxGeometry(0.004, 0.125, 0.004),
      new THREE.MeshStandardMaterial({
        color: 0x00f0ff,
        emissive: 0x0088aa,
        emissiveIntensity: 0.4,
      })
    );
    rgbBar.position.set(0, 0, 0.018);
    ramGroup.add(rgbBar);
    ramMeshRef.current = ramGroup;
    ramGroup.visible = false;
    moboGroup.add(ramGroup);

    // M.2 NVMe Slot and SSD (HyperDrive Gen5)
    const m2SlotMesh = new THREE.Mesh(
      new THREE.BoxGeometry(0.022, 0.008, 0.005),
      new THREE.MeshStandardMaterial({ color: 0x111111, roughness: 0.8 })
    );
    m2SlotMesh.position.set(0, -0.01, 0.005);
    moboGroup.add(m2SlotMesh);

    const m2Group = new THREE.Group();
    m2Group.position.set(0, -0.045, 0.006);
    const m2Pcb = new THREE.Mesh(
      new THREE.BoxGeometry(0.022, 0.075, 0.002),
      new THREE.MeshStandardMaterial({ color: 0x1a221f, roughness: 0.5 })
    );
    m2Group.add(m2Pcb);
    const m2Heatsink = new THREE.Mesh(
      new THREE.BoxGeometry(0.02, 0.07, 0.004),
      new THREE.MeshStandardMaterial({ color: 0x3a404a, metalness: 0.9, roughness: 0.3 })
    );
    m2Heatsink.position.set(0, 0, 0.003);
    m2Group.add(m2Heatsink);
    m2MeshRef.current = m2Group;
    m2Group.visible = false;
    moboGroup.add(m2Group);

    // PCIe 5.0 x16 Slot & GPU (VectorX VX-780)
    const pcieSlotMesh = new THREE.Mesh(
      new THREE.BoxGeometry(0.14, 0.008, 0.007),
      new THREE.MeshStandardMaterial({ color: 0x181a1f, roughness: 0.7 })
    );
    pcieSlotMesh.position.set(-0.03, -0.04, 0.006);
    moboGroup.add(pcieSlotMesh);

    const gpuGroup = new THREE.Group();
    gpuGroup.position.set(-0.02, -0.04, 0.055);

    // Dual Slot Shroud
    const gpuShroud = new THREE.Mesh(
      new THREE.BoxGeometry(0.24, 0.04, 0.09),
      new THREE.MeshStandardMaterial({ color: 0x1c1f26, roughness: 0.4, metalness: 0.7 })
    );
    gpuShroud.castShadow = true;
    gpuGroup.add(gpuShroud);

    // Metal Backplate
    const gpuBackplate = new THREE.Mesh(
      new THREE.BoxGeometry(0.235, 0.004, 0.088),
      new THREE.MeshStandardMaterial({ color: 0x2f3540, metalness: 0.9, roughness: 0.25 })
    );
    gpuBackplate.position.set(0, 0.021, 0);
    gpuGroup.add(gpuBackplate);

    // Dual Fans
    [-0.055, 0.055].forEach((offset) => {
      const gpuFan = new THREE.Group();
      gpuFan.position.set(offset, -0.021, 0);
      for (let j = 0; j < 9; j++) {
        const b = new THREE.Mesh(
          new THREE.BoxGeometry(0.022, 0.002, 0.006),
          new THREE.MeshStandardMaterial({ color: 0x111317 })
        );
        b.rotation.y = (j * Math.PI * 2) / 9;
        gpuFan.add(b);
      }
      gpuGroup.add(gpuFan);
      fanBladesRef.current.push(gpuFan);
    });

    gpuMeshRef.current = gpuGroup;
    gpuGroup.visible = false;
    moboGroup.add(gpuGroup);

    // 4 Diagnostic LEDs on top right
    const ledGeo = new THREE.BoxGeometry(0.002, 0.002, 0.002);
    const ledCpu = new THREE.Mesh(ledGeo, new THREE.MeshStandardMaterial({ color: 0x220000 }));
    ledCpu.position.set(0.13, 0.1, 0.006);
    moboGroup.add(ledCpu);
    ledCpuRef.current = ledCpu;

    const ledDram = new THREE.Mesh(ledGeo, new THREE.MeshStandardMaterial({ color: 0x221100 }));
    ledDram.position.set(0.135, 0.1, 0.006);
    moboGroup.add(ledDram);
    ledDramRef.current = ledDram;

    const ledVga = new THREE.Mesh(ledGeo, new THREE.MeshStandardMaterial({ color: 0x111111 }));
    ledVga.position.set(0.14, 0.1, 0.006);
    moboGroup.add(ledVga);
    ledVgaRef.current = ledVga;

    const ledBoot = new THREE.Mesh(ledGeo, new THREE.MeshStandardMaterial({ color: 0x002200 }));
    ledBoot.position.set(0.145, 0.1, 0.006);
    moboGroup.add(ledBoot);
    ledBootRef.current = ledBoot;

    // Sleeved Power Cables
    // 1. 24-Pin ATX Cable Bundle
    const cable24Group = new THREE.Group();
    const cableCurve24 = new THREE.CubicBezierCurve3(
      new THREE.Vector3(0.14, -0.06, 0.02),
      new THREE.Vector3(0.18, -0.06, 0.08),
      new THREE.Vector3(0.18, 0.04, 0.12),
      new THREE.Vector3(0.145, 0.04, 0.015)
    );
    const tubeGeo24 = new THREE.TubeGeometry(cableCurve24, 20, 0.008, 8, false);
    const cableMat = new THREE.MeshStandardMaterial({ color: 0x1b1d22, roughness: 0.8 });
    const tube24 = new THREE.Mesh(tubeGeo24, cableMat);
    cable24Group.add(tube24);
    cable24PinRef.current = cable24Group;
    cable24Group.visible = false;
    moboGroup.add(cable24Group);

    // 2. CPU EPS 8-Pin Cable
    const cableCpuGroup = new THREE.Group();
    const cableCurveCpu = new THREE.CubicBezierCurve3(
      new THREE.Vector3(-0.11, 0.15, 0.08),
      new THREE.Vector3(-0.12, 0.14, 0.05),
      new THREE.Vector3(-0.12, 0.12, 0.03),
      new THREE.Vector3(-0.115, 0.11, 0.015)
    );
    const tubeGeoCpu = new THREE.TubeGeometry(cableCurveCpu, 16, 0.005, 8, false);
    const tubeCpu = new THREE.Mesh(tubeGeoCpu, cableMat);
    cableCpuGroup.add(tubeCpu);
    cableCpuEpsRef.current = cableCpuGroup;
    cableCpuGroup.visible = false;
    moboGroup.add(cableCpuGroup);

    // 3. PCIe GPU Cable
    const cablePcieGroup = new THREE.Group();
    const cableCurveGpu = new THREE.CubicBezierCurve3(
      new THREE.Vector3(0.08, -0.15, 0.05),
      new THREE.Vector3(0.08, -0.08, 0.12),
      new THREE.Vector3(0.08, -0.04, 0.1),
      new THREE.Vector3(0.06, -0.04, 0.06)
    );
    const tubeGeoGpu = new THREE.TubeGeometry(cableCurveGpu, 16, 0.006, 8, false);
    const tubeGpu = new THREE.Mesh(tubeGeoGpu, cableMat);
    cablePcieGroup.add(tubeGpu);
    cablePcieRef.current = cablePcieGroup;
    cablePcieGroup.visible = false;
    moboGroup.add(cablePcieGroup);

    chassisGroup.add(moboGroup);
    scene.add(chassisGroup);

    // ANIMATION & RENDER LOOP
    let animationFrameId: number;
    const currentCamTarget = cameraTarget.current;

    const animate = () => {
      animationFrameId = requestAnimationFrame(animate);

      // Smooth camera interpolation
      currentCamTarget.currentPos.lerp(currentCamTarget.targetPos, 0.07);
      currentCamTarget.currentLookAt.lerp(currentCamTarget.targetLookAt, 0.07);
      currentCamTarget.currentFov += (currentCamTarget.targetFov - currentCamTarget.currentFov) * 0.07;

      camera.position.copy(currentCamTarget.currentPos);
      camera.lookAt(currentCamTarget.currentLookAt);
      camera.fov = currentCamTarget.currentFov;
      camera.updateProjectionMatrix();

      // Spin fans if system is powered on
      if (buildState.systemPowered) {
        fanBladesRef.current.forEach((fan) => {
          fan.rotation.z += 0.28;
          fan.rotation.y += 0.28;
        });
      }

      renderer.render(scene, camera);
    };

    animate();

    // RESIZE LISTENER
    const handleResize = () => {
      if (!containerRef.current || !rendererRef.current || !cameraRef.current) return;
      const w = containerRef.current.clientWidth;
      const h = containerRef.current.clientHeight;
      cameraRef.current.aspect = w / h;
      cameraRef.current.updateProjectionMatrix();
      rendererRef.current.setSize(w, h);
    };

    window.addEventListener('resize', handleResize);

    return () => {
      window.removeEventListener('resize', handleResize);
      cancelAnimationFrame(animationFrameId);
      if (rendererRef.current && rendererRef.current.domElement) {
        rendererRef.current.domElement.remove();
        rendererRef.current.dispose();
      }
    };
  }, []);

  // Update visible 3D components according to BuildState
  useEffect(() => {
    if (sidePanelMeshRef.current) {
      sidePanelMeshRef.current.visible = !buildState.sidePanelRemoved;
    }

    if (cpuMeshRef.current) {
      cpuMeshRef.current.visible = buildState.cpuInstalled;
    }

    if (cpuSocketLeverRef.current) {
      // Rotate lever open vs closed
      cpuSocketLeverRef.current.rotation.x = buildState.cpuSocketOpen ? -Math.PI / 3 : 0;
    }

    if (thermalPasteMeshRef.current) {
      thermalPasteMeshRef.current.visible = buildState.thermalPasteApplied;
      const scale = Math.max(0.6, buildState.thermalPasteQuality * 1.4);
      thermalPasteMeshRef.current.scale.set(scale, 1, scale);
    }

    if (coolerMeshRef.current) {
      coolerMeshRef.current.visible = buildState.coolerInstalled;
    }

    if (ramMeshRef.current) {
      ramMeshRef.current.visible = buildState.ramInstalled;
    }

    if (m2MeshRef.current) {
      m2MeshRef.current.visible = buildState.m2Installed;
    }

    if (gpuMeshRef.current) {
      gpuMeshRef.current.visible = buildState.gpuInstalled;
    }

    if (cable24PinRef.current) {
      cable24PinRef.current.visible = buildState.cable24PinConnected;
    }

    if (cableCpuEpsRef.current) {
      cableCpuEpsRef.current.visible = buildState.cableCpuEpsConnected;
    }

    if (cablePcieRef.current) {
      cablePcieRef.current.visible = buildState.cablePcieConnected;
    }

    // Diagnostic LEDs based on POST State
    if (ledCpuRef.current && ledDramRef.current && ledVgaRef.current && ledBootRef.current) {
      const matCpu = ledCpuRef.current.material as THREE.MeshStandardMaterial;
      const matDram = ledDramRef.current.material as THREE.MeshStandardMaterial;
      const matVga = ledVgaRef.current.material as THREE.MeshStandardMaterial;
      const matBoot = ledBootRef.current.material as THREE.MeshStandardMaterial;

      matCpu.color.setHex(0x220000);
      matDram.color.setHex(0x221100);
      matVga.color.setHex(0x111111);
      matBoot.color.setHex(0x002200);

      if (buildState.postState === 'CPUCheck') {
        matCpu.color.setHex(0xff1111);
      } else if (buildState.postState === 'MemoryCheck') {
        matDram.color.setHex(0xffaa00);
      } else if (buildState.postState === 'GPUCheck') {
        matVga.color.setHex(0xffffff);
      } else if (buildState.postState === 'BootReady') {
        matBoot.color.setHex(0x00ff44);
      } else if (buildState.postState === 'POSTFailure') {
        if (buildState.faultCode === 'CPU_POWER_MISSING' || buildState.faultCode === 'CPU_NOT_INSTALLED') {
          matCpu.color.setHex(0xff0000);
        } else if (buildState.faultCode === 'DRAM_NOT_SEATED') {
          matDram.color.setHex(0xff8800);
        } else if (buildState.faultCode === 'GPU_POWER_MISSING') {
          matVga.color.setHex(0xffffff);
        }
      }
    }
  }, [buildState]);

  return (
    <div className="relative w-full h-full select-none overflow-hidden">
      {/* 3D WebGL Canvas Container */}
      <div ref={containerRef} className="w-full h-full cursor-grab active:cursor-grabbing" />

      {/* Floating Focus Zone Selector Pills (Desktop & Mobile quick jump) */}
      <div className="absolute top-4 left-4 z-10 flex flex-wrap gap-1.5 p-1 bg-neutral-900/80 backdrop-blur-md rounded-xl border border-neutral-800 shadow-xl max-w-[90vw]">
        {(
          [
            ['Workshop', 'Workshop'],
            ['Assembly', 'Assembly'],
            ['CPUSocket', 'CPU'],
            ['RAMSlots', 'RAM'],
            ['PCIeSlot', 'GPU'],
            ['M2Slot', 'M.2'],
            ['PowerArea', 'Power'],
            ['PSUBasement', 'PSU'],
          ] as [FocusZoneType, string][]
        ).map(([zone, label]) => {
          const isActive = currentZone === zone;
          return (
            <button
              key={zone}
              onClick={() => onZoneSelect(zone)}
              className={`px-3 py-1.5 text-xs font-semibold rounded-lg transition-all ${
                isActive
                  ? 'bg-neutral-100 text-neutral-950 shadow-md scale-102'
                  : 'text-neutral-400 hover:text-neutral-200 hover:bg-neutral-800/60'
              }`}
            >
              {label}
            </button>
          );
        })}
      </div>

      {/* Subtle Focus Framing Watermark */}
      <div className="absolute top-4 right-4 z-10 flex items-center gap-2 bg-neutral-900/80 backdrop-blur-md px-3 py-1.5 rounded-lg border border-neutral-800 text-xs font-medium text-neutral-400">
        <span className="w-2 h-2 rounded-full bg-emerald-500 animate-pulse" />
        <span>Zone: {currentZone}</span>
      </div>
    </div>
  );
};
