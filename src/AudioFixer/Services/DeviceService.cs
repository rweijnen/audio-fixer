using System.ComponentModel;
using System.Runtime.InteropServices;
using AudioFixer.Native;

namespace AudioFixer.Services;

public record DeviceStatus(string InstanceId, string FriendlyName, bool HasProblem, uint ProblemCode);

public sealed class DeviceService
{
    /// <summary>
    /// Returns the status of all CS35L56 amp devices found on the system.
    /// </summary>
    public List<DeviceStatus> GetAmpDeviceStatuses()
    {
        return EnumerateDevicesWithStatus(
            DeviceConstants.GUID_DEVCLASS_MEDIA,
            DeviceConstants.CS35L56_NAME_PATTERN);
    }

    /// <summary>
    /// Returns true if any CS35L56 amp device has a Code 43 error.
    /// </summary>
    public bool HasCode43Problem()
    {
        return GetAmpDeviceStatuses().Any(d => d.HasProblem && d.ProblemCode == DeviceConstants.CM_PROB_FAILED_POST_START);
    }

    /// <summary>
    /// Gets a summary of the current problem state for display.
    /// </summary>
    public (bool HasProblem, int AffectedCount, int TotalCount) GetProblemSummary()
    {
        var statuses = GetAmpDeviceStatuses();
        int affected = statuses.Count(d => d.HasProblem && d.ProblemCode == DeviceConstants.CM_PROB_FAILED_POST_START);
        return (affected > 0, affected, statuses.Count);
    }

    /// <summary>
    /// Fixes audio devices by disabling and re-enabling the Intel SST OED device,
    /// then verifying CS35L56 amps are healthy.
    /// </summary>
    public async Task FixAudioDevicesAsync()
    {
        // Find the Intel SST OED device
        var oedDevice = FindDevice(
            DeviceConstants.GUID_DEVCLASS_SYSTEM,
            DeviceConstants.INTEL_SST_OED_NAME_PATTERN);

        if (oedDevice == null)
            throw new InvalidOperationException(
                $"Could not find device matching \"{DeviceConstants.INTEL_SST_OED_NAME_PATTERN}\". " +
                "Ensure the Intel Smart Sound Technology OED device is present in Device Manager.");

        var (hDevInfo, devInfoData) = oedDevice.Value;
        try
        {
            // Disable the device
            ChangeDeviceState(hDevInfo, ref devInfoData, DeviceConstants.DICS_DISABLE);

            // Wait for driver stack to tear down
            await Task.Delay(2000);

            // Re-enable the device
            ChangeDeviceState(hDevInfo, ref devInfoData, DeviceConstants.DICS_ENABLE);
        }
        finally
        {
            SetupApi.SetupDiDestroyDeviceInfoList(hDevInfo);
        }

        // Wait for child SoundWire devices to re-enumerate
        await Task.Delay(3000);

        // Verify the fix worked
        var statuses = GetAmpDeviceStatuses();
        var stillBroken = statuses.Where(d => d.HasProblem && d.ProblemCode == DeviceConstants.CM_PROB_FAILED_POST_START).ToList();
        if (stillBroken.Count > 0)
        {
            var names = string.Join(", ", stillBroken.Select(d => d.FriendlyName));
            throw new InvalidOperationException(
                $"Fix was applied but {stillBroken.Count} device(s) still have Code 43: {names}");
        }
    }

    private List<DeviceStatus> EnumerateDevicesWithStatus(Guid classGuid, string namePattern)
    {
        var results = new List<DeviceStatus>();
        IntPtr hDevInfo = SetupApi.SetupDiGetClassDevs(
            in classGuid, null, IntPtr.Zero, DeviceConstants.DIGCF_PRESENT);

        if (hDevInfo == DeviceConstants.INVALID_HANDLE_VALUE)
            throw new Win32Exception(Marshal.GetLastWin32Error(), "SetupDiGetClassDevs failed");

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
                bool hasProblem = cr == DeviceConstants.CR_SUCCESS && (status & DeviceConstants.DN_HAS_PROBLEM) != 0;

                results.Add(new DeviceStatus(instanceId, friendlyName, hasProblem, hasProblem ? problemNumber : 0));
            }
        }
        finally
        {
            SetupApi.SetupDiDestroyDeviceInfoList(hDevInfo);
        }

        return results;
    }

    /// <summary>
    /// Finds a single device by class GUID and friendly name pattern.
    /// Returns the device info set handle and device info data (caller must destroy the handle).
    /// </summary>
    private (IntPtr hDevInfo, SP_DEVINFO_DATA devInfoData)? FindDevice(Guid classGuid, string namePattern)
    {
        IntPtr hDevInfo = SetupApi.SetupDiGetClassDevs(
            in classGuid, null, IntPtr.Zero, DeviceConstants.DIGCF_PRESENT);

        if (hDevInfo == DeviceConstants.INVALID_HANDLE_VALUE)
            throw new Win32Exception(Marshal.GetLastWin32Error(), "SetupDiGetClassDevs failed");

        var devInfoData = SP_DEVINFO_DATA.Create();
        for (uint i = 0; SetupApi.SetupDiEnumDeviceInfo(hDevInfo, i, ref devInfoData); i++)
        {
            string? friendlyName = SetupApi.GetDeviceFriendlyName(hDevInfo, ref devInfoData);
            if (friendlyName != null && friendlyName.Contains(namePattern, StringComparison.OrdinalIgnoreCase))
            {
                return (hDevInfo, devInfoData);
            }
        }

        SetupApi.SetupDiDestroyDeviceInfoList(hDevInfo);
        return null;
    }

    private static void ChangeDeviceState(IntPtr hDevInfo, ref SP_DEVINFO_DATA devInfoData, uint stateChange)
    {
        var propChangeParams = SP_PROPCHANGE_PARAMS.Create(stateChange);
        uint size = (uint)Marshal.SizeOf<SP_PROPCHANGE_PARAMS>();

        if (!SetupApi.SetupDiSetClassInstallParams(hDevInfo, ref devInfoData, ref propChangeParams, size))
        {
            int error = Marshal.GetLastWin32Error();
            string action = stateChange == DeviceConstants.DICS_DISABLE ? "disable" : "enable";
            throw new Win32Exception(error, $"SetupDiSetClassInstallParams failed during {action} (error 0x{error:X})");
        }

        if (!SetupApi.SetupDiCallClassInstaller(DeviceConstants.DIF_PROPERTYCHANGE, hDevInfo, ref devInfoData))
        {
            int error = Marshal.GetLastWin32Error();
            string action = stateChange == DeviceConstants.DICS_DISABLE ? "disable" : "enable";
            throw new Win32Exception(error, $"SetupDiCallClassInstaller failed during {action} (error 0x{error:X})");
        }
    }

    private static string GetDeviceInstanceId(IntPtr hDevInfo, ref SP_DEVINFO_DATA devInfoData)
    {
        var buffer = new char[512];
        if (!SetupApi.SetupDiGetDeviceInstanceId(hDevInfo, ref devInfoData, buffer, (uint)buffer.Length, out _))
        {
            int error = Marshal.GetLastWin32Error();
            throw new Win32Exception(error, $"SetupDiGetDeviceInstanceId failed (error 0x{error:X})");
        }

        int len = Array.IndexOf(buffer, '\0');
        return new string(buffer, 0, len >= 0 ? len : buffer.Length);
    }
}
