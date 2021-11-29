using System.Runtime.InteropServices;

namespace LateCat.PoseidonEngine.Utilities
{
    public class BatteryChecker
    {
        private BatteryChecker()
        {

        }

        public static bool GetSystemPowerStatus(ref SystemPowerStatus sps)
        {
            sps = new SystemPowerStatus();

            return GetSystemPowerStatus(sps);
        }

        public static SystemStatusFlag GetBatterySaverStatus()
        {
            var sps = new SystemPowerStatus();

            return GetSystemPowerStatus(sps) ? sps._SystemStatusFlag : SystemStatusFlag.Off;
        }

        public static ACLineStatus GetACPowerStatus()
        {
            var sps = new SystemPowerStatus();

            return GetSystemPowerStatus(sps) ? sps._ACLineStatus : ACLineStatus.Online;
        }

        public static bool IsBatterySavingMode => GetBatterySaverStatus() == SystemStatusFlag.On;

        #region pinvoke

        [DllImport("Kernel32")]
        private static extern bool GetSystemPowerStatus(SystemPowerStatus sps);

        public enum ACLineStatus : byte
        {
            Offline = 0,
            Online = 1,
            Unknown = 255
        }

        public enum BatteryFlag : byte
        {
            High = 1,
            Low = 2,
            Critical = 4,
            Charging = 8,
            NoSystemBattery = 128,
            Unknown = 255
        }

        public enum SystemStatusFlag : byte
        {
            Off = 0,
            On = 1
        }

        [StructLayout(LayoutKind.Sequential)]
        public class SystemPowerStatus
        {
            public ACLineStatus _ACLineStatus;
            public BatteryFlag _BatteryFlag;
            public byte _BatteryLifePercent;
            public SystemStatusFlag _SystemStatusFlag;
            public int _BatteryLifeTime;
            public int _BatteryFullLifeTime;
        }

        #endregion //pinvoke
    }
}
