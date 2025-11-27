using System.Collections.Generic;
using ImGuiNET;
using Slate.Runtime;
using UnityEngine;

public class DrawGraph : WindowBaseBehaviour
{
    #region Main Window Layout

    protected override void WindowLayout()
    {
        DrawTopToolbar();
        DrawGraphZone();
        DrawInspectorPanel();
    }

    #endregion


    #region Draw Features

    private void DrawTopToolbar()
    {
        if (ImGui.Button("Selection"))
            _action = DrawAction.None;
        ImGui.SameLine();

        if (ImGui.Button("Draw Rectangle"))
            _action = DrawAction.Rectangle;

        if (_action != _lastAction)
        {
            _isDrawing = false;
            _lastAction = _action;
        }
    }

    private void DrawGraphZone()
    {
        Vector2 inspectorOffset = new Vector2(_inspectorSize.x + 32f, 0f);
        ImGui.BeginChild("DrawZone", ImGui.GetWindowSize() - inspectorOffset);

        var bg = ImGui.GetBackgroundDrawList();
        var fg = ImGui.GetWindowDrawList();

        Vector2 winPos = ImGui.GetCursorScreenPos();
        Vector2 mouse = ImGui.GetIO().MousePos;
        Vector2 mouseLocal = mouse - winPos;

        _isDrawGraphHovered = ImGui.IsWindowHovered(ImGuiHoveredFlags.AllowWhenBlockedByActiveItem);

        if (_isDrawGraphHovered)
            HandleDrawActions(mouseLocal, winPos, bg, fg);

        DrawAllRectangles(winPos, fg);
        DrawAllLines(winPos, bg);

        ImGui.EndChild();
    }

    private void HandleDrawActions(Vector2 mouseLocal, Vector2 origin, ImDrawListPtr bg, ImDrawListPtr fg)
    {
        switch (_action)
        {
            case DrawAction.None: break;
            case DrawAction.Line: DrawLine(mouseLocal, origin, bg); break;
            case DrawAction.Rectangle: DrawRectangle(mouseLocal, origin, fg); break;
        }
    }


    private void DrawLine(Vector2 mouseLocal, Vector2 origin, ImDrawListPtr drawList)
    {
        _endPos = mouseLocal;

        uint previewColor = ImGui.ColorConvertFloat4ToU32(
            new Vector4(
                _previewLineColor.r / 255f,
                _previewLineColor.g / 255f,
                _previewLineColor.b / 255f,
                _previewLineColor.a / 255f
            )
        );

        drawList.AddLine(origin + _startPos, origin + _endPos, previewColor, 2f);
    }

    private void DrawRectangle(Vector2 mouseLocal, Vector2 origin, ImDrawListPtr drawList)
    {
        if (!_isDrawing && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            _isDrawing = true;

        if (_isDrawing && ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            _startPos = mouseLocal;
            _endPos = mouseLocal + _rectSize;
        }

        if (_isDrawing && ImGui.IsMouseReleased(ImGuiMouseButton.Left))
        {
            _isDrawing = false;

            _rects.Add(new RectData
            {
                pos = Vector2.Min(_startPos, _endPos),
                size = Vector2.Max(_startPos, _endPos) - Vector2.Min(_startPos, _endPos)
            });
        }

        if (_isDrawing)
            DrawRectanglePreview(origin, drawList);
    }

    private void DrawRectanglePreview(Vector2 origin, ImDrawListPtr drawList)
    {
        Vector2 min = Vector2.Min(_startPos, _endPos);
        Vector2 max = Vector2.Max(_startPos, _endPos);

        uint fill = ImGui.ColorConvertFloat4ToU32(
            new Vector4(
                _previewRectColor.r / 255f,
                _previewRectColor.g / 255f,
                _previewRectColor.b / 255f,
                _previewRectColor.a / 255f
            )
        );

        drawList.AddRectFilled(origin + min, origin + max, fill);
        drawList.AddRect(origin + min, origin + max, ImGui.GetColorU32(ImGuiCol.Border), 0f, ImDrawFlags.None, 2f);
    }

    private void DrawAllRectangles(Vector2 origin, ImDrawListPtr drawList)
    {
        uint fillColor = ImGui.ColorConvertFloat4ToU32(
            new Vector4(
                _finalRectColor.r / 255f,
                _finalRectColor.g / 255f,
                _finalRectColor.b / 255f,
                _finalRectColor.a / 255f
            )
        );

        for (int i = 0; i < _rects.Count; i++)
            DrawSingleRectangle(i, origin, drawList, fillColor);
    }

    private void DrawSingleRectangle(int i, Vector2 origin, ImDrawListPtr drawList, uint fillColor)
    {
        RectData rect = _rects[i];

        Vector2 p1 = origin + rect.pos;
        Vector2 p2 = p1 + rect.size;

        drawList.AddRectFilled(p1, p2, fillColor);
        drawList.AddRect(p1, p2, ImGui.GetColorU32(ImGuiCol.Border), 0f, ImDrawFlags.None, 2f);

        DrawRectangleText(rect, p1, drawList);
        HandleRectangleContextArea(i, p1);
        HandleRectangleClicks(i);
    }

    private void DrawRectangleText(RectData rect, Vector2 p1, ImDrawListPtr dl)
    {
        Vector2 textSize = ImGui.CalcTextSize(rect.name);
        Vector2 textPos = p1 + (rect.size - textSize) * 0.5f;

        textPos.x = Mathf.Max(textPos.x, p1.x + 5);
        textPos.y = Mathf.Max(textPos.y, p1.y + 5);

        ImGui.SetCursorScreenPos(textPos);
        ImGui.PushID(rect.GetHashCode());
        ImGui.Text(rect.name);
        ImGui.PopID();
    }

    private void HandleRectangleContextArea(int i, Vector2 p1)
    {
        ImGui.SetCursorScreenPos(p1);
        ImGui.InvisibleButton($"rect_btn_{i}", _rects[i].size);

        if (ImGui.IsItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Right))
            ImGui.OpenPopup($"rectMenu{i}");

        DrawRectangleContextMenu(i);
    }

    private void HandleRectangleClicks(int i)
    {
        RectData rect = _rects[i];

        if (_action == DrawAction.Line &&
            ImGui.IsItemHovered() &&
            ImGui.IsMouseClicked(ImGuiMouseButton.Left))
        {
            TryCompleteLine(rect);
            return;
        }

        if (_action == DrawAction.None &&
            ImGui.IsItemHovered() &&
            ImGui.IsMouseClicked(ImGuiMouseButton.Left))
        {
            _selectRectData = rect;
        }
    }

    private void DrawRectangleContextMenu(int i)
    {
        ImGui.PushStyleColor(ImGuiCol.PopupBg, new Vector4(0f, 0f, 0f, 0.6f));

        if (ImGui.BeginPopup($"rectMenu{i}"))
        {
            if (ImGui.MenuItem("Make a line"))
                StartLineFromRectangle(_rects[i]);

            if (ImGui.MenuItem("Delete this rectangle"))
                DeleteRectangle(i);

            ImGui.EndPopup();
        }

        ImGui.PopStyleColor();
    }

    private void StartLineFromRectangle(RectData rect)
    {
        _action = DrawAction.Line;
        _startPos = rect.Center;
        _lineStartRect = rect;
    }

    private void TryCompleteLine(RectData target)
    {
        if (target == _lineStartRect) return;

        LineData line = new LineData
        {
            a = _startPos,
            b = target.Center,
            startRect = _lineStartRect,
            endRect = target
        };

        _lines.Add(line);
        _lineStartRect.line.Add(line);
        target.line.Add(line);

        _action = DrawAction.None;
    }

    private void DeleteRectangle(int index)
    {
        if (index < 0 || index >= _rects.Count)
            return;
        RectData rectToDelete = _rects[index];
        
        for (int i = _lines.Count - 1; i >= 0; i--)
        {
            if (_lines[i].startRect == rectToDelete || _lines[i].endRect == rectToDelete)
            {
                
                if (_lines[i].startRect != null)
                    _lines[i].startRect.line.Remove(_lines[i]);
                if (_lines[i].endRect != null)
                    _lines[i].endRect.line.Remove(_lines[i]);
                _lines.RemoveAt(i);
            }
        }
        
        _rects.RemoveAt(index);
        
        if (_selectRectData == rectToDelete)
            _selectRectData = null;
    }

    private void DrawAllLines(Vector2 origin, ImDrawListPtr drawList)
    {
        uint col = ImGui.ColorConvertFloat4ToU32(
            new Vector4(
                _finalLineColor.r / 255f,
                _finalLineColor.g / 255f,
                _finalLineColor.b / 255f,
                _finalLineColor.a / 255f
            )
        );

        foreach (LineData line in _lines)
            drawList.AddLine(origin + line.a, origin + line.b, col, 2.5f);
    }

    #endregion


    #region Inspector Behaviour

    private void DrawInspectorPanel()
    {
        ImGui.SameLine();
        ImGui.BeginChild("Inspector", _inspectorSize);

        if (_selectRectData == null)
        {
            ImGui.Text("No selection");
            ImGui.EndChild();
            return;
        }

        ImGui.Text("Inspector");
        ImGui.Separator();

        DrawInspectorNameField();
        DrawInspectorTransitions();

        ImGui.EndChild();
    }

    private void DrawInspectorNameField()
    {
        ImGui.Text("Name");
        ImGui.SameLine(100);

        var name = _selectRectData.name;
        if (ImGui.InputText("##Name", ref name, 32))
            _selectRectData.name = name;
    }

    private void DrawInspectorTransitions()
    {
        ImGui.Text("Transitions:");
        foreach (LineData line in _selectRectData.line)
        {
            ImGui.Text(line.startRect.name);
            ImGui.SameLine(100);
            ImGui.Text("--->");
            ImGui.SameLine();
            ImGui.Text(line.endRect.name);
        }
    }

    #endregion


        #region Private and Protected

    private enum DrawAction { None, Line, Rectangle }

    private class LineData
    {
        public Vector2 a, b;
        public RectData endRect;
        public RectData startRect;
    }

    private class RectData
    {
        public string name = "New node";
        public Vector2 pos;
        public Vector2 size;
        public Vector2 Center => pos + size * 0.5f;
        
        public List<LineData> line = new();
    }

    private List<LineData> _lines = new();
    private List<RectData> _rects = new();
    private RectData _lineStartRect;
    private RectData _selectRectData;

    private bool _isDrawGraphHovered;
    private bool _isDrawing = false;
    private Vector2 _startPos, _endPos;
    private DrawAction _action = DrawAction.None;
    private DrawAction _lastAction = DrawAction.None;

    [SerializeField] private Vector2 _rectSize;
    [SerializeField] private Vector2 _inspectorSize = new Vector2(250, 100);

    [Header("Color")]
    [SerializeField] private Color32 _finalRectColor;

    [SerializeField] private Color32 _previewRectColor;
    [SerializeField] private Color32 _finalLineColor;
    [SerializeField] private Color32 _previewLineColor;

    #endregion
}
