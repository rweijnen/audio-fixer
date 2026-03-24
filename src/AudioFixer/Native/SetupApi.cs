using System.Runtime.InteropServices;

namespace AudioFixer.Native;

[StructLayout(LayoutKind.Sequential)]
internal struct SP_DEVINFO_DATA
{
    public uint cbSize;
    public Guid ClassGuid;
    public uint DevInst;
    public IntPtr Reserved;

    public static SP_DEVINFO_DATA Create()
    {
        return new SP_DEVINFO_DATA
        {
            cbSize = (uint)Marshal.SizeOf<SP_DEVINFO_DATA>()
        };
    }
}

[StructLayout(LayoutKind.Sequential)]
internal struct SP_CLASSINSTALL_HEADER
{
    public uint cbSize;
    public uint InstallFunction;
}

[StructLayout(LayoutKind.Sequential)]
internal struct SP_PROPCHANGE_PARAMS
{
    public SP_CLASSINSTALL_HEADER ClassInstallHeader;
    public uint StateChange;
    public uint Scope;
    public uint HwProfile;

    public static SP_PROPCHANGE_PARAMS Create(uint stateChange)
    {
        return new SP_PROPCHANGE_PARAMS
        {
            ClassInstallHeader = new SP_CLASSINSTALL_HEADER
            {
                cbSize = (uint)Marshal.SizeOf<SP_CLASSINSTALL_HEADER>(),
                InstallFunction = DeviceConstants.DIF_PROPERTYCHANGE
            },
            StateChange = stateChange,
            Scope = DeviceConstants.DICS_FLAG_GLOBAL,
            HwProfile = 0
        };
    }
}

internal static partial class SetupApi
{
    [LibraryImport("setupapi.dll", EntryPoint = "SetupDiGetClassDevsW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    public static partial IntPtr SetupDiGetClassDevs(
        in Guid ClassGuid,
        [MarshalAs(UnmanagedType.LPWStr)] string? Enumerator,
        IntPtr hwndParent,
        uint Flags);

    [LibraryImport("setupapi.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetupDiEnumDeviceInfo(
        IntPtr DeviceInfoSet,
        uint MemberIndex,
        ref SP_DEVINFO_DATA DeviceInfoData);

    [LibraryImport("setupapi.dll", EntryPoint = "SetupDiGetDeviceInstanceIdW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetupDiGetDeviceInstanceId(
        IntPtr DeviceInfoSet,
        ref SP_DEVINFO_DATA DeviceInfoData,
        [Out] char[] DeviceInstanceId,
        uint DeviceInstanceIdSize,
        out uint RequiredSize);

    [LibraryImport("setupapi.dll", EntryPoint = "SetupDiGetDeviceRegistryPropertyW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetupDiGetDeviceRegistryProperty(
        IntPtr DeviceInfoSet,
        ref SP_DEVINFO_DATA DeviceInfoData,
        uint Property,
        out uint PropertyRegDataType,
        IntPtr PropertyBuffer,
        uint PropertyBufferSize,
        out uint RequiredSize);

    [LibraryImport("setupapi.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetupDiSetClassInstallParams(
        IntPtr DeviceInfoSet,
        ref SP_DEVINFO_DATA DeviceInfoData,
        ref SP_PROPCHANGE_PARAMS ClassInstallParams,
        uint ClassInstallParamsSize);

    [LibraryImport("setupapi.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetupDiCallClassInstaller(
        uint InstallFunction,
        IntPtr DeviceInfoSet,
        ref SP_DEVINFO_DATA DeviceInfoData);

    [LibraryImport("setupapi.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

    /// <summary>
    /// Gets the friendly name or device description for a device.
    /// Tries SPDRP_FRIENDLYNAME first, falls back to SPDRP_DEVICEDESC.
    /// </summary>
    public static string? GetDeviceFriendlyName(IntPtr deviceInfoSet, ref SP_DEVINFO_DATA deviceInfoData)
    {
        string? name = GetRegistryPropertyString(deviceInfoSet, ref deviceInfoData, DeviceConstants.SPDRP_FRIENDLYNAME);
        return name ?? GetRegistryPropertyString(deviceInfoSet, ref deviceInfoData, DeviceConstants.SPDRP_DEVICEDESC);
    }

    private static string? GetRegistryPropertyString(IntPtr deviceInfoSet, ref SP_DEVINFO_DATA deviceInfoData, uint property)
    {
        // First call to get required buffer size
        SetupDiGetDeviceRegistryProperty(
            deviceInfoSet, ref deviceInfoData, property,
            out _, IntPtr.Zero, 0, out uint requiredSize);

        if (requiredSize == 0)
            return null;

        IntPtr buffer = Marshal.AllocHGlobal((int)requiredSize);
        try
        {
            if (!SetupDiGetDeviceRegistryProperty(
                    deviceInfoSet, ref deviceInfoData, property,
                    out _, buffer, requiredSize, out _))
                return null;

            return Marshal.PtrToStringUni(buffer);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }
}
