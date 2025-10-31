using UImGui;
using UnityEngine;

namespace Slate.Runtime
{
    public class SampleWindow : MonoBehaviour
    {
        private void Awake()
        {
            _cameraPan = new CameraPan(_camera, _panSpeed, _mousePanSpeed, _panCursor, _zoomSpeed);
        }

        private void Update()
        {
            // Mise à jour de la caméra via le controller
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

        private void OnDestroy()
        {
            // Désactiver le input system
            _cameraPan.Disable();
        }

        private void OnEnable() => UImGuiUtility.Layout += OnLayout;
        private void OnDisable() => UImGuiUtility.Layout -= OnLayout;
        
        private void OnLayout(UImGui.UImGui uImGui)
        {
           
            // ImGui.ShowDemoWindow();
        }
        [Header("Camera Pan Settings")]
        [SerializeField] private float _panSpeed = 10f;
        [SerializeField] private float _mousePanSpeed = 10f;
        [SerializeField] private Camera _camera;
        [SerializeField] private Texture2D _panCursor;
        [SerializeField] private float _zoomSpeed = 100f;
        private CameraPan _cameraPan;


    }
}
