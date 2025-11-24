using System.Collections.Generic;
using ImGuiNET;
using Slate.Runtime;
using UnityEngine;

public class DrawGraph : WindowBaseBehaviour
{
    protected override void WindowLayout()
    {
        
        if (ImGui.Button("None"))
        {
            _action = DrawAction.None;
        }
        ImGui.SameLine();
        
        if (ImGui.Button("Draw Rectangle"))
        {
            _action = DrawAction.Rectangle;
        }
        
        ImGui.SameLine();
        if (ImGui.Button("Erase"))
        {
            _lines.Clear();
            _rects.Clear();
        }
        
        // Reset drag if action switched
        if (_action != _lastAction)
        {
            _isDrawing = false;
            _lastAction = _action;
        }
        
        ImGui.BeginChild("DrawZone", ImGui.GetWindowSize());
        
        var backgroundDrawList = ImGui.GetBackgroundDrawList();
        var foregroundDrawList = ImGui.GetWindowDrawList();
        
        Vector2 winContentPos = ImGui.GetCursorScreenPos(); 
        Vector2 mouse = ImGui.GetIO().MousePos;
        Vector2 mouseLocal = mouse - winContentPos;
        
        bool hovered = ImGui.IsWindowHovered(ImGuiHoveredFlags.AllowWhenBlockedByActiveItem);

        if (hovered)
        {
            switch (_action)
            {
                case DrawAction.None:
                    break;
    
                case DrawAction.Line:
                    DrawLine(mouseLocal, winContentPos, backgroundDrawList);
                    break;
    
                case DrawAction.Rectangle:
                    DrawRectangle(mouseLocal, winContentPos, foregroundDrawList);
                    break;
            }
        }

        // Draw final rectangles
        DrawAllRectangle(winContentPos, foregroundDrawList);

        // Draw final lines
        DrawAllLine(winContentPos, backgroundDrawList);

        ImGui.EndChild();
    }
    
    
#region Utils
    // DRAW LINE
    private void DrawLine(Vector2 mouseLocal, Vector2 origin, ImDrawListPtr drawList)
    {
        
        _endPos = mouseLocal;
        Vector2 p1 = origin + _startPos;
        Vector2 p2 = origin + _endPos;
        uint preview = ImGui.ColorConvertFloat4ToU32(new Vector4(_previewLineColor.r/255f, _previewLineColor.g/255f, _previewLineColor.b/255f, _previewLineColor.a/255f));
        drawList.AddLine(p1, p2, preview, 2f);
    }

    // DRAW RECTANGLE
    private void DrawRectangle(Vector2 mouseLocal, Vector2 origin, ImDrawListPtr drawList)
    {
        if (!_isDrawing && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
        {
            _isDrawing = true;
            // startPos = mouseLocal;
            // endPos = mouseLocal;
        }

        if (_isDrawing && ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            _startPos = mouseLocal;
            _endPos = mouseLocal + _rectSize;
        }

        if (_isDrawing && ImGui.IsMouseReleased(ImGuiMouseButton.Left))
        {
            _isDrawing = false;

            Vector2 min = Vector2.Min(_startPos, _endPos);
            Vector2 max = Vector2.Max(_startPos, _endPos);

            _rects.Add(new RectData()
            {
                pos = min,
                size = max - min
            });
        }

        if (_isDrawing)
        {
            Vector2 min = Vector2.Min(_startPos, _endPos);
            Vector2 max = Vector2.Max(_startPos, _endPos);

            Vector2 p1 = origin + min;
            Vector2 p2 = origin + max;

            uint preview = ImGui.ColorConvertFloat4ToU32(new Vector4(_previewRectColor.r/255f, _previewRectColor.g/255f, _previewRectColor.b/255f, _previewRectColor.a/255f));
            uint outline = ImGui.GetColorU32(ImGuiCol.Border);

            drawList.AddRectFilled(p1, p2, preview);
            drawList.AddRect(p1, p2, outline, 0, ImDrawFlags.None, 2f);
        }
    }
    
    //DRAW ALL RECTANGLE
    private void DrawAllRectangle(Vector2 winContentPos, ImDrawListPtr foregroundDrawList)
    {
        uint fillFinal = ImGui.ColorConvertFloat4ToU32(new Vector4(_finalRectColor.r/255f, _finalRectColor.g/255f, _finalRectColor.b/255f, _finalRectColor.a/255f));
        uint outlineCol = ImGui.GetColorU32(ImGuiCol.Border);

        for (int i = 0; i < _rects.Count ;i++)
        {
            Vector2 p1 = winContentPos + _rects[i].pos;
            Vector2 p2 = p1 + _rects[i].size;

            foregroundDrawList.AddRectFilled(p1, p2, fillFinal);
            foregroundDrawList.AddRect(p1, p2, outlineCol, 0f, ImDrawFlags.None, 2f);
            
            Vector2 textSize = ImGui.CalcTextSize(_rects[i].name);
            
            Vector2 textPos = p1 + (_rects[i].size - textSize) * 0.5f;
            
            textPos.x = Mathf.Max(p1.x + 5f, textPos.x);
            textPos.y = Mathf.Max(p1.y + 5f, textPos.y);
            
            ImGui.SetCursorScreenPos(textPos);
            ImGui.PushID(_rects[i].GetHashCode()); 
            ImGui.Text(_rects[i].name);
            ImGui.PopID();
            
            // --- Right-click context menu ---
            // Invisible button covering the rectangle area to detect right-click
            ImGui.SetCursorScreenPos(p1);
            ImGui.InvisibleButton("rectContext", _rects[i].size);

            if (_action == DrawAction.Line && ImGui.IsItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            {
                if (_rects[i] == _lineStartRect) continue;
                
                _lines.Add(new LineData()
                {
                    a = _startPos,
                    b = _endPos,
                    startRect = _lineStartRect,
                    endRect = _rects[i]
                });
                
                _action = DrawAction.None;
                return;
            }
            
            if (ImGui.IsItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Right))
            {
                ImGui.OpenPopup($"rectMenu{i}");
            }

            if (ImGui.BeginPopup($"rectMenu{i}"))
            {
                if (ImGui.MenuItem("Make a line"))
                {
                    // Start drawing a line from this rectangle
                    _action = DrawAction.Line;
                    _startPos = _rects[i].Center;
                    _lineStartRect = _rects[i];
                }

                if (ImGui.MenuItem("Delete this rectangle"))
                {
                    _rects.RemoveAt(i);
                    i--; // Adjust loop index after removal
                }

                ImGui.EndPopup();
            }
        }
    }
    
    //DRAW ALL LINE
    private void DrawAllLine(Vector2 winContentPos, ImDrawListPtr foregroundDrawList)
    {
        uint lineColor = ImGui.ColorConvertFloat4ToU32(new Vector4(_finalLineColor.r/255f, _finalLineColor.g/255f, _finalLineColor.b/255f, _finalLineColor.a/255f));

        foreach (LineData line in _lines)
        {
            Vector2 p1 = winContentPos + line.a;
            Vector2 p2 = winContentPos + line.b;
            foregroundDrawList.AddLine(p1, p2, lineColor, 2.5f);
        }
    }
#endregion

    // Data
    private enum DrawAction { None, Line, Rectangle }

    private class LineData
    {
        public Vector2 a, b;
        public RectData startRect;
        public RectData endRect;
    }

    private class RectData
    {
        public string name = "New node";
        public Vector2 pos;
        public Vector2 size;
        public Vector2 Center => pos + size * 0.5f;
    }

    private List<LineData> _lines = new();
    private List<RectData> _rects = new();
    private RectData _lineStartRect;

    private bool _isDrawing = false;
    private Vector2 _startPos, _endPos;
    private DrawAction _lastAction = DrawAction.None;

    [SerializeField] private DrawAction _action = DrawAction.None;
    [SerializeField] private Vector2 _rectSize;
    [Header("Color")]
    [SerializeField] private Color32 _finalRectColor;
    [SerializeField] private Color32 _previewRectColor = new Color32(255, 255, 255, 255);
    [SerializeField] private Color32 _finalLineColor = new Color32(255, 255, 255, 255);
    [SerializeField] private Color32 _previewLineColor = new Color32(255, 255, 255, 255);
}