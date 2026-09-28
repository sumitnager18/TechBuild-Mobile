import {
  BuildState,
  DiagnosticResult,
  FaultCode,
  PowerLoadProfile,
  PSUEvaluationResult,
  ThermalCalculationResult,
  ThermalState,
} from '../types';

export class SimulationEngineTS {
  public static calculatePasteEfficiency(amount: number): number {
    if (amount <= 0.01) return 0.08;
    if (amount < 0.45) return 0.15 + (amount / 0.45) * 0.65;
    if (amount <= 0.7) {
      const distFromPeak = Math.abs(amount - 0.55);
      return 1.0 - distFromPeak * 0.35;
    }
    if (amount <= 1.0) return 0.92 - (amount - 0.7) * 0.25;
    const excess = amount - 1.0;
    return Math.max(0.68, 0.84 - excess * 0.28);
  }

  public static calculateThermals(
    cpuPowerWatts: number,
    coolerInstalled: boolean,
    coolerTdpRating: number,
    coolerScrewsTightened: number,
    fanConnected: boolean,
    pasteAmount: number,
    ambientTemp: number = 22.0
  ): ThermalCalculationResult {
    if (!coolerInstalled) {
      return {
        temperatureCelsius: 105.0,
        state: 'Critical',
        performanceMultiplier: 0.0,
        pasteEfficiency: 0.0,
        mountQuality: 0.0,
        diagnosticMessage: 'CRITICAL: No CPU cooler installed. Emergency thermal trip.',
        isEmergencyShutdown: true,
      };
    }

    const screwRatio = Math.min(1.0, Math.max(0.0, coolerScrewsTightened / 4.0));
    const mountQuality = Math.max(0.15, screwRatio);
    const activeCapacity = fanConnected ? coolerTdpRating : coolerTdpRating * 0.35;
    const pasteEfficiency = this.calculatePasteEfficiency(pasteAmount);

    const rCooler = (150.0 / Math.max(50.0, activeCapacity)) * 0.24;
    const rContact = 0.22 / (Math.max(0.08, pasteEfficiency) * Math.max(0.15, mountQuality));
    const effectiveResistance = rCooler + rContact;
    const finalTemp = ambientTemp + cpuPowerWatts * effectiveResistance;

    let state: ThermalState;
    let perfMultiplier: number;
    let message: string;

    if (finalTemp > 98.0) {
      state = 'Critical';
      perfMultiplier = 0.0;
      message = `CPU temperature (${finalTemp.toFixed(1)}°C) exceeds safe limits. Emergency halt.`;
    } else if (finalTemp >= 88.0) {
      state = 'Throttling';
      const tRatio = (finalTemp - 88.0) / (98.0 - 88.0);
      perfMultiplier = Math.max(0.6, 1.0 - tRatio * 0.4);
      message = `CPU thermal throttling at ${finalTemp.toFixed(1)}°C (${Math.round(perfMultiplier * 100)}% performance).`;
    } else if (finalTemp >= 78.0) {
      state = 'Hot';
      perfMultiplier = 1.0;
      message = `CPU operating hot at ${finalTemp.toFixed(1)}°C.`;
    } else if (finalTemp >= 65.0) {
      state = 'Warm';
      perfMultiplier = 1.0;
      message = `CPU in warm operating range (${finalTemp.toFixed(1)}°C).`;
    } else if (finalTemp >= 35.0) {
      state = 'Normal';
      perfMultiplier = 1.0;
      message = `CPU at optimal temperature (${finalTemp.toFixed(1)}°C).`;
    } else {
      state = 'Cold';
      perfMultiplier = 1.0;
      message = `CPU idling at ${finalTemp.toFixed(1)}°C.`;
    }

    return {
      temperatureCelsius: finalTemp,
      state,
      performanceMultiplier: perfMultiplier,
      pasteEfficiency,
      mountQuality,
      diagnosticMessage: message,
      isEmergencyShutdown: state === 'Critical',
    };
  }

  public static calculatePowerLoad(
    cpuTdp: number = 105,
    cpuPeak: number = 142,
    hasGpu: boolean = true,
    gpuTdp: number = 220,
    gpuTransient: number = 320
  ): PowerLoadProfile {
    const actualGpuTdp = hasGpu ? gpuTdp : 0;
    const actualGpuTransient = hasGpu ? gpuTransient : 0;
    const baseSystem = 60;

    const continuous = cpuTdp + actualGpuTdp + baseSystem;
    const peak = cpuPeak + actualGpuTdp * 1.15 + baseSystem + 25;
    const transient = cpuPeak + actualGpuTransient + baseSystem + 40;

    return {
      continuousLoadWatts: continuous,
      peakLoadWatts: peak,
      transientLoadWatts: transient,
    };
  }

  public static evaluatePSU(
    psuWattage: number,
    load: PowerLoadProfile,
    availablePcie8Pins: number,
    requiredPcie8Pins: number
  ): PSUEvaluationResult {
    if (psuWattage <= 0) {
      return {
        status: 'Insufficient',
        ratedWattage: 0,
        continuousHeadroomWatts: -load.continuousLoadWatts,
        peakHeadroomWatts: -load.peakLoadWatts,
        message: 'PSU is absent or zero wattage.',
      };
    }

    if (availablePcie8Pins < requiredPcie8Pins) {
      return {
        status: 'MissingConnector',
        ratedWattage: psuWattage,
        continuousHeadroomWatts: psuWattage - load.continuousLoadWatts,
        peakHeadroomWatts: psuWattage - load.peakLoadWatts,
        message: `PSU lacks required PCIe power connectors (${availablePcie8Pins} available, ${requiredPcie8Pins} required).`,
      };
    }

    const continuousHeadroom = psuWattage - load.continuousLoadWatts;
    const peakHeadroom = psuWattage * 1.1 - load.peakLoadWatts;

    if (continuousHeadroom < 0 || peakHeadroom < 0) {
      return {
        status: 'Insufficient',
        ratedWattage: psuWattage,
        continuousHeadroomWatts: continuousHeadroom,
        peakHeadroomWatts: peakHeadroom,
        message: `PSU wattage (${psuWattage}W) is insufficient for load (${load.continuousLoadWatts.toFixed(0)}W continuous).`,
      };
    }

    if (continuousHeadroom < psuWattage * 0.15) {
      return {
        status: 'Marginal',
        ratedWattage: psuWattage,
        continuousHeadroomWatts: continuousHeadroom,
        peakHeadroomWatts: peakHeadroom,
        message: `PSU wattage is marginal with low headroom (${continuousHeadroom.toFixed(0)}W remaining).`,
      };
    }

    return {
      status: 'Healthy',
      ratedWattage: psuWattage,
      continuousHeadroomWatts: continuousHeadroom,
      peakHeadroomWatts: peakHeadroom,
      message: 'PSU delivers adequate, stable power headroom.',
    };
  }

  public static evaluateBuild(build: BuildState): {
    canPost: boolean;
    load: PowerLoadProfile;
    psuResult: PSUEvaluationResult;
    thermals: ThermalCalculationResult;
    faults: DiagnosticResult[];
  } {
    const faults: DiagnosticResult[] = [];

    // Assembly checks
    if (!build.cpuInstalled) {
      faults.push({
        faultCode: 'CPU_NOT_INSTALLED',
        severity: 'Critical',
        subsystem: 'CPU',
        message: 'CPU is not installed in socket AM5.',
        suggestedInspectionTarget: 'CPUSocket',
      });
    }

    if (!build.ramInstalled) {
      faults.push({
        faultCode: 'DRAM_NOT_SEATED',
        severity: 'Critical',
        subsystem: 'Memory',
        message: 'No functional DDR5 memory installed in channel A2.',
        suggestedInspectionTarget: 'RAMSlot_A2',
      });
    }

    if (!build.coolerInstalled) {
      faults.push({
        faultCode: 'COOLER_NOT_MOUNTED',
        severity: 'Critical',
        subsystem: 'Thermal',
        message: 'CPU cooler is not mounted. System will overheat immediately.',
        suggestedInspectionTarget: 'CPUCooler',
      });
    } else if (build.coolerScrewsTightened < 4) {
      faults.push({
        faultCode: 'COOLER_LOOSE_SCREWS',
        severity: 'Warning',
        subsystem: 'Thermal',
        message: `Cooler mounting screws incomplete (${build.coolerScrewsTightened}/4 tightened).`,
        suggestedInspectionTarget: 'CoolerScrews',
      });
    }

    // Power checks
    if (!build.cable24PinConnected) {
      faults.push({
        faultCode: 'ATX_24PIN_DISCONNECTED',
        severity: 'Critical',
        subsystem: 'Power',
        message: 'Main ATX 24-Pin power cable is disconnected.',
        suggestedInspectionTarget: 'ATX_24Pin',
      });
    }

    if (build.cpuInstalled && !build.cableCpuEpsConnected) {
      faults.push({
        faultCode: 'CPU_POWER_MISSING',
        severity: 'Critical',
        subsystem: 'Power',
        message: 'CPU EPS 12V 8-Pin power cable is disconnected.',
        suggestedInspectionTarget: 'CPU_EPS',
      });
    }

    if (build.gpuInstalled && !build.cablePcieConnected) {
      faults.push({
        faultCode: 'GPU_POWER_MISSING',
        severity: 'Critical',
        subsystem: 'Power',
        message: 'VectorX VX-780 requires supplementary PCIe power.',
        suggestedInspectionTarget: 'PCIe_Power',
      });
    }

    // Power load & PSU
    const load = this.calculatePowerLoad(105, 142, build.gpuInstalled, 220, 320);
    const psuResult = this.evaluatePSU(750, load, 4, build.gpuInstalled ? 1 : 0);

    // Thermals
    const thermals = this.calculateThermals(
      105,
      build.coolerInstalled,
      180,
      build.coolerScrewsTightened,
      build.coolerFanConnected,
      build.thermalPasteQuality,
      22.0
    );

    if (thermals.isEmergencyShutdown) {
      faults.push({
        faultCode: 'THERMAL_RUNAWAY',
        severity: 'Critical',
        subsystem: 'Thermal',
        message: thermals.diagnosticMessage,
        suggestedInspectionTarget: 'CPUCooler',
      });
    }

    const canPost = faults.length === 0 && psuResult.status !== 'Insufficient';

    return {
      canPost,
      load,
      psuResult,
      thermals,
      faults,
    };
  }
}
