using UnityEngine;

using UImGui;
using Foundation.Runtime;

namespace Slate.Runtime
{
    public class SampleWindow : FBehaviour
    {
        private void OnEnable() => UImGuiUtility.Layout += OnLayout;
        private void OnDisable() => UImGuiUtility.Layout -= OnLayout;

        private void OnLayout(UImGui.UImGui uImGui)
        {

        }

        private void Awake()
        {
            _camera = Camera.main;
            _cameraPan = new CameraPan(_camera,_panSpeed, _mousePanSpeed, _panCursor, _zoomSpeed);
        }

        private void Update()
        {
            _cameraPan.UpdatePan();
            HandleCursor();
        }
        
        private void HandleCursor()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            
            if (_cameraPan.m_isMiddleClickHeld)
            {
                if (_panCursor is not null)
                {
                    Cursor.SetCursor(_panCursor, Vector2.zero, CursorMode.Auto);
                }
            }
            else
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); // curseur par défaut
            }
        }

        [SerializeField] private float _panSpeed = 10f;
        [SerializeField] private float _mousePanSpeed = 10f;
        [SerializeField] private Texture2D _panCursor;
        [SerializeField] private float _zoomSpeed = 100f;
        
        private CameraPan _cameraPan;
        private Camera _camera;
    }
}
