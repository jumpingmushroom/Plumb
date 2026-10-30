using System;
using System.Globalization;

namespace Plumb.Core.Model
{
    /// <summary>Maps a material's vanilla <c>_RippleDistance</c> and the strength setting to the value to write.</summary>
    public static class WarpModel
    {
        public const float MaxStrength = 3f;

        public static float Target(float vanilla, float strength, bool compare) =>
            compare ? vanilla : vanilla * Math.Max(0f, Math.Min(MaxStrength, strength));

        public static bool TryParseStrength(string arg, out float value)
        {
            value = 0f;
            if (arg == null || !float.TryParse(arg.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float v))
                return false;
            if (float.IsNaN(v) || float.IsInfinity(v) || v < 0f || v > MaxStrength)
                return false;
            value = v;
            return true;
        }
    }
}
