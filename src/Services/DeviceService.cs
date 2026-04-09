using System.ComponentModel;
using System.Runtime.InteropServices;
using AudioFixer.Native;

namespace AudioFixer.Services;

public record DeviceStatus(string InstanceId, string FriendlyName, bool HasProblem, uint ProblemCode);

public sealed class DeviceService
{
    private static LogService? _log;

    public static void SetLogger(LogService log) => _log = log;

    private static void Log(string message) => _log?.Log(message);
    private static void LogError(string message) => _log?.LogError(message);

    public List<DeviceStatus> GetAmpDeviceStatuses()
    {
        return EnumerateDevicesWithStatus(
            DeviceConstants.GUID_DEVCLASS_MEDIA,
            DeviceConstants.CS35L56_NAME_PATTERN);
    }

    public bool HasCode43Problem()
    {
        return GetAmpDeviceStatuses().Any(d => d.HasProblem && d.ProblemCode == DeviceConstants.CM_PROB_FAILED_POST_START);
    }

    public (bool HasProblem, int AffectedCount, int TotalCount) GetProblemSummary()
    {
        var statuses = GetAmpDeviceStatuses();
        int affected = statuses.Count(d => d.HasProblem && d.ProblemCode == DeviceConstants.CM_PROB_FAILED_POST_START);
        foreach (var s in statuses)
        {
            Log($"DEVICE   {s.FriendlyName} [{s.InstanceId}] Problem={s.HasProblem} Code={s.ProblemCode}");
        }

        if (statuses.Count == 0)
        {
            Log("DEVICE   No CS35L56 devices found — treating as problem");
            return (true, 0, 0);
        }

        return (affected > 0, affected, statuses.Count);
    }

    public void FixAudioDevices()
    {
        Log("FIX      Finding Intel SST OED device...");
        var oedDevice = FindDevice(
            DeviceConstants.GUID_DEVCLASS_SYSTEM,
            DeviceConstants.INTEL_SST_OED_NAME_PATTERN);

        if (oedDevice == null)
        {
            LogError("FIX      Intel SST OED device not found");
            throw new InvalidOperationException(
                $"Could not find device matching \"{DeviceConstants.INTEL_SST_OED_NAME_PATTERN}\". " +
                "Ensure the Intel Smart Sound Technology OED device is present in Device Manager.");
        }

        var (hDevInfo, devInfoData, friendlyName) = oedDevice.Value;
        Log($"FIX      Found: {friendlyName}");

        var disableParams = SP_PROPCHANGE_PARAMS.Create(DeviceConstants.DICS_DISABLE);
        var enableParams = SP_PROPCHANGE_PARAMS.Create(DeviceConstants.DICS_ENABLE);
        uint paramSize = (uint)Marshal.SizeOf<SP_PROPCHANGE_PARAMS>();

        bool disabled = false;
        try
        {
            Log("FIX      Disabling device...");
            if (!SetupApi.SetupDiSetClassInstallParams(hDevInfo, ref devInfoData,
                    ref disableParams,
                    paramSize))
            {
                int err = Marshal.GetLastWin32Error();
                LogError($"SetupDiSetClassInstallParams(disable) failed: error 0x{err:X}");
                throw new Win32Exception(err, $"SetupDiSetClassInstallParams(disable) failed: error 0x{err:X}");
            }
            Log("FIX      SetupDiSetClassInstallParams(disable) succeeded");

            if (!SetupApi.SetupDiCallClassInstaller(DeviceConstants.DIF_PROPERTYCHANGE, hDevInfo, ref devInfoData))
            {
                int err = Marshal.GetLastWin32Error();
                LogError($"SetupDiCallClassInstaller(disable) failed: error 0x{err:X}");
                throw new Win32Exception(err, $"SetupDiCallClassInstaller(disable) failed: error 0x{err:X}");
            }
            Log("FIX      Device disabled successfully");
            disabled = true;

            Log("FIX      Waiting 2s for driver stack teardown...");
            Thread.Sleep(2000);

            Log("FIX      Re-enabling device...");
            if (!SetupApi.SetupDiSetClassInstallParams(hDevInfo, ref devInfoData,
                    ref enableParams,
                    paramSize))
            {
                int err = Marshal.GetLastWin32Error();
                LogError($"SetupDiSetClassInstallParams(enable) failed: error 0x{err:X}");
                throw new Win32Exception(err, $"SetupDiSetClassInstallParams(enable) failed: error 0x{err:X}");
            }
            Log("FIX      SetupDiSetClassInstallParams(enable) succeeded");

            if (!SetupApi.SetupDiCallClassInstaller(DeviceConstants.DIF_PROPERTYCHANGE, hDevInfo, ref devInfoData))
            {
                int err = Marshal.GetLastWin32Error();
                LogError($"SetupDiCallClassInstaller(enable) failed: error 0x{err:X}");
                throw new Win32Exception(err, $"SetupDiCallClassInstaller(enable) failed: error 0x{err:X}");
            }
            Log("FIX      Device re-enabled successfully");
        }
        catch (Exception) when (disabled)
        {
            // Enable failed after disable succeeded — try to re-enable as recovery
            LogError("FIX      Enable failed after disable, attempting recovery re-enable...");
            try
            {
                var recoveryParams = SP_PROPCHANGE_PARAMS.Create(DeviceConstants.DICS_ENABLE);
                if (!SetupApi.SetupDiSetClassInstallParams(hDevInfo, ref devInfoData, ref recoveryParams, paramSize))
                {
                    int err = Marshal.GetLastWin32Error();
                    LogError($"FIX      Recovery SetupDiSetClassInstallParams failed: error 0x{err:X}");
                }
                else if (!SetupApi.SetupDiCallClassInstaller(DeviceConstants.DIF_PROPERTYCHANGE, hDevInfo, ref devInfoData))
                {
                    int err = Marshal.GetLastWin32Error();
                    LogError($"FIX      Recovery SetupDiCallClassInstaller failed: error 0x{err:X}");
                }
                else
                {
                    Log("FIX      Recovery re-enable succeeded");
                }
            }
            catch (Exception rex)
            {
                LogError($"FIX      Recovery re-enable also failed: {rex.Message}");
            }
            throw; // rethrow original
        }
        finally
        {
            if (!SetupApi.SetupDiDestroyDeviceInfoList(hDevInfo))
            {
                int err = Marshal.GetLastWin32Error();
                LogError($"SetupDiDestroyDeviceInfoList failed: error 0x{err:X}");
            }
        }

        Thread.Sleep(3000);

        Log("FIX      Verifying devices...");
        var statuses = GetAmpDeviceStatuses();

        if (statuses.Count == 0)
        {
            LogError("FIX      No CS35L56 devices found after fix — devices did not re-enumerate");
            throw new InvalidOperationException(
                "No CS35L56 devices found after fix. Devices did not re-enumerate — a reboot may be required.");
        }

        var stillBroken = statuses.Where(d => d.HasProblem && d.ProblemCode == DeviceConstants.CM_PROB_FAILED_POST_START).ToList();
        if (stillBroken.Count > 0)
        {
            var names = string.Join(", ", stillBroken.Select(d => d.FriendlyName));
            LogError($"FIX      {stillBroken.Count} device(s) still broken: {names}");
            throw new InvalidOperationException(
                $"Fix was applied but {stillBroken.Count} device(s) still have Code 43: {names}");
        }

        Log($"FIX      All {statuses.Count} device(s) verified OK");
    }

    private List<DeviceStatus> EnumerateDevicesWithStatus(Guid classGuid, string namePattern)
    {
        var results = new List<DeviceStatus>();
        IntPtr hDevInfo = SetupApi.SetupDiGetClassDevs(
            in classGuid, null, IntPtr.Zero, DeviceConstants.DIGCF_PRESENT);

        if (hDevInfo == DeviceConstants.INVALID_HANDLE_VALUE)
        {
            int err = Marshal.GetLastWin32Error();
            LogError($"SetupDiGetClassDevs failed (error 0x{err:X})");
            throw new Win32Exception(err, $"SetupDiGetClassDevs failed (error 0x{err:X})");
        }

        try
        {
            var devInfoData = SP_DEVINFO_DATA.Create();
            for (uint i = 0; SetupApi.SetupDiEnumDeviceInfo(hDevInfo, i, ref devInfoData); i++)
            {
                string? friendlyName = SetupApi.GetDeviceFriendlyName(hDevInfo, ref devInfoData);
                if (friendlyName == null || !friendlyName.Contains(namePattern, StringComparison.OrdinalIgnoreCase))
                    continue;

                string instanceId = GetDeviceInstanceId(hDevInfo, ref devInfoData);

                uint cr = CfgMgr32.CM_Get_DevNode_Status(out uint status, out uint problemNumber, devInfoData.DevInst, 0);
                if (cr != DeviceConstants.CR_SUCCESS)
                {
                    LogError($"CM_Get_DevNode_Status failed for {friendlyName}: CR=0x{cr:X}");
                    continue;
                }

                bool hasProblem = (status & DeviceConstants.DN_HAS_PROBLEM) != 0;
                results.Add(new DeviceStatus(instanceId, friendlyName, hasProblem, hasProblem ? problemNumber : 0));
            }

            // Check if enumeration ended with an error other than NO_MORE_ITEMS
            int lastError = Marshal.GetLastWin32Error();
            if (lastError != 0 && lastError != 259) // 259 = ERROR_NO_MORE_ITEMS
            {
                LogError($"SetupDiEnumDeviceInfo ended with unexpected error 0x{lastError:X}");
            }
        }
        finally
        {
            SetupApi.SetupDiDestroyDeviceInfoList(hDevInfo);
        }

        return results;
    }

    private (IntPtr hDevInfo, SP_DEVINFO_DATA devInfoData, string friendlyName)? FindDevice(Guid classGuid, string namePattern)
    {
        IntPtr hDevInfo = SetupApi.SetupDiGetClassDevs(
            in classGuid, null, IntPtr.Zero, DeviceConstants.DIGCF_PRESENT);

        if (hDevInfo == DeviceConstants.INVALID_HANDLE_VALUE)
        {
            int err = Marshal.GetLastWin32Error();
            LogError($"SetupDiGetClassDevs failed (error 0x{err:X})");
            throw new Win32Exception(err, $"SetupDiGetClassDevs failed (error 0x{err:X})");
        }

        var devInfoData = SP_DEVINFO_DATA.Create();
        for (uint i = 0; SetupApi.SetupDiEnumDeviceInfo(hDevInfo, i, ref devInfoData); i++)
        {
            string? friendlyName = SetupApi.GetDeviceFriendlyName(hDevInfo, ref devInfoData);
            if (friendlyName != null && friendlyName.Contains(namePattern, StringComparison.OrdinalIgnoreCase))
            {
                return (hDevInfo, devInfoData, friendlyName);
            }
        }

        SetupApi.SetupDiDestroyDeviceInfoList(hDevInfo);
        return null;
    }



    private static string GetDeviceInstanceId(IntPtr hDevInfo, ref SP_DEVINFO_DATA devInfoData)
    {
        var buffer = new char[512];
        if (!SetupApi.SetupDiGetDeviceInstanceId(hDevInfo, ref devInfoData, buffer, (uint)buffer.Length, out _))
        {
            int error = Marshal.GetLastWin32Error();
            LogError($"SetupDiGetDeviceInstanceId failed (error 0x{error:X})");
            throw new Win32Exception(error, $"SetupDiGetDeviceInstanceId failed (error 0x{error:X})");
        }

        int len = Array.IndexOf(buffer, '\0');
        return new string(buffer, 0, len >= 0 ? len : buffer.Length);
    }
}
