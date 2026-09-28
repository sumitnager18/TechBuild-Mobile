using System.Collections.Generic;
using PCTechnician.Diagnostics;
using PCTechnician.Hardware;
using PCTechnician.Power;
using PCTechnician.Thermal;

namespace PCTechnician.Simulation
{
    public static class SimulationEvaluator
    {
        public const string RAIL_24PIN = "Rail_ATX_24Pin";
        public const string RAIL_CPU_EPS = "Rail_CPU_EPS";
        public const string RAIL_PCIE_GPU = "Rail_PCIe_GPU";

        public static SimulationEvaluationResult Evaluate(
            SimulationSnapshot snapshot,
            MotherboardData mobo,
            CPUData cpu,
            RAMData ram,
            GPUData gpu,
            CoolerData cooler,
            PSUData psu,
            StorageData storage)
        {
            var result = new SimulationEvaluationResult();
            if (snapshot == null)
            {
                result.DiagnosticFaults.Add(DiagnosticResult.Critical(
                    DiagnosticFaultCode.HARDWARE_DATA_MISSING,
                    DiagnosticSubsystem.General,
                    "Simulation snapshot is missing.",
                    "SimulationSnapshot"));
                return result;
            }

            bool hasMobo = mobo != null && !string.IsNullOrEmpty(snapshot.MotherboardId);
            bool hasCpu = cpu != null && !string.IsNullOrEmpty(snapshot.CpuId);
            bool hasRam = ram != null && snapshot.RamModuleIds != null && snapshot.RamModuleIds.Count > 0;
            bool hasGpu = gpu != null && !string.IsNullOrEmpty(snapshot.GpuId);
            bool hasCooler = cooler != null && !string.IsNullOrEmpty(snapshot.CoolerId);
            bool hasPsu = psu != null && !string.IsNullOrEmpty(snapshot.PsuId);
            bool hasStorage = storage != null && !string.IsNullOrEmpty(snapshot.StorageId);

            if (!hasMobo)
                AddCritical(result, DiagnosticFaultCode.HARDWARE_DATA_MISSING, DiagnosticSubsystem.Motherboard,
                    "Motherboard data is missing or the snapshot has no motherboard ID.", "MotherboardTray");

            if (!hasCpu)
                AddCritical(result, DiagnosticFaultCode.CPU_NOT_INSTALLED, DiagnosticSubsystem.CPU,
                    "CPU is not installed or CPU specification data is missing.", "CPUSocket");
            else if (!snapshot.CpuSocketLatched)
                AddError(result, DiagnosticFaultCode.CPU_NOT_INSTALLED, DiagnosticSubsystem.CPU,
                    "CPU socket retention arm is not locked.", "CPUSocketLever");

            if (!hasRam)
                AddCritical(result, DiagnosticFaultCode.DRAM_NOT_SEATED, DiagnosticSubsystem.Memory,
                    "No functional DDR memory is installed or RAM specification data is missing.", "RAMSlot_A2");
            else if (!snapshot.RamLatched)
                AddError(result, DiagnosticFaultCode.DRAM_NOT_SEATED, DiagnosticSubsystem.Memory,
                    "RAM module is not fully seated; retention clip is unlatched.", "RAMSlot_A2");

            if (!hasCooler)
                AddCritical(result, DiagnosticFaultCode.COOLER_NOT_MOUNTED, DiagnosticSubsystem.Thermal,
                    "CPU cooler is not mounted or cooler specification data is missing.", "CPUCoolerMount");
            else
            {
                if (snapshot.CoolerScrewsTightened < cooler.MountingScrewCount)
                    AddWarning(result, DiagnosticFaultCode.COOLER_LOOSE_SCREWS, DiagnosticSubsystem.Thermal,
                        $"Cooler mounting screws are incomplete ({snapshot.CoolerScrewsTightened}/{cooler.MountingScrewCount} tightened).", "CoolerScrews");

                if (!snapshot.CoolerFanConnected)
                    AddWarning(result, DiagnosticFaultCode.FAN_HEADER_DISCONNECTED, DiagnosticSubsystem.Thermal,
                        "CPU cooler PWM fan header is disconnected.", "CPU_FAN_Header");
            }

            if (hasMobo && hasCpu)
                AddCompatibilityFault(result, CompatibilityEngine.ValidateCPUWithMotherboard(cpu, mobo),
                    DiagnosticFaultCode.CPU_SOCKET_MISMATCH, DiagnosticSubsystem.CPU, "CPUSocket");

            if (hasMobo && hasRam)
                AddCompatibilityFault(result, CompatibilityEngine.ValidateRAMWithMotherboard(ram, mobo),
                    DiagnosticFaultCode.DRAM_TYPE_MISMATCH, DiagnosticSubsystem.Memory, "RAMSlot_A2");

            if (hasMobo && hasGpu)
                AddCompatibilityFault(result, CompatibilityEngine.ValidateGPUWithMotherboard(gpu, mobo),
                    DiagnosticFaultCode.GPU_PCIE_MISMATCH, DiagnosticSubsystem.GPU, "PCIe_x16");

            if (hasMobo && hasStorage)
                AddCompatibilityFault(result, CompatibilityEngine.ValidateStorageWithMotherboard(storage, mobo),
                    DiagnosticFaultCode.STORAGE_COMPATIBILITY_MISMATCH, DiagnosticSubsystem.Storage, "M2Slot");

            if (hasCpu && hasCooler)
                AddCompatibilityFault(result, CompatibilityEngine.ValidateCoolerWithCPU(cooler, cpu),
                    DiagnosticFaultCode.COOLER_NOT_MOUNTED, DiagnosticSubsystem.Thermal, "CPUCoolerMount");

            result.IsAssembledCorrectly = result.DiagnosticFaults.Count == 0;

            var powerGraph = new PowerGraph();

            if (hasPsu)
            {
                for (int i = 0; i < psu.Atx24PinCount; i++)
                    powerGraph.AddSourceConnector(new PowerSourceConnector($"PSU_ATX24_{i + 1}", snapshot.PsuId, PowerConnectorType.ATX24Pin, 300f));

                for (int i = 0; i < psu.CpuEps8PinCount; i++)
                    powerGraph.AddSourceConnector(new PowerSourceConnector($"PSU_EPS8_{i + 1}", snapshot.PsuId, PowerConnectorType.CPUEPS8Pin, 280f));

                for (int i = 0; i < psu.Pcie8PinCount; i++)
                    powerGraph.AddSourceConnector(new PowerSourceConnector($"PSU_PCIE8_{i + 1}", snapshot.PsuId, PowerConnectorType.PCIe8Pin, 300f));
            }

            powerGraph.AddNode(new PowerNode(RAIL_24PIN, snapshot.PsuId, snapshot.MotherboardId, PowerConnectorType.ATX24Pin, 300, 75));
            if (hasCpu)
                powerGraph.AddNode(new PowerNode(RAIL_CPU_EPS, snapshot.PsuId, snapshot.CpuId, PowerConnectorType.CPUEPS8Pin, 280, cpu.TDPWatts));
            if (hasGpu)
                powerGraph.AddNode(new PowerNode(RAIL_PCIE_GPU, snapshot.PsuId, snapshot.GpuId, PowerConnectorType.PCIe8Pin, 300, gpu.TDPWatts));

            var explicitConnections = new HashSet<string>();
            if (snapshot.ConnectedPowerConnections != null)
            {
                foreach (var connection in snapshot.ConnectedPowerConnections)
                {
                    if (connection == null) continue;
                    if (explicitConnections.Contains(connection.RailId)) continue;
                    if (powerGraph.ConnectRail(connection.RailId, connection.SourceConnectorId, out var error))
                        explicitConnections.Add(connection.RailId);
                    else
                        AddError(result, DiagnosticFaultCode.PSU_SOURCE_CONNECTOR_MISSING, DiagnosticSubsystem.Power, error, connection.SourceConnectorId);
                }
            }

            // Legacy snapshots store only rail IDs. Resolve each to one unused physical connector.
            if (snapshot.ConnectedPowerRails != null)
            {
                foreach (var rail in snapshot.ConnectedPowerRails)
                {
                    if (explicitConnections.Contains(rail)) continue;

                    PowerConnectorType type;
                    if (rail == RAIL_24PIN) type = PowerConnectorType.ATX24Pin;
                    else if (rail == RAIL_CPU_EPS) type = PowerConnectorType.CPUEPS8Pin;
                    else if (rail == RAIL_PCIE_GPU) type = PowerConnectorType.PCIe8Pin;
                    else continue;

                    if (powerGraph.ConnectRail(rail, type, out var error))
                        explicitConnections.Add(rail);
                    else
                        AddError(result, DiagnosticFaultCode.PSU_SOURCE_CONNECTOR_MISSING, DiagnosticSubsystem.Power, error, rail);
                }
            }

            bool pwr24Connected = !hasMobo || powerGraph.IsRailConnected(RAIL_24PIN);
            bool pwrCpuConnected = !hasCpu || powerGraph.IsRailConnected(RAIL_CPU_EPS);
            bool pwrGpuConnected = !hasGpu || powerGraph.IsRailConnected(RAIL_PCIE_GPU);

            if (hasMobo && mobo.Requires24Pin && !pwr24Connected)
                AddCritical(result, DiagnosticFaultCode.ATX_24PIN_DISCONNECTED, DiagnosticSubsystem.Power,
                    "Main ATX 24-Pin power cable is disconnected or no compatible PSU connector is available.", "ATX_24Pin");

            if (hasCpu && !pwrCpuConnected)
                AddCritical(result, DiagnosticFaultCode.CPU_POWER_MISSING, DiagnosticSubsystem.Power,
                    "CPU EPS 12V 8-Pin power cable is disconnected or no compatible PSU connector is available.", "CPU_EPS");

            if (hasGpu && !pwrGpuConnected)
                AddCritical(result, DiagnosticFaultCode.GPU_POWER_MISSING, DiagnosticSubsystem.Power,
                    "GPU PCIe power cable is disconnected or no compatible PSU connector is available.", "PCIe_Power");

            result.IsGpuPowered = pwrGpuConnected;
            result.IsPoweredCorrectly = pwr24Connected && pwrCpuConnected && pwrGpuConnected;

            if (hasCpu)
            {
                int gpuTdp = hasGpu ? gpu.TDPWatts : 0;
                int gpuTransient = hasGpu ? gpu.PeakTransientWatts : 0;
                result.EstimatedLoad = PowerLoadProfile.Calculate(cpu.TDPWatts, cpu.PeakPowerWatts, gpuTdp, gpuTransient);

                if (hasPsu)
                {
                    result.PSUStatusResult = powerGraph.EvaluatePSU(
                        psu.Wattage,
                        psu.TransientExcursionPercentage,
                        result.EstimatedLoad,
                        psu.Pcie8PinCount,
                        hasGpu ? gpu.RequiredPcie8PinCables : 0);

                    if (result.PSUStatusResult.Status == PSUStatus.Insufficient)
                        AddError(result, DiagnosticFaultCode.PSU_INSUFFICIENT_WATTAGE, DiagnosticSubsystem.Power,
                            result.PSUStatusResult.Message, "PSU");
                    else if (result.PSUStatusResult.Status == PSUStatus.MissingConnector)
                        AddError(result, DiagnosticFaultCode.PSU_MISSING_CABLES, DiagnosticSubsystem.Power,
                            result.PSUStatusResult.Message, "PSU");
                }
                else
                {
                    AddCritical(result, DiagnosticFaultCode.HARDWARE_DATA_MISSING, DiagnosticSubsystem.Power,
                        "PSU specification data is missing; power capacity cannot be evaluated.", "PSU");
                }

                if (hasCooler)
                {
                    result.CPUThermalResult = ThermalModel.CalculateCPUThermals(
                        cpuPowerWatts: cpu.TDPWatts,
                        coolerInstalled: true,
                        coolerTdpRating: cooler.MaxTdpDissipationWatts,
                        coolerScrewsTightened: snapshot.CoolerScrewsTightened,
                        totalScrewsRequired: cooler.MountingScrewCount,
                        fanConnected: snapshot.CoolerFanConnected,
                        pasteAmount: snapshot.ThermalPasteAmount,
                        caseAirflowFactor: 1.0f,
                        ambientTemp: ThermalModelParameters.DefaultAmbientTempCelsius,
                        maxSafeTempCelsius: cpu.MaxSafeTempCelsius);

                    result.IsCpuThermallySafe = !result.CPUThermalResult.IsEmergencyShutdown;

                    if (result.CPUThermalResult.IsEmergencyShutdown)
                        AddCritical(result, DiagnosticFaultCode.THERMAL_RUNAWAY, DiagnosticSubsystem.Thermal,
                            result.CPUThermalResult.DiagnosticMessage, "CPUCooler");
                    else if (result.CPUThermalResult.State == ThermalState.Throttling)
                        AddWarning(result, DiagnosticFaultCode.THERMAL_PASTE_DEGRADED, DiagnosticSubsystem.Thermal,
                            result.CPUThermalResult.DiagnosticMessage, "ThermalPaste");
                }
                else
                {
                    result.IsCpuThermallySafe = false;
                }
            }

            result.CanPostSucceed = result.IsAssembledCorrectly
                && result.IsPoweredCorrectly
                && result.IsCpuThermallySafe
                && result.PSUStatusResult.CanSafelyPowerContinuous;

            return result;
        }

        private static void AddCompatibilityFault(
            SimulationEvaluationResult result,
            CompatibilityResult compatibility,
            DiagnosticFaultCode fallbackCode,
            DiagnosticSubsystem subsystem,
            string target)
        {
            if (compatibility.IsCompatible) return;
            result.DiagnosticFaults.Add(DiagnosticResult.Error(
                MapCompatibilityCode(compatibility.ErrorCode, fallbackCode),
                subsystem,
                compatibility.UserMessage,
                target));
        }

        private static DiagnosticFaultCode MapCompatibilityCode(string code, DiagnosticFaultCode fallback)
        {
            if (code == CompatibilityEngine.ERROR_CPU_SOCKET_MISMATCH) return DiagnosticFaultCode.CPU_SOCKET_MISMATCH;
            if (code == CompatibilityEngine.ERROR_RAM_TYPE_MISMATCH) return DiagnosticFaultCode.DRAM_TYPE_MISMATCH;
            if (code == CompatibilityEngine.ERROR_STORAGE_INTERFACE_MISMATCH ||
                code == CompatibilityEngine.ERROR_STORAGE_FORM_FACTOR_MISMATCH) return DiagnosticFaultCode.STORAGE_COMPATIBILITY_MISMATCH;
            if (code == CompatibilityEngine.ERROR_GPU_INTERFACE_MISMATCH ||
                code == CompatibilityEngine.ERROR_GPU_PCIE_GENERATION_MISMATCH) return DiagnosticFaultCode.GPU_PCIE_MISMATCH;
            return fallback;
        }

        private static void AddCritical(SimulationEvaluationResult result, DiagnosticFaultCode code, DiagnosticSubsystem subsystem, string message, string target)
            => result.DiagnosticFaults.Add(DiagnosticResult.Critical(code, subsystem, message, target));

        private static void AddError(SimulationEvaluationResult result, DiagnosticFaultCode code, DiagnosticSubsystem subsystem, string message, string target)
            => result.DiagnosticFaults.Add(DiagnosticResult.Error(code, subsystem, message, target));

        private static void AddWarning(SimulationEvaluationResult result, DiagnosticFaultCode code, DiagnosticSubsystem subsystem, string message, string target)
            => result.DiagnosticFaults.Add(DiagnosticResult.Warning(code, subsystem, message, target));
    }
}