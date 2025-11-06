using Foundation.Runtime;
using UnityEngine;

namespace Slate.Runtime
{
    public class BoundingBoxUI : FBehaviour
    {
        public Rect boxRect = new Rect(100, 100, 200, 150);

        void OnGUI()
        {
            GUI.color = Color.red;

            GUI.Box(boxRect, "Bounding Box");

            GUI.Label(new Rect(boxRect.x + 10, boxRect.y + 30, 180, 20), "Visible en build !");
        }
    }
}
