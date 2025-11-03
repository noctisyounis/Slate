using Inputs.Runtime;
using UnityEngine;
using UnityEngine.InputSystem.Users;

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
        

        public void UpdatePan()
        {
            HandleKeyboardPan();
            if (m_isMiddleClickHeld) HandleMousePan();
            else _mouseDelta = Vector2.zero;
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
            _input = ScriptableObject.CreateInstance<InputsHandler>();
            _input.EnableInputsHandling();
            
            _input.m_move += ctx => m_moveInput = ctx;
            _input.m_zoom += ctx => _zoomDelta = ctx;
            _input.m_pan += ctx => m_isMiddleClickHeld = ctx;
            // _input.Slate.Move.canceled += ctx => m_moveInput = Vector2.zero;
            //
            // // _input.Slate.Look.performed += ctx => _mouseDelta = ctx.ReadValue<Vector2>();
            // // _input.Slate.Look.canceled += ctx => _mouseDelta = Vector2.zero;
            //
            // _input.Slate.Pan.performed += ctx => m_isMiddleClickHeld = ctx.ReadValue<float>() > 0.5f;
            // _input.Slate.Pan.canceled += ctx => m_isMiddleClickHeld = false;
            //
            // _input.Slate.Zoom.performed += ctx => _zoomDelta = ctx.ReadValue<float>();
            // _input.Slate.Zoom.canceled += ctx => _zoomDelta = 0f;

        }
        
        #endregion
        
        
        #region Main Methods
        
        private void HandleMousePan()
        {
            // Pan souris (Middleclick maintenu)
            if (!m_isMiddleClickHeld) // Empêche la souris d'agir
            {
                Vector3 mouseMove = new Vector3(-_mouseDelta.x, -_mouseDelta.y, 0f) * (_mousePanSpeed * Time.deltaTime);
                _camera.transform.Translate(mouseMove, Space.World);
            }
        }

        private void HandleKeyboardPan()
        {
            // Pan clavier
            if (m_isMiddleClickHeld)
            {
                Vector3 move = new Vector3(m_moveInput.x,  m_moveInput.y, 0f) * (_panSpeed * Time.deltaTime);
                _camera.transform.Translate(move, Space.World);
            }
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
        private readonly InputsHandler _input;
        
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

