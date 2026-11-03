using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Plumb.Core.Model;
using UnityEngine;

namespace Plumb
{
    /// <summary>
    /// Scales <c>_RippleDistance</c> on shared materials. Runs on world load and on setting changes only:
    /// nothing per piece or per frame, so a big base costs the same as an empty one.
    /// </summary>
    internal static class Ripple
    {
        private static readonly int ID = Shader.PropertyToID("_RippleDistance");
        private static readonly VanillaTable Table = new VanillaTable();
        private static readonly Dictionary<Material, float> Vanilla = new Dictionary<Material, float>();
        private static bool _compare;
        private static bool _scannedOnce;

        public static int MaterialCount => Vanilla.Count;

        public static bool Compare
        {
            get => _compare;
            set { _compare = value; Apply(); }
        }

        public static void Scan()
        {
            foreach (Material m in Resources.FindObjectsOfTypeAll<Material>())
            {
                if (m == null || !m.HasProperty(ID) || Vanilla.ContainsKey(m))
                    continue;
                // Nothing is scaled before the first scan, so its values are all true vanilla.
                Vanilla[m] = Table.Record(m.name, m.GetFloat(ID), trusted: !_scannedOnce);
            }

            if (!_scannedOnce)
            {
                _scannedOnce = true;
                var values = Vanilla.Values.GroupBy(v => v).OrderBy(g => g.Key)
                    .Select(g => g.Key.ToString("0.####", CultureInfo.InvariantCulture) + "×" + g.Count());
                // Values this large are unlikely to be a building warp; name them so we know what else shares the property.
                var large = Vanilla.Where(kv => kv.Value >= 1f).Select(kv => kv.Key).Select(m => VanillaTable.BaseName(m.name) + "@" + m.shader.name).Distinct().OrderBy(n => n);
                PlumbPlugin.Log.LogInfo("Plumb: materials with vanilla >= 1: " + string.Join(", ", large));
                PlumbPlugin.Log.LogInfo("Plumb: " + Vanilla.Count + " materials (" + Table.Count + " names), vanilla values: " + string.Join(", ", values));
            }
        }

        public static void Apply()
        {
            foreach (Material dead in Vanilla.Keys.Where(m => m == null).ToList())
                Vanilla.Remove(dead);
            float strength = PluginConfig.Strength.Value;
            foreach (var kv in Vanilla)
                kv.Key.SetFloat(ID, WarpModel.Target(kv.Value, strength, _compare));
        }

        public static string Describe() =>
            "Plumb: strength " + PluginConfig.Strength.Value.ToString("0.###", CultureInfo.InvariantCulture)
            + (_compare ? " (comparing: showing vanilla)" : "") + ", " + Vanilla.Count + " materials";
    }
}
