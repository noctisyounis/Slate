using Inputs.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Slate.Runtime
{
    
    public class CameraPan
    {
        #region public
        
        public Vector2 m_moveInput { get; set; }
        public bool m_isMiddleClickHeld{ get; set; }
        
        #endregion
        
        
        #region Utils
        
        public void Disable()
        {
            _input.m_move -= ctx => m_moveInput = ctx;
            _input.m_zoom -= ctx => _zoomDelta = ctx;
            _input.m_pan -= ctx => m_isMiddleClickHeld = ctx;

            _input.DisableInputsHandling();
        }

        public void UpdatePan()
        {
            HandleKeyboardPan();
            // HandleMousePan();   // ancienne version
            HandleMousePanTest();  // nouvelle version
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

        private void HandleKeyboardPan()
        {
            // Pan clavier
            if (m_isMiddleClickHeld || Mouse.current.delta.magnitude > 0.0f)
                return;

            float zoomFactor = _camera.orthographic ? _camera.orthographicSize : _camera.fieldOfView * _settings.m_correcZoom;
            
            float currentZoom = _camera.orthographicSize;
            float interpolant = Mathf.Clamp01(Mathf.InverseLerp(_settings.m_maxOrthoZoom, _settings.m_minOrthoZoom, currentZoom)); 
            float finalSpeed = Mathf.Lerp(-_settings.m_panSpeedmax, -_settings.m_panSpeedmin, interpolant);
            
            Vector3 move = new Vector3(m_moveInput.x,  m_moveInput.y, 0f) * (finalSpeed * Time.deltaTime);
            _camera.transform.Translate(move * zoomFactor, Space.World);
        }
        
        /// <summary>
        /// Old version MousePan
        /// </summary>
        private void HandleMousePan()
        {
            if (!m_isMiddleClickHeld)
                return;

            // Pan souris (Middleclick maintenu)
            // float zoomFactor = _camera.orthographic ? _camera.orthographicSize : _camera.fieldOfView * _settings.m_correcZoom;

            float currentZoom = _camera.orthographicSize;
            float interpolant = Mathf.Clamp01(Mathf.InverseLerp(_settings.m_maxOrthoZoom, _settings.m_minOrthoZoom, currentZoom)); 
            float finalSpeed = Mathf.Lerp(-_settings.m_mousePanSpeedmax, -_settings.m_mousePanSpeedmin, interpolant);
            
            Vector3 mouseMove = new Vector3(m_moveInput.x, m_moveInput.y, 0f) * (finalSpeed * Time.deltaTime);
            //Debug.Log($"{mouseMove} / {finalSpeed}");
            _camera.transform.Translate(mouseMove, Space.World);
        }

        private void HandleMousePanTest()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;
            
            if (m_isMiddleClickHeld && !_isDragging)
            {
                _isDragging = true;
                
                Vector3 mouseScreenPos = mouse.position.ReadValue();
                mouseScreenPos.z = Mathf.Abs(_camera.transform.position.z);
                _dragOriginWorld = _camera.ScreenToWorldPoint(mouseScreenPos);
            }
            
            if (_isDragging && m_isMiddleClickHeld)
            {
                Vector3 mouseScreenPos = mouse.position.ReadValue();
                mouseScreenPos.z = Mathf.Abs(_camera.transform.position.z);
                
                Vector3 currentworld = _camera.ScreenToWorldPoint(mouseScreenPos);
                Vector3 delta = _dragOriginWorld - currentworld;
                
                _camera.transform.Translate(delta, Space.World);
                
                _dragOriginWorld = _camera.ScreenToWorldPoint(mouseScreenPos);
            }
            
            if (!m_isMiddleClickHeld && _isDragging)
            {
                _isDragging = false;
            }
            
        }
        
        private void HandleZoom()
        {
            if (Mathf.Abs(_zoomDelta) <= 0.01f) return;

            if (_camera.orthographic)
            {
                // Zoom caméra orthographique : on ajuste la taille
                
                _camera.orthographicSize -= _zoomDelta * (_settings.m_zoomSpeed)* _camera.orthographicSize *  Time.deltaTime;
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
        
        // test mousepan
        private bool _isDragging;
        private Vector3 _dragOriginWorld;

        #endregion
    }
    
}

