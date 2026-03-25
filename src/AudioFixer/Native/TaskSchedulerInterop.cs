using System.Runtime.InteropServices;

namespace AudioFixer.Native;

// COM interop for Task Scheduler 2.0
// Interfaces defined from taskschd.h vtable order (dual interfaces via IUnknown binding).

[ComImport, Guid("0f87369f-a4e5-4cfc-bd3e-73e6154572dd")]
internal class TaskSchedulerClass;

// ITaskService : IDispatch
// GUID: 2faba4c7-4da9-4013-9697-20cc3fd40f85
// Vtable after IDispatch: GetFolder, GetRunningTasks, NewTask, Connect, ...
[ComImport, Guid("2faba4c7-4da9-4013-9697-20cc3fd40f85")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ITaskService
{
    // IDispatch
    [PreserveSig] int GetTypeInfoCount(out uint pctinfo);
    [PreserveSig] int GetTypeInfo(uint iTInfo, uint lcid, out IntPtr ppTInfo);
    [PreserveSig] int GetIDsOfNames(ref Guid riid, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)] string[] rgszNames, uint cNames, uint lcid, [MarshalAs(UnmanagedType.LPArray)] int[] rgDispId);
    [PreserveSig] int Invoke(int dispIdMember, ref Guid riid, uint lcid, ushort wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr);

    // ITaskService
    ITaskFolder GetFolder([MarshalAs(UnmanagedType.BStr)] string path);
    [return: MarshalAs(UnmanagedType.Interface)] object GetRunningTasks(int flags);
    ITaskDefinition NewTask(uint flags);
    void Connect(
        [MarshalAs(UnmanagedType.Struct)] object? serverName,
        [MarshalAs(UnmanagedType.Struct)] object? user,
        [MarshalAs(UnmanagedType.Struct)] object? domain,
        [MarshalAs(UnmanagedType.Struct)] object? password);
}

// ITaskFolder : IDispatch
// GUID: 8cfac062-a080-4c15-9a88-aa7c2af80dfc
// Vtable after IDispatch: get_Name, get_Path, GetFolder, GetFolders, CreateFolder,
//   DeleteFolder, GetTask, GetTasks, DeleteTask, RegisterTask, RegisterTaskDefinition, ...
[ComImport, Guid("8cfac062-a080-4c15-9a88-aa7c2af80dfc")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ITaskFolder
{
    // IDispatch
    [PreserveSig] int GetTypeInfoCount(out uint pctinfo);
    [PreserveSig] int GetTypeInfo(uint iTInfo, uint lcid, out IntPtr ppTInfo);
    [PreserveSig] int GetIDsOfNames(ref Guid riid, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)] string[] rgszNames, uint cNames, uint lcid, [MarshalAs(UnmanagedType.LPArray)] int[] rgDispId);
    [PreserveSig] int Invoke(int dispIdMember, ref Guid riid, uint lcid, ushort wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr);

    // ITaskFolder
    [return: MarshalAs(UnmanagedType.BStr)] string get_Name();
    [return: MarshalAs(UnmanagedType.BStr)] string get_Path();
    ITaskFolder GetFolder([MarshalAs(UnmanagedType.BStr)] string path);
    [return: MarshalAs(UnmanagedType.Interface)] object GetFolders(int flags);
    ITaskFolder CreateFolder(
        [MarshalAs(UnmanagedType.BStr)] string subFolderName,
        [MarshalAs(UnmanagedType.Struct)] object sddl);
    void DeleteFolder([MarshalAs(UnmanagedType.BStr)] string subFolderName, int flags);
    [return: MarshalAs(UnmanagedType.Interface)] object GetTask([MarshalAs(UnmanagedType.BStr)] string path);
    [return: MarshalAs(UnmanagedType.Interface)] object GetTasks(int flags);
    void DeleteTask([MarshalAs(UnmanagedType.BStr)] string name, int flags);
    [return: MarshalAs(UnmanagedType.Interface)] object RegisterTask(
        [MarshalAs(UnmanagedType.BStr)] string path,
        [MarshalAs(UnmanagedType.BStr)] string xmlText,
        int flags,
        [MarshalAs(UnmanagedType.Struct)] object userId,
        [MarshalAs(UnmanagedType.Struct)] object password,
        int logonType,
        [MarshalAs(UnmanagedType.Struct)] object sddl);
    [return: MarshalAs(UnmanagedType.Interface)] object RegisterTaskDefinition(
        [MarshalAs(UnmanagedType.BStr)] string path,
        [MarshalAs(UnmanagedType.Interface)] ITaskDefinition pDefinition,
        int flags,
        [MarshalAs(UnmanagedType.Struct)] object? userId,
        [MarshalAs(UnmanagedType.Struct)] object? password,
        int logonType,
        [MarshalAs(UnmanagedType.Struct)] object? sddl);
}

// ITaskDefinition : IDispatch
// GUID: f5bc8fc5-536d-4f77-b852-fbc1356fdeb6
// Vtable after IDispatch: get/put_RegistrationInfo, get/put_Triggers, get/put_Settings,
//   get/put_Data, get/put_Principal, get/put_Actions, get/put_XmlText
[ComImport, Guid("f5bc8fc5-536d-4f77-b852-fbc1356fdeb6")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ITaskDefinition
{
    // IDispatch
    [PreserveSig] int GetTypeInfoCount(out uint pctinfo);
    [PreserveSig] int GetTypeInfo(uint iTInfo, uint lcid, out IntPtr ppTInfo);
    [PreserveSig] int GetIDsOfNames(ref Guid riid, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)] string[] rgszNames, uint cNames, uint lcid, [MarshalAs(UnmanagedType.LPArray)] int[] rgDispId);
    [PreserveSig] int Invoke(int dispIdMember, ref Guid riid, uint lcid, ushort wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr);

    // ITaskDefinition
    IRegistrationInfo get_RegistrationInfo();
    void put_RegistrationInfo(IRegistrationInfo pRegistrationInfo);
    ITriggerCollection get_Triggers();
    void put_Triggers(ITriggerCollection pTriggers);
    ITaskSettings get_Settings();
    void put_Settings(ITaskSettings pSettings);
    [return: MarshalAs(UnmanagedType.BStr)] string get_Data();
    void put_Data([MarshalAs(UnmanagedType.BStr)] string data);
    IPrincipal get_Principal();
    void put_Principal(IPrincipal pPrincipal);
    IActionCollection get_Actions();
    void put_Actions(IActionCollection pActions);
    [return: MarshalAs(UnmanagedType.BStr)] string get_XmlText();
    void put_XmlText([MarshalAs(UnmanagedType.BStr)] string text);
}

// IRegistrationInfo : IDispatch
// GUID: 416d8b73-cb41-4ea1-805c-9be9a5ac4a74
[ComImport, Guid("416d8b73-cb41-4ea1-805c-9be9a5ac4a74")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IRegistrationInfo
{
    // IDispatch
    [PreserveSig] int GetTypeInfoCount(out uint pctinfo);
    [PreserveSig] int GetTypeInfo(uint iTInfo, uint lcid, out IntPtr ppTInfo);
    [PreserveSig] int GetIDsOfNames(ref Guid riid, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)] string[] rgszNames, uint cNames, uint lcid, [MarshalAs(UnmanagedType.LPArray)] int[] rgDispId);
    [PreserveSig] int Invoke(int dispIdMember, ref Guid riid, uint lcid, ushort wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr);

    // IRegistrationInfo - vtable order from taskschd.h
    [return: MarshalAs(UnmanagedType.BStr)] string get_Description();
    void put_Description([MarshalAs(UnmanagedType.BStr)] string description);
}

// ITriggerCollection : IDispatch
// GUID: 85df5081-1b24-4f32-878a-d9d14df4cb77
[ComImport, Guid("85df5081-1b24-4f32-878a-d9d14df4cb77")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ITriggerCollection
{
    // IDispatch
    [PreserveSig] int GetTypeInfoCount(out uint pctinfo);
    [PreserveSig] int GetTypeInfo(uint iTInfo, uint lcid, out IntPtr ppTInfo);
    [PreserveSig] int GetIDsOfNames(ref Guid riid, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)] string[] rgszNames, uint cNames, uint lcid, [MarshalAs(UnmanagedType.LPArray)] int[] rgDispId);
    [PreserveSig] int Invoke(int dispIdMember, ref Guid riid, uint lcid, ushort wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr);

    // ITriggerCollection
    int get_Count();
    [return: MarshalAs(UnmanagedType.Interface)] object get_Item(int index);
    [return: MarshalAs(UnmanagedType.Interface)] object get__NewEnum();
    [return: MarshalAs(UnmanagedType.Interface)] ITrigger Create(int type);
    void Remove([MarshalAs(UnmanagedType.Struct)] object index);
    void Clear();
}

// ITrigger : IDispatch
// GUID: 09941815-ea89-4b5b-89e0-2a773801fac3
[ComImport, Guid("09941815-ea89-4b5b-89e0-2a773801fac3")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ITrigger
{
    // IDispatch
    [PreserveSig] int GetTypeInfoCount(out uint pctinfo);
    [PreserveSig] int GetTypeInfo(uint iTInfo, uint lcid, out IntPtr ppTInfo);
    [PreserveSig] int GetIDsOfNames(ref Guid riid, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)] string[] rgszNames, uint cNames, uint lcid, [MarshalAs(UnmanagedType.LPArray)] int[] rgDispId);
    [PreserveSig] int Invoke(int dispIdMember, ref Guid riid, uint lcid, ushort wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr);

    // ITrigger
    int get_Type();
    [return: MarshalAs(UnmanagedType.BStr)] string get_Id();
    void put_Id([MarshalAs(UnmanagedType.BStr)] string id);
    [return: MarshalAs(UnmanagedType.Interface)] object get_Repetition();
    void put_Repetition([MarshalAs(UnmanagedType.Interface)] object pRepetition);
    [return: MarshalAs(UnmanagedType.BStr)] string get_ExecutionTimeLimit();
    void put_ExecutionTimeLimit([MarshalAs(UnmanagedType.BStr)] string limit);
    [return: MarshalAs(UnmanagedType.BStr)] string get_StartBoundary();
    void put_StartBoundary([MarshalAs(UnmanagedType.BStr)] string start);
    [return: MarshalAs(UnmanagedType.BStr)] string get_EndBoundary();
    void put_EndBoundary([MarshalAs(UnmanagedType.BStr)] string end);
    [return: MarshalAs(UnmanagedType.VariantBool)] bool get_Enabled();
    void put_Enabled([MarshalAs(UnmanagedType.VariantBool)] bool enabled);
}

// IEventTrigger : ITrigger (extends ITrigger with Subscription, Delay, ValueQueries)
// GUID: d45b0167-9653-4eef-b94f-0732ca7af251
[ComImport, Guid("d45b0167-9653-4eef-b94f-0732ca7af251")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IEventTrigger
{
    // IDispatch
    [PreserveSig] int GetTypeInfoCount(out uint pctinfo);
    [PreserveSig] int GetTypeInfo(uint iTInfo, uint lcid, out IntPtr ppTInfo);
    [PreserveSig] int GetIDsOfNames(ref Guid riid, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)] string[] rgszNames, uint cNames, uint lcid, [MarshalAs(UnmanagedType.LPArray)] int[] rgDispId);
    [PreserveSig] int Invoke(int dispIdMember, ref Guid riid, uint lcid, ushort wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr);

    // ITrigger base members (must be repeated for vtable)
    int get_Type();
    [return: MarshalAs(UnmanagedType.BStr)] string get_Id();
    void put_Id([MarshalAs(UnmanagedType.BStr)] string id);
    [return: MarshalAs(UnmanagedType.Interface)] object get_Repetition();
    void put_Repetition([MarshalAs(UnmanagedType.Interface)] object pRepetition);
    [return: MarshalAs(UnmanagedType.BStr)] string get_ExecutionTimeLimit();
    void put_ExecutionTimeLimit([MarshalAs(UnmanagedType.BStr)] string limit);
    [return: MarshalAs(UnmanagedType.BStr)] string get_StartBoundary();
    void put_StartBoundary([MarshalAs(UnmanagedType.BStr)] string start);
    [return: MarshalAs(UnmanagedType.BStr)] string get_EndBoundary();
    void put_EndBoundary([MarshalAs(UnmanagedType.BStr)] string end);
    [return: MarshalAs(UnmanagedType.VariantBool)] bool get_Enabled();
    void put_Enabled([MarshalAs(UnmanagedType.VariantBool)] bool enabled);

    // IEventTrigger
    [return: MarshalAs(UnmanagedType.BStr)] string get_Subscription();
    void put_Subscription([MarshalAs(UnmanagedType.BStr)] string query);
    [return: MarshalAs(UnmanagedType.BStr)] string get_Delay();
    void put_Delay([MarshalAs(UnmanagedType.BStr)] string delay);
    // ValueQueries omitted - not needed
}

// IActionCollection : IDispatch
// GUID: 02820e19-7b98-4ed2-b2e8-fdccceff619b
[ComImport, Guid("02820e19-7b98-4ed2-b2e8-fdccceff619b")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IActionCollection
{
    // IDispatch
    [PreserveSig] int GetTypeInfoCount(out uint pctinfo);
    [PreserveSig] int GetTypeInfo(uint iTInfo, uint lcid, out IntPtr ppTInfo);
    [PreserveSig] int GetIDsOfNames(ref Guid riid, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)] string[] rgszNames, uint cNames, uint lcid, [MarshalAs(UnmanagedType.LPArray)] int[] rgDispId);
    [PreserveSig] int Invoke(int dispIdMember, ref Guid riid, uint lcid, ushort wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr);

    // IActionCollection
    int get_Count();
    [return: MarshalAs(UnmanagedType.Interface)] object get_Item(int index);
    [return: MarshalAs(UnmanagedType.Interface)] object get__NewEnum();
    [return: MarshalAs(UnmanagedType.Interface)] object get_XmlText();
    void put_XmlText([MarshalAs(UnmanagedType.BStr)] string text);
    IAction Create(int type);
    void Remove([MarshalAs(UnmanagedType.Struct)] object index);
    void Clear();
    [return: MarshalAs(UnmanagedType.BStr)] string get_Context();
    void put_Context([MarshalAs(UnmanagedType.BStr)] string context);
}

// IAction : IDispatch
// GUID: bae54997-48b1-4cbe-9965-d6be263ebea4
[ComImport, Guid("bae54997-48b1-4cbe-9965-d6be263ebea4")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IAction
{
    // IDispatch
    [PreserveSig] int GetTypeInfoCount(out uint pctinfo);
    [PreserveSig] int GetTypeInfo(uint iTInfo, uint lcid, out IntPtr ppTInfo);
    [PreserveSig] int GetIDsOfNames(ref Guid riid, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)] string[] rgszNames, uint cNames, uint lcid, [MarshalAs(UnmanagedType.LPArray)] int[] rgDispId);
    [PreserveSig] int Invoke(int dispIdMember, ref Guid riid, uint lcid, ushort wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr);

    // IAction
    [return: MarshalAs(UnmanagedType.BStr)] string get_Id();
    void put_Id([MarshalAs(UnmanagedType.BStr)] string id);
    int get_Type();
}

// IExecAction : IAction
// GUID: 4c3d624d-fd6b-49a3-b9b7-09cb3cd3f047
[ComImport, Guid("4c3d624d-fd6b-49a3-b9b7-09cb3cd3f047")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IExecAction
{
    // IDispatch
    [PreserveSig] int GetTypeInfoCount(out uint pctinfo);
    [PreserveSig] int GetTypeInfo(uint iTInfo, uint lcid, out IntPtr ppTInfo);
    [PreserveSig] int GetIDsOfNames(ref Guid riid, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)] string[] rgszNames, uint cNames, uint lcid, [MarshalAs(UnmanagedType.LPArray)] int[] rgDispId);
    [PreserveSig] int Invoke(int dispIdMember, ref Guid riid, uint lcid, ushort wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr);

    // IAction base
    [return: MarshalAs(UnmanagedType.BStr)] string get_Id();
    void put_Id([MarshalAs(UnmanagedType.BStr)] string id);
    int get_Type();

    // IExecAction
    [return: MarshalAs(UnmanagedType.BStr)] string get_Path();
    void put_Path([MarshalAs(UnmanagedType.BStr)] string path);
    [return: MarshalAs(UnmanagedType.BStr)] string get_Arguments();
    void put_Arguments([MarshalAs(UnmanagedType.BStr)] string arguments);
    [return: MarshalAs(UnmanagedType.BStr)] string get_WorkingDirectory();
    void put_WorkingDirectory([MarshalAs(UnmanagedType.BStr)] string directory);
}

// ITaskSettings : IDispatch
// GUID: 8fd4711d-2d02-4c8c-87e3-eff699de127e
// Vtable has many properties; we define them all in order to keep offsets correct.
[ComImport, Guid("8fd4711d-2d02-4c8c-87e3-eff699de127e")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ITaskSettings
{
    // IDispatch
    [PreserveSig] int GetTypeInfoCount(out uint pctinfo);
    [PreserveSig] int GetTypeInfo(uint iTInfo, uint lcid, out IntPtr ppTInfo);
    [PreserveSig] int GetIDsOfNames(ref Guid riid, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)] string[] rgszNames, uint cNames, uint lcid, [MarshalAs(UnmanagedType.LPArray)] int[] rgDispId);
    [PreserveSig] int Invoke(int dispIdMember, ref Guid riid, uint lcid, ushort wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr);

    // ITaskSettings - all properties in vtable order
    [return: MarshalAs(UnmanagedType.VariantBool)] bool get_AllowDemandStart();
    void put_AllowDemandStart([MarshalAs(UnmanagedType.VariantBool)] bool allow);
    [return: MarshalAs(UnmanagedType.BStr)] string get_RestartInterval();
    void put_RestartInterval([MarshalAs(UnmanagedType.BStr)] string interval);
    int get_RestartCount();
    void put_RestartCount(int count);
    int get_MultipleInstances();
    void put_MultipleInstances(int policy);
    [return: MarshalAs(UnmanagedType.VariantBool)] bool get_StopIfGoingOnBatteries();
    void put_StopIfGoingOnBatteries([MarshalAs(UnmanagedType.VariantBool)] bool stop);
    [return: MarshalAs(UnmanagedType.VariantBool)] bool get_DisallowStartIfOnBatteries();
    void put_DisallowStartIfOnBatteries([MarshalAs(UnmanagedType.VariantBool)] bool disallow);
    [return: MarshalAs(UnmanagedType.VariantBool)] bool get_AllowHardTerminate();
    void put_AllowHardTerminate([MarshalAs(UnmanagedType.VariantBool)] bool allow);
    [return: MarshalAs(UnmanagedType.VariantBool)] bool get_StartWhenAvailable();
    void put_StartWhenAvailable([MarshalAs(UnmanagedType.VariantBool)] bool start);
    [return: MarshalAs(UnmanagedType.BStr)] string get_XmlText();
    void put_XmlText([MarshalAs(UnmanagedType.BStr)] string text);
    [return: MarshalAs(UnmanagedType.VariantBool)] bool get_RunOnlyIfNetworkAvailable();
    void put_RunOnlyIfNetworkAvailable([MarshalAs(UnmanagedType.VariantBool)] bool run);
    [return: MarshalAs(UnmanagedType.BStr)] string get_ExecutionTimeLimit();
    void put_ExecutionTimeLimit([MarshalAs(UnmanagedType.BStr)] string limit);
    [return: MarshalAs(UnmanagedType.VariantBool)] bool get_Enabled();
    void put_Enabled([MarshalAs(UnmanagedType.VariantBool)] bool enabled);
    [return: MarshalAs(UnmanagedType.BStr)] string get_DeleteExpiredTaskAfter();
    void put_DeleteExpiredTaskAfter([MarshalAs(UnmanagedType.BStr)] string expirationDelay);
    int get_Priority();
    void put_Priority(int priority);
    int get_Compatibility();
    void put_Compatibility(int compatibility);
    [return: MarshalAs(UnmanagedType.VariantBool)] bool get_Hidden();
    void put_Hidden([MarshalAs(UnmanagedType.VariantBool)] bool hidden);
    [return: MarshalAs(UnmanagedType.Interface)] object get_IdleSettings();
    void put_IdleSettings([MarshalAs(UnmanagedType.Interface)] object pIdleSettings);
    [return: MarshalAs(UnmanagedType.VariantBool)] bool get_RunOnlyIfIdle();
    void put_RunOnlyIfIdle([MarshalAs(UnmanagedType.VariantBool)] bool run);
    [return: MarshalAs(UnmanagedType.VariantBool)] bool get_WakeToRun();
    void put_WakeToRun([MarshalAs(UnmanagedType.VariantBool)] bool wake);
    [return: MarshalAs(UnmanagedType.Interface)] object get_NetworkSettings();
    void put_NetworkSettings([MarshalAs(UnmanagedType.Interface)] object pNetworkSettings);
}

// IPrincipal : IDispatch
// GUID: d98d51e5-c9b4-496a-a9c1-18980261cf0f
[ComImport, Guid("d98d51e5-c9b4-496a-a9c1-18980261cf0f")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IPrincipal
{
    // IDispatch
    [PreserveSig] int GetTypeInfoCount(out uint pctinfo);
    [PreserveSig] int GetTypeInfo(uint iTInfo, uint lcid, out IntPtr ppTInfo);
    [PreserveSig] int GetIDsOfNames(ref Guid riid, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)] string[] rgszNames, uint cNames, uint lcid, [MarshalAs(UnmanagedType.LPArray)] int[] rgDispId);
    [PreserveSig] int Invoke(int dispIdMember, ref Guid riid, uint lcid, ushort wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr);

    // IPrincipal - vtable order
    [return: MarshalAs(UnmanagedType.BStr)] string get_Id();
    void put_Id([MarshalAs(UnmanagedType.BStr)] string id);
    [return: MarshalAs(UnmanagedType.BStr)] string get_DisplayName();
    void put_DisplayName([MarshalAs(UnmanagedType.BStr)] string name);
    [return: MarshalAs(UnmanagedType.BStr)] string get_UserId();
    void put_UserId([MarshalAs(UnmanagedType.BStr)] string userId);
    int get_LogonType();
    void put_LogonType(int logonType);
    [return: MarshalAs(UnmanagedType.BStr)] string get_GroupId();
    void put_GroupId([MarshalAs(UnmanagedType.BStr)] string groupId);
    int get_RunLevel();
    void put_RunLevel(int runLevel);
}

// Constants
internal static class TaskConstants
{
    public const int TASK_TRIGGER_EVENT = 0;
    public const int TASK_ACTION_EXEC = 0;
    public const int TASK_CREATE_OR_UPDATE = 6;
    public const int TASK_LOGON_INTERACTIVE_TOKEN = 3;
    public const int TASK_RUNLEVEL_HIGHEST = 1;
    public const int TASK_INSTANCES_IGNORE_NEW = 2;
}
