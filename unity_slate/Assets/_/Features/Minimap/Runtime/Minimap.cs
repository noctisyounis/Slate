using Foundation.Runtime;
using UnityEngine;

namespace Minimap.Runtime
{
    public class Minimap : FBehaviour
    {
        [Header("Screen & Map Dimensions")]
        [SerializeField] float baseWidth = 250f;
        [SerializeField] float baseHeight = 250f;
        [SerializeField] float referenceWidth = 1920f;
        [SerializeField] float referenceHeight = 1080f;

        [Header("Zoom Settings")]
        private Camera targetCamera;
        [SerializeField] float zoomMin = 5f;
        [SerializeField] float zoomMax = 50f;

        [Header("Textures & Styles")]
        [SerializeField] Texture2D minimapTexture;
        [SerializeField] Color borderColor = Color.white;

        [Header("Map Settings")]
        [SerializeField] Vector2 cameraViewSize = new Vector2(0.2f, 0.2f);
        [SerializeField] Vector2 worldMin = new Vector2(-50, -50);
        [SerializeField] Vector2 worldMax = new Vector2(50, 50);
        [SerializeField] float minScale = 0.5f;

        [Header("References")]
        public Transform m_camera_position;

        [Header("Visibility Control")]
        [SerializeField] float fadeDuration = 0.3f;
        [SerializeField] float visibleDelay = 0.5f;

        private Vector3 lastPlayerPos;
        private bool isMoving = false;
        private float visibleTimer = 0f;
        private float currentAlpha = 0f;

        private Vector2 cameraPos = new Vector2(0.5f, 0.5f);

        void Start()
        {
            targetCamera = Camera.main;

            if (m_camera_position)
                lastPlayerPos = m_camera_position.position;
        }

        void Update()
        {
            if (!m_camera_position) return;

            cameraPos.x = Mathf.InverseLerp(worldMin.x, worldMax.x, m_camera_position.position.x);
            cameraPos.y = 1f - Mathf.InverseLerp(worldMin.y, worldMax.y, m_camera_position.position.y);

            isMoving = (m_camera_position.position != lastPlayerPos);

            if (isMoving)
                visibleTimer = visibleDelay;

            lastPlayerPos = m_camera_position.position;

            float zoomNormalized = 0.5f;

            if (targetCamera.orthographic)
            {
                zoomNormalized = Mathf.InverseLerp(zoomMin, zoomMax, targetCamera.orthographicSize);
            }
            else
            {
                float camZ = Mathf.Abs(targetCamera.transform.position.z);
                zoomNormalized = Mathf.InverseLerp(zoomMin, zoomMax, camZ);
            }

            float minView = 0.05f;
            float maxView = 0.35f;

            cameraViewSize.x = Mathf.Lerp(minView, maxView, zoomNormalized);

            float screenRatio = (float)Screen.width / Screen.height;
            cameraViewSize.y = cameraViewSize.x / screenRatio;

            float targetAlpha = (visibleTimer > 0f) ? 1f : 0f;
            currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, Time.deltaTime / fadeDuration);

            if (visibleTimer > 0f)
                visibleTimer -= Time.deltaTime;
        }

        void OnGUI()
        {
            if (currentAlpha <= 0f)
                return;

            GUI.color = new Color(1f, 1f, 1f, currentAlpha);

            float scaleX = Screen.width / referenceWidth;
            float scaleY = Screen.height / referenceHeight;
            float scale = Mathf.Min(scaleX, scaleY);
            scale = Mathf.Max(scale, minScale);
            float w = baseWidth * scale;
            float h = baseHeight * scale;
            Rect mapRect = new Rect(Screen.width - w - 20 * scale, Screen.height - h - 20 * scale, w, h);

            if (minimapTexture)
                GUI.DrawTexture(mapRect, minimapTexture, ScaleMode.ScaleToFit);
            else
                GUI.Box(mapRect, "Minimap");

            float innerW = mapRect.width * cameraViewSize.x;
            float innerH = mapRect.height * cameraViewSize.y;
            float px = mapRect.x + mapRect.width * cameraPos.x - innerW / 2f;
            float py = mapRect.y + mapRect.height * cameraPos.y - innerH / 2f;

            Rect viewRect = new Rect(px, py, innerW, innerH);
            DrawDashedRect(viewRect, borderColor, 8f, 4f, 2f);

            GUI.color = Color.white;
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
