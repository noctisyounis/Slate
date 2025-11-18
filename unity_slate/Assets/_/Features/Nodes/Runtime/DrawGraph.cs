using System.Collections.Generic;
using ImGuiNET;
using UnityEngine;

public class DrawGraph : MonoBehaviour
{
    private void OnEnable() =>
        UImGui.UImGuiUtility.Layout += OnImGuiLayout;

    private void OnDisable() =>
        UImGui.UImGuiUtility.Layout -= OnImGuiLayout;

    private void OnImGuiLayout(UImGui.UImGui uImGui)
    {
        ImGui.Begin("DrawGraph");

        var drawList = ImGui.GetWindowDrawList();

        Vector2 winContentPos = ImGui.GetCursorScreenPos(); 
        Vector2 mouse = ImGui.GetIO().MousePos;
        Vector2 mouseLocal = mouse - winContentPos;

        // Reset drag if action switched
        if (action != lastAction)
        {
            isDrawing = false;
            lastAction = action;
        }

        switch (action)
        {
            case DrawAction.None:
                break;

            case DrawAction.Line:
                DrawLine(mouseLocal, winContentPos, drawList);
                break;

            case DrawAction.Rectangle:
                DrawRectangle(mouseLocal, winContentPos, drawList);
                break;
        }

        // Draw final rectangles
        uint fillFinal = ImGui.GetColorU32(new Vector4(0.2f, 0.7f, 1f, 0.5f));
        uint outlineCol = ImGui.GetColorU32(ImGuiCol.Border);

        foreach (var rect in rects)
        {
            Vector2 p1 = winContentPos + rect.pos;
            Vector2 p2 = p1 + rect.size;

            drawList.AddRectFilled(p1, p2, fillFinal);
            drawList.AddRect(p1, p2, outlineCol, 0f, ImDrawFlags.None, 2f);
        }

        // Draw final lines
        uint lineColor = ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 1f));

        foreach (LineData line in lines)
        {
            Vector2 p1 = winContentPos + line.a;
            Vector2 p2 = winContentPos + line.b;
            drawList.AddLine(p1, p2, lineColor, 2.5f);
        }

        ImGui.Dummy(new Vector2(800, 600));
        ImGui.End();
    }

    // DRAW LINE
    private void DrawLine(Vector2 mouseLocal, Vector2 origin, ImDrawListPtr drawList)
    {
        if (!isDrawing && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
        {
            isDrawing = true;
            startPos = mouseLocal;
            endPos = mouseLocal;
        }

        if (isDrawing && ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            endPos = mouseLocal;
        }

        if (isDrawing && ImGui.IsMouseReleased(ImGuiMouseButton.Left))
        {
            isDrawing = false;

            lines.Add(new LineData()
            {
                a = startPos,
                b = endPos
            });
        }

        if (isDrawing)
        {
            Vector2 p1 = origin + startPos;
            Vector2 p2 = origin + endPos;
            uint preview = ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 0.4f));
            drawList.AddLine(p1, p2, preview, 2f);
        }
    }

    // DRAW RECTANGLE
    private void DrawRectangle(Vector2 mouseLocal, Vector2 origin, ImDrawListPtr drawList)
    {
        if (!isDrawing && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
        {
            isDrawing = true;
            startPos = mouseLocal;
            endPos = mouseLocal;
        }

        if (isDrawing && ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            endPos = mouseLocal;
        }

        if (isDrawing && ImGui.IsMouseReleased(ImGuiMouseButton.Left))
        {
            isDrawing = false;

            Vector2 min = Vector2.Min(startPos, endPos);
            Vector2 max = Vector2.Max(startPos, endPos);

            rects.Add(new RectData()
            {
                pos = min,
                size = max - min
            });
        }

        if (isDrawing)
        {
            Vector2 min = Vector2.Min(startPos, endPos);
            Vector2 max = Vector2.Max(startPos, endPos);

            Vector2 p1 = origin + min;
            Vector2 p2 = origin + max;

            uint preview = ImGui.GetColorU32(new Vector4(0.2f, 0.7f, 1f, 0.3f));
            uint outline = ImGui.GetColorU32(ImGuiCol.Border);

            drawList.AddRectFilled(p1, p2, preview);
            drawList.AddRect(p1, p2, outline, 0, ImDrawFlags.None, 2f);
        }
    }

    // Data
    private enum DrawAction { None, Line, Rectangle }

    private class LineData
    {
        public Vector2 a, b;
    }

    private class RectData
    {
        public Vector2 pos, size;
    }

    private List<LineData> lines = new();
    private List<RectData> rects = new();

    private bool isDrawing = false;
    private Vector2 startPos, endPos;
    private DrawAction lastAction = DrawAction.None;

    [SerializeField] private DrawAction action = DrawAction.None;
}