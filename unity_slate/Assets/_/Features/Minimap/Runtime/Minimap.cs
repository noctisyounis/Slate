using Foundation.Runtime;
using UnityEngine;

namespace Minimap.Runtime
{
    public class Minimap : FBehaviour
    {
        public Texture2D minimapTexture;
        [SerializeField] Rect minimapRect = new Rect(0, 0, 200, 200);
        [SerializeField] Color borderColor = Color.white;

        [SerializeField] Vector2 cameraPos = new Vector2(0.5f, 0.5f);
        [SerializeField] Vector2 cameraViewSize = new Vector2(0.2f, 0.2f);

        public Transform m_camera_position;
        [SerializeField] Vector2 worldMin = new Vector2(-50, -50);
        [SerializeField] Vector2 worldMax = new Vector2(50, 50);

        void Update()
        {
            if (m_camera_position)
            {
                cameraPos.x = Mathf.InverseLerp(worldMin.x, worldMax.x, m_camera_position.position.x);
                cameraPos.y = Mathf.InverseLerp(worldMin.y, worldMax.y, m_camera_position.position.y);
            }
        }

        void OnGUI()
        {
            float w = 250, h = 250;
            Rect mapRect = new Rect(Screen.width - w - 20, Screen.height - h - 20, w, h);

            if (minimapTexture)
                GUI.DrawTexture(mapRect, minimapTexture, ScaleMode.ScaleToFit);
            else
                GUI.Box(mapRect, "Minimap");

            float innerW = mapRect.width * cameraViewSize.x;
            float innerH = mapRect.height * cameraViewSize.y;

            float px = mapRect.x + mapRect.width * cameraPos.x - innerW / 2f;
            float py = mapRect.y + mapRect.height * (1 - cameraPos.y) - innerH / 2f;

            Rect viewRect = new Rect(px, py, innerW, innerH);

            DrawDashedRect(viewRect, borderColor, 8f, 4f, 2f);
        }

        void DrawDashedRect(Rect rect, Color color, float dashLength, float gap, float thickness)
        {
            Texture2D tex = Texture2D.whiteTexture;
            GUI.color = color;

            // Top
            DrawDashedLine(rect.xMin, rect.yMin, rect.xMax, rect.yMin, dashLength, gap, thickness, tex);
            // Bottom
            DrawDashedLine(rect.xMin, rect.yMax, rect.xMax, rect.yMax, dashLength, gap, thickness, tex);
            // Left
            DrawDashedLine(rect.xMin, rect.yMin, rect.xMin, rect.yMax, dashLength, gap, thickness, tex);
            // Right
            DrawDashedLine(rect.xMax, rect.yMin, rect.xMax, rect.yMax, dashLength, gap, thickness, tex);
        }

        void DrawDashedLine(float x1, float y1, float x2, float y2, float dash, float gap, float thickness, Texture2D tex)
        {
            float dx = x2 - x1;
            float dy = y2 - y1;
            float length = Mathf.Sqrt(dx * dx + dy * dy);
            float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;

            float drawLength = 0f;
            while (drawLength < length)
            {
                float seg = Mathf.Min(dash, length - drawLength);
                Matrix4x4 matrix = GUI.matrix;
                GUIUtility.RotateAroundPivot(angle, new Vector2(x1 + dx * (drawLength / length), y1 + dy * (drawLength / length)));

                GUI.DrawTexture(new Rect(x1 + dx * (drawLength / length), y1 + dy * (drawLength / length), seg, thickness), tex);
                GUI.matrix = matrix;
                drawLength += dash + gap;
            }
        }
    }
}
