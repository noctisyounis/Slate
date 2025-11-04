using Inputs.Runtime;
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
        
        public void Disable() => _input.DisableInputsHandling();

        public void UpdatePan()
        {
            HandleKeyboardPan();
            if (m_isMiddleClickHeld) HandleMousePan();
            else _mouseDelta = Vector2.zero;
            HandleCursor();
            HandleZoom();
        }



        public CameraPan(Camera camera, CameraPanSettings settings , Texture2D panCursor = null, float zoomSpeed = 100f)
        {
            _camera = camera;
            _settings = settings;

            _panCursor = panCursor;
            _defaultCursor = null; // Cursor par défaut

            // Initialiser le input system
            _input = ScriptableObject.CreateInstance<InputsHandler>();
            _input.EnableInputsHandling();

            _input.m_move += ctx => m_moveInput = ctx;
            _input.m_zoom += ctx => _zoomDelta = ctx;
            _input.m_pan += ctx => m_isMiddleClickHeld = ctx;
        }
        
        #endregion
        
        
        #region Main Methods
        
        private void HandleMousePan()
        {
            // Pan souris (Middleclick maintenu)
            if (!m_isMiddleClickHeld) // Empêche la souris d'agir
            {
                Vector3 mouseMove = new Vector3(_mouseDelta.x, _mouseDelta.y, 0f) * (_settings.m_mousePanSpeed * Time.deltaTime);
                _camera.transform.Translate(mouseMove, Space.World);
            }
        }

        private void HandleKeyboardPan()
        {
            // Pan clavier
            if (m_isMiddleClickHeld)
            {
                Vector3 move = new Vector3(m_moveInput.x,  m_moveInput.y, 0f) * (_settings.m_panSpeed * Time.deltaTime);
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
            if (Mathf.Abs(_zoomDelta) <= 0.01f) return;

            if (_camera.orthographic)
            {
                // Zoom caméra orthographique : on ajuste la taille
                
                _camera.orthographicSize -= _zoomDelta * (_settings.m_zoomSpeed) *  Time.deltaTime;
                _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize, _settings.m_minOrthoZoom, _settings.m_maxOrthoZoom);
            }

            else
            {
                // Zoom caméra perspective : on ajuste la position Z
                Vector3 pos = _camera.transform.position;
                pos.z += _zoomDelta * _settings.m_zoomSpeed * Time.deltaTime;
                pos.z = Mathf.Clamp(pos.z, _settings.m_minZoom, _settings.m_maxZoom);
                _camera.transform.position = pos;
            }
        }
        
        #endregion
        
        
        #region Private and protected
        
        private Camera _camera;
        private readonly InputsHandler _input;
        private CameraPanSettings _settings;
        
        private Texture2D _panCursor;           // Le sprite du curseur pour le pan
        private Texture2D _defaultCursor;       // Sauvegarde du curseur par défaut
        private Vector2 _cursorHotspot =  Vector2.zero;
        
        private float _zoomDelta;

        #endregion
    }
    
}

