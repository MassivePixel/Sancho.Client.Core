//using Plugin.Settings;
//using Plugin.Settings.Abstractions;

namespace Sancho.Client.Core.Helpers
{
    public static class Settings
    {
        //static ISettings AppSettings => CrossSettings.Current;

        public static string DeviceId
        {
            get;
            set;
            //get { return AppSettings.GetValueOrDefault(nameof(DeviceId), string.Empty); }
            //internal set { AppSettings.AddOrUpdateValue(nameof(DeviceId), value); }
        }
    }
}
