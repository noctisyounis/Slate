
using UnityEngine;

namespace Slate.Runtime
{
    public class CameraPan : MonoBehaviour
    {
        #region public
        
        public float m_panSpeed = 10f;
        public float m_mousePanSpeed = 0.5f;
        
        #endregion
        
        
        #region Api Unity

        private void Awake()
        {
            _cam = Camera.main;
            _input = new SlateInputActions.Runtime.SlateInputActions();
            
            // Lier les Actions
            _input.
        }

        #endregion
        
        
        #region Private and protected

        private Camera _cam;
        private SlateInputActions.Runtime.SlateInputActions _input;
        private Vector2 _moveInput;
        private Vector2 _mouseDelta;
        private bool _isRightClickHeld;
        private Vector3 _lastMousePosition;

        #endregion
    }
    
}

