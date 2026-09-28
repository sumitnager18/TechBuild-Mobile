import { CompatibilityResult, HardwareSpec } from '../types';

export const HARDWARE_CATALOG = {
  chassis: {
    id: 'case_01',
    name: 'Obsidian 500 ATX',
    brand: 'ApexForge',
    category: 'Chassis',
    details: {
      formFactor: 'ATX Mid-Tower',
      maxGpuLength: 360,
      sidePanel: 'Tempered Glass',
    },
  } as HardwareSpec,
  motherboard: {
    id: 'mobo_01',
    name: 'ApexForge A870-M',
    brand: 'ApexForge',
    category: 'Motherboard',
    details: {
      socket: 'AM5_FICTIONAL',
      memoryType: 'DDR5',
      dimmSlots: 4,
      pcie16Slots: 1,
      m2Slots: 2,
    },
  } as HardwareSpec,
  cpu: {
    id: 'cpu_01',
    name: 'NovaCore N7-7600',
    brand: 'NovaCore',
    category: 'CPU',
    details: {
      socket: 'AM5_FICTIONAL',
      cores: 8,
      threads: 16,
      tdpWatts: 105,
      boostClock: '5.2 GHz',
    },
  } as HardwareSpec,
  ram: {
    id: 'ram_01',
    name: 'TitanRAM DDR5-32',
    brand: 'TitanRAM',
    category: 'RAM',
    details: {
      memoryType: 'DDR5',
      capacityGb: 32,
      speedMhz: 6000,
      cl: 30,
    },
  } as HardwareSpec,
  gpu: {
    id: 'gpu_01',
    name: 'VectorX VX-780',
    brand: 'VectorX',
    category: 'GPU',
    details: {
      vramGb: 16,
      tdpWatts: 220,
      requiredPcie8Pin: 1,
      lengthMm: 285,
    },
  } as HardwareSpec,
  storage: {
    id: 'ssd_01',
    name: 'HyperDrive Gen5 2TB',
    brand: 'HyperDrive',
    category: 'Storage',
    details: {
      formFactor: 'M.2 2280',
      interface: 'NVMe PCIe 4.0',
      capacityGb: 2000,
      readSpeedMb: 7000,
    },
  } as HardwareSpec,
  cooler: {
    id: 'cooler_01',
    name: 'ArcticBreeze A-4 Dual-Tower',
    brand: 'ArcticBreeze',
    category: 'Cooler',
    details: {
      supportedSocket: 'AM5_FICTIONAL',
      maxTdpWatts: 180,
      screwCount: 4,
      fanHeader: 'PWM 4-Pin',
    },
  } as HardwareSpec,
  psu: {
    id: 'psu_01',
    name: 'VoltEdge 750 Bronze',
    brand: 'VoltEdge',
    category: 'PSU',
    details: {
      wattage: 750,
      efficiency: '80 Plus Bronze',
      isModular: true,
    },
  } as HardwareSpec,
};

export class CompatibilityEngineTS {
  public static validateCPU(cpuSocket: string, moboSocket: string): CompatibilityResult {
    if (cpuSocket !== moboSocket) {
      return {
        isCompatible: false,
        errorCode: 'CPU_SOCKET_MISMATCH',
        userMessage: `CPU socket '${cpuSocket}' does not match motherboard socket '${moboSocket}'.`,
      };
    }
    return { isCompatible: true, errorCode: '', userMessage: 'Compatible.' };
  }

  public static validateRAM(ramType: string, moboType: string): CompatibilityResult {
    if (ramType !== moboType) {
      return {
        isCompatible: false,
        errorCode: 'RAM_TYPE_MISMATCH',
        userMessage: `RAM type '${ramType}' cannot be installed into '${moboType}' slots.`,
      };
    }
    return { isCompatible: true, errorCode: '', userMessage: 'Compatible.' };
  }

  public static validatePSU(psuWatts: number, estimatedSystemTdp: number): CompatibilityResult {
    if (psuWatts < estimatedSystemTdp) {
      return {
        isCompatible: false,
        errorCode: 'PSU_INSUFFICIENT_POWER',
        userMessage: `PSU capacity (${psuWatts}W) is lower than load demand (${estimatedSystemTdp}W).`,
      };
    }
    return { isCompatible: true, errorCode: '', userMessage: 'Compatible.' };
  }
}
