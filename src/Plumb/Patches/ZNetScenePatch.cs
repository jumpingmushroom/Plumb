using System;
using HarmonyLib;

namespace Plumb.Patches
{
    [HarmonyPatch(typeof(ZNetScene), "Awake")]
    internal static class ZNetScenePatch
    {
        private static void Postfix()
        {
            try
            {
                Ripple.Scan();
                Ripple.Apply();
            }
            catch (Exception e)
            {
                PlumbPlugin.WarnOnce("Plumb: scan on world load", e);
            }
        }
    }
}
