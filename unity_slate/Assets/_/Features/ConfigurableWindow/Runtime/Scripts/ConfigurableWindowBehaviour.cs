using System;
using Foundation.Runtime;
using ImGuiNET;
using UImGui;
using UnityEngine;

public class ConfigurableWindowBehaviour : FBehaviour
{
    public GameObject _test;
    private Renderer _renderer;
    private Color _color;
    public int _number;
    public int inputnumber;

    private bool showWindow = true;
    
    private Vector4 colorValue = new Vector4(1, 1, 1, 1);

    private int currentTab = 0; 
    
    private void OnEnable()
    {
        UImGuiUtility.Layout += OnLayout;
    }

    
    private void OnDisable()
    {
        UImGuiUtility.Layout -= OnLayout;
        UImGuiUtility.OnInitialize -= OnInitialize;
        UImGuiUtility.OnDeinitialize -= OnDeinitialize;
    }

    
    private void OnLayout(UImGui.UImGui obj)
    {
        if (!showWindow) return;

        ImGui.SetNextWindowSize(new Vector2(400, 300), ImGuiCond.Once);
        ImGui.Begin("Configurable Window");

        string[] tabs = { "Debug commands", "SlateConfigs" };
        if (ImGui.BeginTabBar("##tabs"))
        {
            if (ImGui.BeginTabItem(tabs[0]))
            {
                ImGui.Text("Debug commands ici");
                if (ImGui.Button("Test Button"))
                {
                    Debug.Log("Test Button clicked");
                }
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem(tabs[1]))
            {
                ImGui.Text("Configure la couleur ici :");
                ImGui.ColorEdit4("Color Picker", ref colorValue);

                ImGui.Spacing();

                if (ImGui.Button("Save"))
                {
                    // Appliquer la couleur au mesh
                    if (_test != null)
                    {
                        var renderer = _test.GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            _color = new Color(colorValue.x, colorValue.y, colorValue.z, colorValue.w);
                            renderer.material.color = _color;
                        }
                    }

                    // Sauvegarder la couleur dans ton système avec SetFact
                    SetFact("color", _color, true);
                    Save();
                }

                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }

        if (ImGui.Button("Fermer"))
        {
            showWindow = false;
        }

        ImGui.End();
    }

    
    private void OnInitialize(UImGui.UImGui obj)
    {
        // runs after UImGui.OnEnable();
    }

    private void OnDeinitialize(UImGui.UImGui obj)
    {
        // runs after UImGui.OnDisable();
    }
    
    
    private void Start()
    {
        
        _renderer = _test.GetComponent<Renderer>();
        //if (!FactExists("color", out _color))
        //{
            Load();
       // }

        if (FactExists("number", out _number));
        if (FactExists("color", out _color))
        {
            _renderer.material.color = _color;
        }
        else _renderer.material.color = Color.white;
        // color = _renderer.material.color;
        //SetFact("color", color, true);
        //Save();
        
        
    }

    [ContextMenu("Set blue")]
    public void SetBlue()
    {
        _number = inputnumber;
        SetFact("number", _number,true);
        
        var color = Color.blue;
        _renderer.material.color = color; 
        
        SetFact("color", color, true);
        Save();

    }
    
    [ContextMenu("Set Red")]
    public void SetRed()
    {
        _number = inputnumber;
        SetFact("number", _number,true);
        
        var color = Color.red;
        _renderer.material.color = color; 
        
        SetFact("color", color, true);
        Save();

    }
    
    [ContextMenu("Set Yellow")]
    public void SetYellow()
    {
        _number = inputnumber;
        SetFact("number", _number,true);
        
        var color = Color.yellow;
        _renderer.material.color = color; 
        
        SetFact("color", color, true);
        Save();

    }
}