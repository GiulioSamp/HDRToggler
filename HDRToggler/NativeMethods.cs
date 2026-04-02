using System.Runtime.InteropServices;

namespace HDRToggler;

public static class NativeMethods
{
    public const uint QDC_ONLY_ACTIVE_PATHS = 0x00000002;
    public const int ERROR_SUCCESS = 0;

    public enum DISPLAYCONFIG_DEVICE_INFO_TYPE : int
    {
        DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME        = 2,
        DISPLAYCONFIG_DEVICE_INFO_GET_ADVANCED_COLOR_INFO = 9,
        DISPLAYCONFIG_DEVICE_INFO_SET_ADVANCED_COLOR_STATE = 10,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LUID
    {
        public uint LowPart;
        public int  HighPart;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_DEVICE_INFO_HEADER
    {
        public DISPLAYCONFIG_DEVICE_INFO_TYPE type;  // 4 bytes
        public uint size;                             // 4 bytes
        public LUID adapterId;                        // 8 bytes
        public uint id;                               // 4 bytes
    }                                                 // total: 20 bytes

    // Total size verified against Windows SDK: 72 bytes.
    // Only the fields we need are exposed; the rest is padding.
    [StructLayout(LayoutKind.Explicit, Size = 72)]
    public struct DISPLAYCONFIG_PATH_INFO
    {
        // targetInfo starts at offset 20 inside the struct
        [FieldOffset(20)] public LUID targetAdapterId;
        [FieldOffset(28)] public uint targetId;
    }

    // Total size verified against Windows SDK: 64 bytes.
    [StructLayout(LayoutKind.Explicit, Size = 64)]
    public struct DISPLAYCONFIG_MODE_INFO
    {
        [FieldOffset(0)] public uint infoType;
        [FieldOffset(4)] public uint id;
        [FieldOffset(8)] public LUID adapterId;
    }

    // header(20) + value(4) + colorEncoding(4) + bitsPerColorChannel(4) = 32 bytes
    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO
    {
        public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
        public uint value;               // bitfield: bit0=supported, bit1=enabled
        public uint colorEncoding;
        public uint bitsPerColorChannel;

        public bool AdvancedColorSupported => (value & 0x1) != 0;
        public bool AdvancedColorEnabled   => (value & 0x2) != 0;
    }

    // header(20) + value(4) = 24 bytes
    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE
    {
        public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
        public uint value;   // bit0 = enableAdvancedColor
    }

    // header(20) + flags(4) + outputTechnology(4) + edidManufactureId(2) +
    // edidProductCodeId(2) + connectorInstance(4) +
    // monitorFriendlyDeviceName(64*2=128) + monitorDevicePath(128*2=256) = 420 bytes
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct DISPLAYCONFIG_TARGET_DEVICE_NAME
    {
        public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
        public uint   flags;
        public uint   outputTechnology;
        public ushort edidManufactureId;
        public ushort edidProductCodeId;
        public uint   connectorInstance;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string monitorFriendlyDeviceName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string monitorDevicePath;
    }

    [DllImport("user32.dll", ExactSpelling = true)]
    public static extern int GetDisplayConfigBufferSizes(
        uint flags,
        out uint numPathArrayElements,
        out uint numModeInfoArrayElements);

    [DllImport("user32.dll", ExactSpelling = true)]
    public static extern int QueryDisplayConfig(
        uint flags,
        ref uint numPathArrayElements,
        [Out] DISPLAYCONFIG_PATH_INFO[] pathArray,
        ref uint numModeInfoArrayElements,
        [Out] DISPLAYCONFIG_MODE_INFO[] modeInfoArray,
        IntPtr currentTopologyId);

    [DllImport("user32.dll", EntryPoint = "DisplayConfigGetDeviceInfo", ExactSpelling = true)]
    public static extern int DisplayConfigGetAdvancedColorInfo(
        ref DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO requestPacket);

    [DllImport("user32.dll", EntryPoint = "DisplayConfigGetDeviceInfo", ExactSpelling = true)]
    public static extern int DisplayConfigGetTargetName(
        ref DISPLAYCONFIG_TARGET_DEVICE_NAME requestPacket);

    [DllImport("user32.dll", ExactSpelling = true)]
    public static extern int DisplayConfigSetDeviceInfo(
        ref DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE setPacket);
}
