using System;
using System.Collections.Generic;
using System.Linq;
using ImGuiNET;
using Slate.Runtime;
using UnityEngine;

namespace WindowCreator.Runtime
{
    /// <summary>
    /// EnumDatabaseEditor
    /// ------------------
    /// Fenêtre ImGui de type RPG Maker MV permettant :
    /// - création / suppression / renommage de listes EnumRegistry
    /// - édition des valeurs d'une liste
    /// - sauvegarde automatique dans Latest/data/database/EnumRegistry.json
    ///
    /// Dépend de WindowBaseBehaviour :
    /// - déplacement libre
    /// - masquage/affichage tools
    /// </summary>
    public class EnumDatabaseEditor : WindowBaseBehaviour
    {
        #region Publics

        [Header("Window")]
        public string m_windowTitle = "Enum Database Editor";

        [Header("Debug")]
        public bool m_showDebug = false;

        #endregion


        #region API Unity (Awake, Start, Update, etc.)

        private void Awake()
        {
            EnumRegistry.Initialize();
            _selectedListKey = EnumRegistry.m_keys.FirstOrDefault();
        }

        protected override void OnDisable()
        {
            EnumRegistry.SaveIfDirty();
        }

        private void OnApplicationQuit()
        {
            EnumRegistry.SaveIfDirty();
        }

        protected override void WindowLayout()
        {
            ImGui.SetNextWindowSize(new Vector2(900, 650), ImGuiCond.FirstUseEver);
            ImGui.Begin(m_windowTitle);

            DrawListsPanel();
            ImGui.SameLine();
            Splitter("split_lists_values", ref _colListsWidth, ref _colValuesWidth, _minListsWidth, _minValuesWidth);
            ImGui.SameLine();
            DrawValuesPanel();

            if (m_showDebug)
            {
                ImGui.Separator();
                ImGui.TextDisabled($"Selected List: {_selectedListKey}");
                ImGui.TextDisabled($"Lists Count: {EnumRegistry.m_keys.Count()}");
            }

            ImGui.End();
        }

        #endregion


        #region Utils (méthodes publics)

        #endregion


        #region Main Methods (méthodes private)

        private void DrawListsPanel()
        {
            ImGui.BeginChild("ListsPanel", new Vector2(_colListsWidth, 0f), ImGuiChildFlags.Border);

            ImGui.Text("Enum Lists");
            ImGui.Separator();

            var keys = EnumRegistry.m_keys.ToList();

            if (keys.Count == 0)
                ImGui.TextDisabled("No lists yet.");

            foreach (var key in keys)
            {
                bool selected = key == _selectedListKey;
                if (ImGui.Selectable(key, selected))
                    _selectedListKey = key;
            }

            ImGui.Spacing();
            ImGui.Separator();

            // Create list
            ImGui.Text("New List Name");
            _newListName ??= string.Empty;
            ImGui.SetNextItemWidth(180);
            ImGui.InputText("##NewListName", ref _newListName, 64);

            if (ImGui.Button("+ Add List"))
            {
                if (string.IsNullOrWhiteSpace(_newListName))
                    _newListName = "NewList";

                if (EnumRegistry.AddList(_newListName))
                    _selectedListKey = _newListName;

                _newListName = string.Empty;
            }

            ImGui.Spacing();
            ImGui.Separator();

            // Rename list
            if (!string.IsNullOrEmpty(_selectedListKey))
            {
                ImGui.Text("Rename Selected");
                _renameListName ??= _selectedListKey;

                ImGui.SetNextItemWidth(180);
                ImGui.InputText("##RenameListName", ref _renameListName, 64);

                if (ImGui.Button("Rename"))
                {
                    if (EnumRegistry.RenameList(_selectedListKey, _renameListName))
                        _selectedListKey = _renameListName;
                }

                ImGui.SameLine();
                if (ImGui.Button("Delete"))
                {
                    EnumRegistry.RemoveList(_selectedListKey);
                    _selectedListKey = EnumRegistry.m_keys.FirstOrDefault();
                }
            }

            ImGui.EndChild();
        }

        private void DrawValuesPanel()
        {
            ImGui.BeginChild("ValuesPanel", new Vector2(_colValuesWidth, 0f), ImGuiChildFlags.Border);

            if (string.IsNullOrEmpty(_selectedListKey))
            {
                ImGui.TextDisabled("Select or create a list.");
                ImGui.EndChild();
                return;
            }

            var list = EnumRegistry.Get(_selectedListKey);
            if (list == null)
            {
                ImGui.TextDisabled("List not found.");
                ImGui.EndChild();
                return;
            }

            ImGui.Text($"Values - {_selectedListKey}");
            ImGui.Separator();

            // Values
            for (int i = 0; i < list.Count; i++)
            {
                string value = list[i] ?? string.Empty;

                ImGui.PushID(i);

                ImGui.SetNextItemWidth(260);
                if (ImGui.InputText("##Value", ref value, 128))
                {
                    list[i] = value;
                    EnumRegistry.Save(); // édition immédiate
                }

                ImGui.SameLine();
                if (ImGui.SmallButton("X"))
                {
                    list.RemoveAt(i);
                    EnumRegistry.Save();
                    ImGui.PopID();
                    break;
                }

                ImGui.PopID();
            }

            ImGui.Spacing();
            ImGui.Separator();

            // Add value
            ImGui.Text("New Value");
            _newValueName ??= string.Empty;

            ImGui.SetNextItemWidth(260);
            ImGui.InputText("##NewValueName", ref _newValueName, 128);

            if (ImGui.Button("+ Add Value"))
            {
                if (string.IsNullOrWhiteSpace(_newValueName))
                    _newValueName = "NewValue";

                if (!list.Contains(_newValueName))
                {
                    list.Add(_newValueName);
                    EnumRegistry.Save();
                }

                _newValueName = string.Empty;
            }

            ImGui.EndChild();
        }

        private void Splitter(string id, ref float left, ref float right, float minLeft, float minRight)
        {
            ImGui.PushID(id);

            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.35f, 0.35f, 0.35f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.45f, 0.45f, 0.45f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.55f, 0.55f, 0.55f, 1.0f));

            ImGui.Button("##splitter", new Vector2(_splitterSize, -1f));

            ImGui.PopStyleColor(3);

            if (ImGui.IsItemActive())
            {
                float delta = ImGui.GetIO().MouseDelta.x;
                left += delta;
                right -= delta;

                if (left < minLeft) { right -= (minLeft - left); left = minLeft; }
                if (right < minRight) { left -= (minRight - right); right = minRight; }
            }

            ImGui.PopID();
        }

        #endregion


        #region Private and Protected

        private string _selectedListKey;
        private string _newListName;
        private string _renameListName;
        private string _newValueName;

        private float _colListsWidth = 260f;
        private float _colValuesWidth = 600f;

        private const float _splitterSize = 6f;
        private const float _minListsWidth = 180f;
        private const float _minValuesWidth = 240f;

        #endregion
    }
}
