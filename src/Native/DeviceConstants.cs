namespace AudioFixer.Native;

internal static class DeviceConstants
{
    // Device class GUIDs
    public static readonly Guid GUID_DEVCLASS_MEDIA = new("4d36e96c-e325-11ce-bfc1-08002be10318");
    public static readonly Guid GUID_DEVCLASS_SYSTEM = new("4d36e97d-e325-11ce-bfc1-08002be10318");

    // SetupDiGetClassDevs flags
    public const uint DIGCF_PRESENT = 0x00000002;

    // Device registry property codes (SPDRP_*)
    public const uint SPDRP_DEVICEDESC = 0x00000000;
    public const uint SPDRP_FRIENDLYNAME = 0x0000000C;

    // CM status flags
    public const uint DN_HAS_PROBLEM = 0x00000400;

    // CM problem codes
    public const uint CM_PROB_FAILED_POST_START = 0x2B; // Code 43

    // CM_Locate_DevNode flags
    public const uint CM_LOCATE_DEVNODE_NORMAL = 0x00000000;

    // CR return codes
    public const uint CR_SUCCESS = 0x00000000;

    // DIF codes
    public const uint DIF_PROPERTYCHANGE = 0x00000012;

    // DICS state change codes
    public const uint DICS_ENABLE = 0x00000001;
    public const uint DICS_DISABLE = 0x00000002;

    // DICS flags
    public const uint DICS_FLAG_GLOBAL = 0x00000001;

    // Device name matching (case-insensitive substring match)
    public const string CS35L56_NAME_PATTERN = "CS35L56";
    public const string INTEL_SST_OED_NAME_PATTERN = "Smart Sound Technology OED";

    // INVALID_HANDLE_VALUE
    public static readonly IntPtr INVALID_HANDLE_VALUE = new(-1);
}
