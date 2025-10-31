using UnityEngine;

namespace Slate.Runtime
{
    
    public class CameraPan
    {
        #region public
        
        public Vector2 m_moveInput { get; set; }
        public Vector2 _mouseDelta{ get; set; }
        public bool m_isMiddleClickHeld{ get; set; }
        
        #endregion
        
        
        #region Utils
        
        public void Disable() => _input.Disable();

        public void UpdatePan()
        {
            HandleKeyboardPan();
            HandleMousePan();
            HandleCursor();
            HandleZoom();
        }



        public CameraPan(Camera camera, float panSpeed = 10f, float mousePanSpeed = 10f, Texture2D panCursor = null, float zoomSpeed = 100f)
        {
            _camera = camera;
            _panSpeed = panSpeed;
            _mousePanSpeed  = mousePanSpeed;

            _panCursor = panCursor;
            _defaultCursor = null; // Cursor par défaut
            
            _zoomSpeed = zoomSpeed;

            // Initialiser le input system
            _input = new SlateInputActions.Runtime.SlateInputActions();
            _input.Enable();
            _input.Camera.Move.performed += ctx => m_moveInput = ctx.ReadValue<Vector2>();
            _input.Camera.Move.canceled += ctx => m_moveInput = Vector2.zero;
            
            _input.Camera.Look.performed += ctx => _mouseDelta = ctx.ReadValue<Vector2>();
            _input.Camera.Look.canceled += ctx => _mouseDelta = Vector2.zero;

            _input.Camera.MiddleClick.performed += ctx => m_isMiddleClickHeld = true;
            _input.Camera.MiddleClick.canceled += ctx => m_isMiddleClickHeld = false;
            
            _input.Camera.Zoom.performed += ctx => _zoomDelta = ctx.ReadValue<float>();
            _input.Camera.Zoom.canceled += ctx => _zoomDelta = 0f;

        }
        
        #endregion
        
        
        #region Main Methods
        
        private void HandleMousePan()
        {
            // Pan souris (clic droit maintenu)
            if (m_isMiddleClickHeld)
            {
                Vector3 mouseMove = new Vector3(-_mouseDelta.x, -_mouseDelta.y, 0f) * (_mousePanSpeed * Time.deltaTime);
                _camera.transform.Translate(mouseMove, Space.World);
            }
        }

        private void HandleKeyboardPan()
        {
            // Pan clavier
            Vector3 move = new Vector3(m_moveInput.x,  m_moveInput.y, 0f) * (_panSpeed * Time.deltaTime);
            _camera.transform.Translate(move, Space.World);
        }
        private void HandleCursor()
        {
            if (m_isMiddleClickHeld)
            {
                // Cursor de pan
                if (_panCursor is not null)
                {
                    Cursor.SetCursor(_panCursor, _cursorHotspot, CursorMode.Auto);
                }

                Cursor.visible = true;
            }

            else
            {
                // Cursor par défaut
                Cursor.SetCursor(_defaultCursor, Vector2.zero, CursorMode.Auto);
                Cursor.visible = true;
            }
        }

        private void HandleZoom()
        {
            if (Mathf.Abs(_zoomDelta) > 0.01f)
            {
                Vector3 pos = _camera.transform.position;
                pos.z += _zoomDelta * _zoomSpeed * Time.deltaTime;
                pos.z = Mathf.Clamp(pos.z, _minZoom, _maxZoom);
                _camera.transform.position = pos;
            }
        }
        
        #endregion
        
        
        #region Private and protected
        
        private float _panSpeed = 10f;
        private float _mousePanSpeed = 10f;
        private Camera _camera;
        private readonly SlateInputActions.Runtime.SlateInputActions _input;
        
        private Texture2D _panCursor;           // Le sprite du curseur pour le pan
        private Texture2D _defaultCursor;       // Sauvegarde du curseur par défaut
        private Vector2 _cursorHotspot =  Vector2.zero;
        
        private float _zoomDelta;
        private float _zoomSpeed = 100f;        // vitesse de zoom
        private float _minZoom = -50f;        // distance minimal de la caméra
        private float _maxZoom = -2f;         // distance maximal de la caméra

        #endregion
    }
    
}

