using UnityEngine;

namespace SharedData.Runtime
{
    [CreateAssetMenu(fileName = "ImGuiTheme", menuName = "Style/ImGui Theme", order = 0)]
    public class ImGUIThemeAsset : ScriptableObject
    {
        [Header("Layout")]
        public float alpha = 1f;
        public Vector2 windowPadding = new Vector2(8, 8);
        public float windowRounding = 0f;
        public float windowBorderSize = 1f;
        public Vector2 windowMinSize = new Vector2(32, 32);
        public Vector2 windowTitleAlign = new Vector2(0f, 0.5f);

        public float childRounding = 0f;
        public float childBorderSize = 0f;
        public float popupRounding = 0f;
        public float popupBorderSize = 0f;

        public Vector2 framePadding = new Vector2(4, 3);
        public float frameRounding = 0f;
        public float frameBorderSize = 0f;

        public Vector2 itemSpacing = new Vector2(8, 4);
        public Vector2 itemInnerSpacing = new Vector2(4, 4);
        public Vector2 cellPadding = new Vector2(0, 0);

        public float indentSpacing = 21f;
        public float scrollbarSize = 14f;
        public float scrollbarRounding = 9f;
        public float grabMinSize = 10f;
        public float grabRounding = 0f;

        public float tabRounding = 4f;
        public float tabBorderSize  = 0f;

        public Vector2 buttonTextAlign = new Vector2(0.5f, 0.5f);
        public Vector2 selectableTextAlign = new Vector2(0f, 0f);

        public Vector2 displayWindowPadding = new Vector2(19, 19);
        public Vector2 displaySafeAreaPadding = new Vector2(3, 3);

        public float mouseCursorScale = 1f;
        public float curveTessellationTol = 2f;
        public float circleTessellationMaxError = 2f;
        
        [Header("Core")]
        public Color text = Color.white;
        public Color textDisabled = Color.grey;
        public Color windowBg = new Color(0.06f, 0.06f, 0.06f, 1f);
        public Color childBg = new Color(0, 0, 0, 0);
        public Color popupBg = new Color(0.08f, 0.08f, 0.08f, 0.94f);
        public Color border = new Color(0.43f, 0.43f, 0.50f, 0.50f);
        public Color borderShadow = new Color(0, 0, 0, 0);

        [Header("Frames / Buttons")]
        public Color frameBg = new Color(0.20f, 0.25f, 0.30f, 1f);
        public Color frameBgHovered = new Color(0.26f, 0.59f, 0.98f, 1f);
        public Color frameBgActive = new Color(0.26f, 0.59f, 0.98f, 1f);

        public Color button = new Color(0.26f, 0.59f, 0.98f, 0.40f);
        public Color buttonHovered = new Color(0.26f, 0.59f, 0.98f, 1f);
        public Color buttonActive = new Color(0.06f, 0.53f, 0.98f, 1f);

        [Header("Headers / Tabs / Menus")]
        public Color header = new Color(0.26f, 0.59f, 0.98f, 0.31f);
        public Color headerHovered = new Color(0.26f, 0.59f, 0.98f, 0.80f);
        public Color headerActive = new Color(0.26f, 0.59f, 0.98f, 1f);

        public Color menuBarBg = new Color(0.14f, 0.14f, 0.14f, 1f);
        public Color titleBg = new Color(0.04f, 0.04f, 0.04f, 1f);
        public Color titleBgActive = new Color(0.16f, 0.29f, 0.48f, 1f);
        public Color titleBgCollapsed = new Color(0, 0, 0, 0.51f);

        [Header("Ghost Buttons")]
        public Color ghostButton = new Color(0, 0, 0, 0);
        public Color ghostHover = new Color(1, 1, 1, 0.08f);
        public Color ghostActive = new Color(1, 1, 1, 0.12f);
    }
}