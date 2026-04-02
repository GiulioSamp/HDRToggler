using System.Runtime.InteropServices;

namespace HDRToggler;

public record HdrMonitor(
    string FriendlyName,
    NativeMethods.LUID AdapterId,
    uint TargetId,
    bool SupportsHdr,
    bool HdrEnabled);

public static class HdrService
{
    // Fired after a toggle; source = the object that triggered it (used to skip self-refresh)
    public static event Action<object?>? StateChanged;

    public static List<HdrMonitor> GetMonitors()
    {
        var result = new List<HdrMonitor>();

        if (NativeMethods.GetDisplayConfigBufferSizes(
                NativeMethods.QDC_ONLY_ACTIVE_PATHS,
                out uint pathCount,
                out uint modeCount) != NativeMethods.ERROR_SUCCESS)
            return result;

        var paths = new NativeMethods.DISPLAYCONFIG_PATH_INFO[pathCount];
        var modes = new NativeMethods.DISPLAYCONFIG_MODE_INFO[modeCount];

        if (NativeMethods.QueryDisplayConfig(
                NativeMethods.QDC_ONLY_ACTIVE_PATHS,
                ref pathCount, paths,
                ref modeCount, modes,
                IntPtr.Zero) != NativeMethods.ERROR_SUCCESS)
            return result;

        foreach (var path in paths)
        {
            var nameReq = new NativeMethods.DISPLAYCONFIG_TARGET_DEVICE_NAME
            {
                header = new NativeMethods.DISPLAYCONFIG_DEVICE_INFO_HEADER
                {
                    type = NativeMethods.DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME,
                    size = (uint)Marshal.SizeOf<NativeMethods.DISPLAYCONFIG_TARGET_DEVICE_NAME>(),
                    adapterId = path.targetAdapterId,
                    id = path.targetId,
                }
            };
            NativeMethods.DisplayConfigGetTargetName(ref nameReq);

            var name = string.IsNullOrWhiteSpace(nameReq.monitorFriendlyDeviceName)
                ? $"Display {path.targetId}"
                : nameReq.monitorFriendlyDeviceName;

            var colorReq = new NativeMethods.DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO
            {
                header = new NativeMethods.DISPLAYCONFIG_DEVICE_INFO_HEADER
                {
                    type = NativeMethods.DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_ADVANCED_COLOR_INFO,
                    size = (uint)Marshal.SizeOf<NativeMethods.DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO>(),
                    adapterId = path.targetAdapterId,
                    id = path.targetId,
                }
            };

            if (NativeMethods.DisplayConfigGetAdvancedColorInfo(ref colorReq) != NativeMethods.ERROR_SUCCESS)
                continue;

            if (!colorReq.AdvancedColorSupported)
                continue;

            result.Add(new HdrMonitor(
                FriendlyName:  name,
                AdapterId:     path.targetAdapterId,
                TargetId:      path.targetId,
                SupportsHdr:   true,
                HdrEnabled:    colorReq.AdvancedColorEnabled));
        }

        return result;
    }

    public static void SetHdr(HdrMonitor monitor, bool enabled, object? source = null)
    {
        var req = new NativeMethods.DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE
        {
            header = new NativeMethods.DISPLAYCONFIG_DEVICE_INFO_HEADER
            {
                type = NativeMethods.DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_SET_ADVANCED_COLOR_STATE,
                size = (uint)Marshal.SizeOf<NativeMethods.DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE>(),
                adapterId = monitor.AdapterId,
                id = monitor.TargetId,
            },
            value = enabled ? 1u : 0u
        };

        NativeMethods.DisplayConfigSetDeviceInfo(ref req);
        StateChanged?.Invoke(source);
    }

    public static void ToggleHdr(HdrMonitor monitor, object? source = null)
        => SetHdr(monitor, !monitor.HdrEnabled, source);
}
