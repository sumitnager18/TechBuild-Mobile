export type ComponentCategory =
  | 'Chassis'
  | 'Motherboard'
  | 'CPU'
  | 'Cooler'
  | 'RAM'
  | 'GPU'
  | 'Storage'
  | 'PSU'
  | 'Cable'
  | 'Tool';

export type ComponentState =
  | 'Uninstalled'
  | 'PickedUp'
  | 'Aligned'
  | 'PartiallyInserted'
  | 'Inserted'
  | 'Seated'
  | 'Locked'
  | 'RetentionLocked'
  | 'PowerRequired'
  | 'PowerConnected';

export type FocusZoneType =
  | 'Workshop'
  | 'Assembly'
  | 'CPUSocket'
  | 'RAMSlots'
  | 'PCIeSlot'
  | 'M2Slot'
  | 'PowerArea'
  | 'PSUBasement';

export interface HardwareSpec {
  id: string;
  name: string;
  brand: string;
  category: ComponentCategory;
  details: Record<string, string | number | boolean>;
}

export type FaultSeverity = 'Info' | 'Warning' | 'Error' | 'Critical';

export type DiagnosticSubsystem =
  | 'General'
  | 'Power'
  | 'Thermal'
  | 'CPU'
  | 'Memory'
  | 'GPU'
  | 'Storage'
  | 'Motherboard'
  | 'Assembly';

export type FaultCode =
  | 'NONE'
  | 'CPU_NOT_INSTALLED'
  | 'CPU_SOCKET_MISMATCH'
  | 'CPU_POWER_MISSING'
  | 'DRAM_NOT_SEATED'
  | 'DRAM_TYPE_MISMATCH'
  | 'COOLER_NOT_MOUNTED'
  | 'COOLER_LOOSE_SCREWS'
  | 'FAN_HEADER_DISCONNECTED'
  | 'THERMAL_PASTE_MISSING'
  | 'THERMAL_PASTE_DEGRADED'
  | 'THERMAL_RUNAWAY'
  | 'GPU_NOT_SEATED'
  | 'GPU_POWER_MISSING'
  | 'PSU_INSUFFICIENT_WATTAGE'
  | 'PSU_MISSING_CABLES'
  | 'ATX_24PIN_DISCONNECTED'
  | 'STORAGE_NOT_FASTENED'
  | 'STORAGE_NOT_DETECTED';

export interface DiagnosticResult {
  faultCode: FaultCode;
  severity: FaultSeverity;
  subsystem: DiagnosticSubsystem;
  message: string;
  suggestedInspectionTarget: string;
}

export interface CompatibilityResult {
  isCompatible: boolean;
  errorCode: string;
  userMessage: string;
  severity?: FaultSeverity;
}

export type ThermalState = 'Cold' | 'Normal' | 'Warm' | 'Hot' | 'Throttling' | 'Critical';

export interface ThermalCalculationResult {
  temperatureCelsius: number;
  state: ThermalState;
  performanceMultiplier: number; // 0.0 to 1.0
  pasteEfficiency: number;
  mountQuality: number;
  diagnosticMessage: string;
  isEmergencyShutdown: boolean;
}

export interface PowerLoadProfile {
  continuousLoadWatts: number;
  peakLoadWatts: number;
  transientLoadWatts: number;
}

export type PSUStatus = 'Healthy' | 'Marginal' | 'Insufficient' | 'MissingConnector';

export interface PSUEvaluationResult {
  status: PSUStatus;
  ratedWattage: number;
  continuousHeadroomWatts: number;
  peakHeadroomWatts: number;
  message: string;
}

export type POSTState =
  | 'PowerOff'
  | 'PowerButtonPressed'
  | 'PowerStarting'
  | 'CPUCheck'
  | 'MemoryCheck'
  | 'GPUCheck'
  | 'StorageCheck'
  | 'BootReady'
  | 'POSTFailure';

export interface BuildState {
  sidePanelRemoved: boolean;
  cpuSocketOpen: boolean;
  cpuInstalled: boolean;
  cpuAlignedCorrectly: boolean;
  thermalPasteApplied: boolean;
  thermalPasteQuality: number; // 0.0 - 1.0 (pea dot size)
  coolerInstalled: boolean;
  coolerScrewsTightened: number; // 0 - 4
  coolerFanConnected: boolean;
  ramLatchOpen: boolean;
  ramInstalled: boolean;
  m2ScrewRemoved: boolean;
  m2Installed: boolean;
  m2Screwed: boolean;
  gpuBracketRemoved: boolean;
  gpuPcieLatchOpen: boolean;
  gpuInstalled: boolean;
  cable24PinConnected: boolean;
  cableCpuEpsConnected: boolean;
  cablePcieConnected: boolean;
  caseClosed: boolean;
  systemPowered: boolean;
  postState: POSTState;
  faultCode: FaultCode;
  faultMessage: string;
}

export interface SimulationSnapshotData {
  version: number;
  timestamp: number;
  cpuId: string;
  coolerId: string;
  gpuId: string;
  psuId: string;
  storageId: string;
  ramModuleIds: string[];
  cpuSocketLatched: boolean;
  thermalPasteAmount: number;
  coolerScrewsTightened: number;
  coolerFanConnected: boolean;
  ramLatched: boolean;
  gpuPcieLatched: boolean;
  connectedPowerRails: string[];
  powerSwitchOn: boolean;
}
