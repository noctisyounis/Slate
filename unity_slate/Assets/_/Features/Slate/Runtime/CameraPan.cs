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
        }

        public CameraPan(Camera camera, float panSpeed = 10f, float mousePanSpeed = 0.5f)
        {
            _camera = camera;
            _panSpeed = panSpeed;
            _mousePanSpeed  = mousePanSpeed;

            // Initialiser le input system
            _input = new SlateInputActions.Runtime.SlateInputActions();
            _input.Enable();
            _input.Camera.Move.performed += ctx => m_moveInput = ctx.ReadValue<Vector2>();
            _input.Camera.Move.canceled += ctx => m_moveInput = Vector2.zero;
            
            _input.Camera.Look.performed += ctx => _mouseDelta = ctx.ReadValue<Vector2>();
            _input.Camera.Look.canceled += ctx => _mouseDelta = Vector2.zero;

            _input.Camera.MiddleClick.performed += ctx => m_isMiddleClickHeld = true;
            _input.Camera.MiddleClick.canceled += ctx => m_isMiddleClickHeld = false;

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
        
        #endregion
        
        
        #region Private and protected
        
        private readonly float _panSpeed = 10f;
        private readonly float _mousePanSpeed = 0.5f;
        private readonly Camera _camera;
        private readonly SlateInputActions.Runtime.SlateInputActions _input;

        #endregion
    }
    
}

