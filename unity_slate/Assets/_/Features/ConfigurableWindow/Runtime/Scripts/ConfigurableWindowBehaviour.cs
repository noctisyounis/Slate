using System;
using Foundation.Runtime;
using UnityEngine;

public class ConfigurableWindowBehaviour : FBehaviour
{
    public GameObject _test;
    private Renderer _renderer;
    private Color _color;
    public int _number;
    public int inputnumber;
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
        SetFact("color", color, true);
        Save();

    }
}