using System.Collections.Generic;
using UnityEngine;

namespace Minimap.Runtime
{
    public static class ImguiWindowsTracker
    {
        public class WindowInfo
        {
            public string name;
            public Rect rect;
        }

        public static Dictionary<string, WindowInfo> Windows = new();

        public static void UpdateWindow(string name, Rect rect)
        {
            if (!Windows.ContainsKey(name))
                Windows[name] = new WindowInfo { name = name };

            Windows[name].rect = rect;
        }
    }
}
