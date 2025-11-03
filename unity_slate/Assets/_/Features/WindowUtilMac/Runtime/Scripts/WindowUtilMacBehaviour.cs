using System;
using System.Runtime.InteropServices;
using Foundation.Runtime;

public class WindowUtilMacBehaviour : FBehaviour
{
    [DllImport("no_border_mac")]
    private static extern void MakeWindowBorderless();

    void Start()
    {
        try
        {
            MakeWindowBorderless();
            Info("Fenêtre borderless appliquée");
        }
        catch (DllNotFoundException e)
        {
            Error($"Plugin not found: {e.Message}");
        }
        catch (EntryPointNotFoundException e)
        {
            Error($"Function not found in plugin: {e.Message}");
        }
    }
}