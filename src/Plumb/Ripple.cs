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
        private static readonly HashSet<Material> Materials = new HashSet<Material>();
        private static bool _compare;
        private static bool _loggedFirstScan;

        public static int MaterialCount => Materials.Count;

        public static bool Compare
        {
            get => _compare;
            set { _compare = value; Apply(); }
        }

        public static void Scan()
        {
            foreach (Material m in Resources.FindObjectsOfTypeAll<Material>())
            {
                if (m == null || !m.HasProperty(ID) || !Materials.Add(m))
                    continue;
                Table.Resolve(m.name, m.GetFloat(ID));
            }

            if (!_loggedFirstScan)
            {
                _loggedFirstScan = true;
                var values = Materials.Select(m => Table.Resolve(m.name, 0f)).GroupBy(v => v).OrderBy(g => g.Key)
                    .Select(g => g.Key.ToString("0.####", CultureInfo.InvariantCulture) + "×" + g.Count());
                PlumbPlugin.Log.LogInfo("Plumb: " + Materials.Count + " materials (" + Table.Count + " names), vanilla values: " + string.Join(", ", values));
            }
        }

        public static void Apply()
        {
            Materials.RemoveWhere(m => m == null);
            float strength = PluginConfig.Strength.Value;
            foreach (Material m in Materials)
                m.SetFloat(ID, WarpModel.Target(Table.Resolve(m.name, 0f), strength, _compare));
        }

        public static string Describe() =>
            "Plumb: strength " + PluginConfig.Strength.Value.ToString("0.###", CultureInfo.InvariantCulture)
            + (_compare ? " (comparing: showing vanilla)" : "") + ", " + Materials.Count + " materials";
    }
}
