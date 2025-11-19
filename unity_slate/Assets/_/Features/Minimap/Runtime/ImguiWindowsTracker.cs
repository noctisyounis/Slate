using UnityEngine;

namespace Minimap.Runtime
{
    public static class ImguiWindowsTracker
    {
        public static readonly List<Rect> OpenWindows = new List<Rect>();

        public static void RegisterWindow(Rect rect)
        {
            OpenWindows.Add(rect);
        }

        public static void Clear()
        {
            OpenWindows.Clear();
        }
    }
}
