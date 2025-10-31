using System;
using UnityEngine;

namespace Slate.Runtime
{
    public class CameraPan : MonoBehaviour
    {
        #region public
        
        public Vector2 m_moveInput { get; set; }
        public Vector2 _mouseDelta{ get; set; }
        public bool m_isMiddleClickHeld{ get; set; }
        
        #endregion
        
        
        #region Api Unity

        private void Awake()
        {
            
            _camera = Camera.main;
            _input = new SlateInputActions.Runtime.SlateInputActions();

            // Clavier (WASD / Arrow)
            _input.Camera.Move.performed += ctx => m_moveInput = ctx.ReadValue<Vector2>();
            _input.Camera.Move.canceled += ctx => m_moveInput = Vector2.zero;

            // Souris pour le pan (déplacement)
            _input.Camera.Look.performed += ctx => _mouseDelta = ctx.ReadValue<Vector2>();
            _input.Camera.Look.canceled += ctx => _mouseDelta = Vector2.zero;

            // Clic droit pour activer le pan souris
            _input.Camera.MiddleClick.performed += ctx => m_isMiddleClickHeld = true;
            _input.Camera.MiddleClick.canceled += ctx => m_isMiddleClickHeld = false;
        }

        private void OnEnable()
        {
            _input.Enable();
        }
        private void OnDisable() => _input.Disable();

        private void Update()
        {
            HandleKeyboardPan();
            HandleMousePan();
        }
        
        
        #endregion
        
        
        #region Utils

        public void HandleMousePan()
        {
            // Pan souris (clic droit maintenu)
            if (m_isMiddleClickHeld)
            {
                Vector3 mouseMove = new Vector3(-_mouseDelta.x, -_mouseDelta.y, 0f) * (_mousePanSpeed * Time.deltaTime);
                transform.Translate(mouseMove, Space.World);
            }
        }

        public void HandleKeyboardPan()
        {
            // Pan clavier
            Vector3 move = new Vector3(m_moveInput.x,  m_moveInput.y, 0f) * (_panSpeed * Time.deltaTime);
            transform.Translate(move, Space.World);
        }
        
        #endregion
        
        
        #region Private and protected
        
        [SerializeField] private float _panSpeed = 10f;
        [SerializeField] private float _mousePanSpeed = 0.5f;
        [SerializeField] private Camera _camera;
        
        private SlateInputActions.Runtime.SlateInputActions _input;


        #endregion
    }
    
}

