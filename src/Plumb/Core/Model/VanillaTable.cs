using System.Collections.Generic;

namespace Plumb.Core.Model
{
    /// <summary>
    /// Decides each material's vanilla ripple value. On the first scan nothing has been scaled yet, so every
    /// material's own value is trusted. A material first seen later may be a runtime copy ("X (Instance)", or the
    /// ghost's new Material(X)) already carrying the scaled value, so it takes the value recorded for its base name.
    /// </summary>
    // shortcut: a later-seen copy of one of several same-named materials takes the first one's value; upgrade if that shows up wrong in-game.
    public sealed class VanillaTable
    {
        private const string InstanceSuffix = " (Instance)";
        private readonly Dictionary<string, float> _vanilla = new Dictionary<string, float>();

        public int Count => _vanilla.Count;

        public float Record(string materialName, float current, bool trusted)
        {
            string key = BaseName(materialName);
            if (!_vanilla.TryGetValue(key, out float v))
            {
                _vanilla[key] = current;
                return current;
            }
            return trusted ? current : v;
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
