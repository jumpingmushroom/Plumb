using Plumb.Core.Model;

namespace Plumb
{
    /// <summary>"plumb" prints the state; "plumb X" sets and saves the strength.</summary>
    internal static class PlumbConsole
    {
        public static void Register()
        {
            new Terminal.ConsoleCommand("plumb", "[0-3] Plumb: building warp strength (0 = straight, 1 = vanilla)",
                delegate (Terminal.ConsoleEventArgs args)
                {
                    if (args.Length > 1)
                    {
                        if (!WarpModel.TryParseStrength(args[1], out float v))
                        {
                            args.Context.AddString("plumb [0-3]   0 = straight, 1 = vanilla");
                            return;
                        }
                        PluginConfig.Strength.Value = v;
                    }
                    args.Context.AddString(Ripple.Describe());
                });
        }
    }
}
