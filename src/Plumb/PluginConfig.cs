using BepInEx.Configuration;
using Plumb.Core.Model;

namespace Plumb
{
    public static class PluginConfig
    {
        public static ConfigEntry<float> Strength;
        public static ConfigEntry<KeyboardShortcut> CompareKey;

        public static void Bind(ConfigFile cfg)
        {
            Strength = cfg.Bind("General", "Strength", 0f,
                new ConfigDescription("How much building pieces warp. 0 = perfectly straight, 1 = vanilla, up to 3 = more warped than vanilla.",
                    new AcceptableValueRange<float>(0f, WarpModel.MaxStrength)));
            CompareKey = cfg.Bind("General", "CompareKey", KeyboardShortcut.Empty,
                "Toggle between vanilla warp and your Strength, to compare. Not saved.");

            Strength.SettingChanged += (_, __) => Ripple.Apply();
        }
    }
}
