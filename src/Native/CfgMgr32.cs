using System.Runtime.InteropServices;

namespace AudioFixer.Native;

internal static partial class CfgMgr32
{
    [LibraryImport("CfgMgr32.dll")]
    public static partial uint CM_Get_DevNode_Status(
        out uint pulStatus,
        out uint pulProblemNumber,
        uint dnDevInst,
        uint ulFlags);
}
