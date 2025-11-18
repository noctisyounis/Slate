using Inputs.Runtime;
using Manager.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Slate.Runtime
{
    
    public class CameraPan
    {
        #region public
        
        public Vector2 m_moveInput { get; set; }
        public bool m_isMiddleClickHeld{ get; set; }

        public static Vector3 m_MousePanDelta => _mousePanDelta;
        public static Vector3 m_KeyboardPanDelta => _keyboardPanDelta;

        public float WorldToScreenDeltaMultiplier
        {
            get => _worldToScreenDeltaMultiplier;
            set => _worldToScreenDeltaMultiplier = value;
        }

        public float m_midiInput => _midiInput;

        #endregion
        
        
        #region Utils
        
        public void Disable()
        {
            _input.m_move -= ctx => m_moveInput = ctx;
            _input.m_zoom -= ctx => _zoomDelta = ctx;
            _input.m_pan -= ctx => m_isMiddleClickHeld = ctx;
            _input.m_MIDI -= ctx => _midiInput = ctx;

            _input.DisableInputsHandling();
        }

        public void CustomAwake()
        {
            RefreshCurrentZoom();
        }

        public void UpdatePan()
        {
            HandleKeyboardPan();
            HandleMousePanTest();
            HandleZoom();
            
            Vector3 worldDelta = _mousePanDelta + _keyboardPanDelta * 1.669291f;
            // transform world delta to screen delta
            Vector2 screenDelta = WorldDeltaToScreenDelta(worldDelta);
            if (screenDelta != Vector2.zero)
                WindowPosManager.MoveWindows(screenDelta);
            
            _keyboardPanDelta = Vector3.zero;
            _mousePanDelta = Vector3.zero;

//            Debug.Log("Midi input: " + _midiInput);
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
            _input.m_MIDI += ctx => _midiInput = ctx + _midiStartValue;
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
            
            // Vector3 move = new Vector3(m_moveInput.x,  m_moveInput.y, 0f) * (finalSpeed * Time.deltaTime);
            _keyboardPanDelta = new Vector3(m_moveInput.x,  m_moveInput.y, 0f) * (finalSpeed * Time.deltaTime);
            _camera.transform.Translate(_keyboardPanDelta * zoomFactor, Space.World);
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
                
                Vector3 currentWorld = _camera.ScreenToWorldPoint(mouseScreenPos);
                //Vector3 delta = _dragOriginWorld - currentworld;
                _mousePanDelta = _dragOriginWorld - currentWorld;
                _camera.transform.Translate(_mousePanDelta, Space.World);
                
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
                _camera.orthographicSize -= _zoomDelta * (_settings.m_zoomSpeed)* _camera.orthographicSize *  Time.deltaTime;
                _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize, _settings.m_minOrthoZoom, _settings.m_maxOrthoZoom);

                RefreshCurrentZoom();
            }
            else
            {
                Vector3 pos = _camera.transform.position;
                pos.z += _zoomDelta * _settings.m_zoomSpeed * Time.deltaTime;
                pos.z = Mathf.Clamp(pos.z, _settings.m_minZoom, _settings.m_maxZoom);
                _camera.transform.position = pos;
            }
        }

        private void RefreshCurrentZoom()
        {
            _zoomNormalized = Remap(_camera.orthographicSize, 8, 2, 0, 1);
            _settings.m_zoomNormalized = _zoomNormalized;

            Shader.SetGlobalFloat("ZoomNormalised", _zoomNormalized); // update shader global value
        }

        #endregion

        #region Utils

        private Vector2 WorldDeltaToScreenDelta(Vector3 worldDelta)
        {
            if (worldDelta == Vector3.zero || _camera is null) return Vector2.zero;

            Vector3 worldRef = _camera.transform.position + _camera.transform.forward * 10f;
            Vector3 worldRefWithDelta = worldRef + worldDelta;
            Vector3 screenRef = _camera.WorldToScreenPoint(worldRef);
            Vector3 screenRefWithDelta = _camera.WorldToScreenPoint(worldRefWithDelta);
            
            // screen coordinates: (x, y) where y origin is bottom-left in WorldToScreenPoint on Unity.
            // ImGui expects origin (0,0) at top-left, while Unity's Screen coordinates have (0,0) bottom-left.
            // Convert Unity screen Y to top-left origin by flipping Y.
            float screenHeight = Screen.height;
            Vector2 screenRefV2 = new Vector2(screenRef.x, screenHeight - screenRef.y);
            Vector2 screenRefWithDeltaV2 = new Vector2(screenRefWithDelta.x, screenHeight - screenRefWithDelta.y);
            // distance between screen coordinates becomes delta
            Vector2 delta = -(screenRefWithDeltaV2 - screenRefV2);
            return delta * _worldToScreenDeltaMultiplier;
        }

        private float Remap(float value, float from1, float to1, float from2, float to2)
            => (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        #endregion


            #region Private and protected

        private Camera _camera;
        private readonly InputsHandler _input;
        private CameraPanSettings _settings;
        
        private Texture2D _panCursor;           // Le sprite du curseur pour le pan
        private Texture2D _defaultCursor;       // Sauvegarde du curseur par défaut
        private Vector2 _cursorHotspot =  Vector2.zero;
        
        private float _zoomDelta;
        private float _zoomNormalized;
        
        // test mousepan
        private bool _isDragging;
        private Vector3 _dragOriginWorld;
        private static Vector3 _mousePanDelta;
        private static Vector3 _keyboardPanDelta;
        private float _worldToScreenDeltaMultiplier = 3f;

        private float _midiInput;
        private float _midiStartValue = 1f;

        #endregion
    }
    
}

