using System.Collections.Generic;

namespace Plumb.Core.Model
{
    /// <summary>
    /// The vanilla ripple value per material base name, recorded the first time the name is seen.
    /// Runtime copies ("X (Instance)", or the ghost's new Material(X)) already carry the scaled value,
    /// so they must resolve to the original's vanilla value, never record their own.
    /// </summary>
    // shortcut: two different materials with the same name share one vanilla value; upgrade to per-instance tracking if a mod ships same-named materials with different ripple values.
    public sealed class VanillaTable
    {
        private const string InstanceSuffix = " (Instance)";
        private readonly Dictionary<string, float> _vanilla = new Dictionary<string, float>();

        public int Count => _vanilla.Count;

        public float Resolve(string materialName, float current)
        {
            string key = BaseName(materialName);
            if (_vanilla.TryGetValue(key, out float v))
                return v;
            _vanilla[key] = current;
            return current;
        }

        public static string BaseName(string materialName)
        {
            string s = materialName ?? "";
            while (s.EndsWith(InstanceSuffix))
                s = s.Substring(0, s.Length - InstanceSuffix.Length);
            return s;
        }
    }
}
