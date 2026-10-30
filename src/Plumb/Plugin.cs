using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace Plumb
{
    /// <summary>
    /// Removes the deliberate warping on building pieces by scaling the piece shader's
    /// <c>_RippleDistance</c> on shared materials. Purely client-side and visual.
    /// </summary>
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInProcess("valheim.exe")]
    [BepInProcess("valheim.x86_64")]
    public sealed class PlumbPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.jumpingmushroom.plumb";
        public const string PluginName = "Plumb";
        public const string PluginVersion = "0.1.0";

        internal static ManualLogSource Log;

        private static readonly HashSet<string> Warned = new HashSet<string>();
        private Harmony _harmony;

        private void Awake()
        {
            Log = Logger;
            PluginConfig.Bind(base.Config);
            PlumbConsole.Register();

            // One class at a time: a target that moved in a game update costs that patch, not all of them.
            _harmony = new Harmony(PluginGuid);
            int ok = 0, failed = 0;
            foreach (Type t in typeof(PlumbPlugin).Assembly.GetTypes())
            {
                if (!t.IsDefined(typeof(HarmonyPatch), false))
                    continue;
                try
                {
                    _harmony.CreateClassProcessor(t).Patch();
                    ok++;
                }
                catch (Exception e)
                {
                    failed++;
                    Logger.LogWarning("Plumb: patch " + t.Name + " failed: " + e.Message);
                }
            }

            Logger.LogInfo(PluginName + " " + PluginVersion + " loaded (" + ok + " patches, " + failed + " failed).");
        }

        private void Start()
        {
            // Both mods write _RippleDistance; whichever runs last wins.
            foreach (var info in BepInEx.Bootstrap.Chainloader.PluginInfos.Values)
            {
                string id = info.Metadata.GUID + " " + info.Metadata.Name;
                if (id.IndexOf("StraightStructures", StringComparison.OrdinalIgnoreCase) >= 0
                    || id.IndexOf("Straight Structures", StringComparison.OrdinalIgnoreCase) >= 0)
                    Logger.LogWarning("Badgers Straight Structures is also installed; both change the same setting and the last one to run wins. Remove one.");
            }
        }

        private void Update()
        {
            if (Player.m_localPlayer == null || !PluginConfig.CompareKey.Value.IsDown())
                return;
            if (Console.IsVisible() || (Chat.instance != null && Chat.instance.HasFocus()))
                return;
            Ripple.Compare = !Ripple.Compare;
            Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, Ripple.Compare ? "Plumb: vanilla warp" : "Plumb: your strength");
        }

        private void OnDestroy()
        {
            if (_harmony != null)
                _harmony.UnpatchSelf();
        }

        /// <summary>Log an exception once per key, so a broken patch can't flood the log every frame.</summary>
        internal static void WarnOnce(string key, Exception e)
        {
            if (Warned.Add(key))
                Log.LogWarning(key + ": " + e);
        }
    }
}
