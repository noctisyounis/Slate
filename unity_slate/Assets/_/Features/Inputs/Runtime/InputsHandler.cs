using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

using static CustomInputActions;

namespace Inputs.Runtime
{
    [CreateAssetMenu(fileName = "Inputs_SO", menuName = "ScriptableObjects/InputsSO")]
    public class InputsHandler : ScriptableObject, ISlateActions
    {
        public event UnityAction<Vector2> m_move = delegate { };
        public event UnityAction<float> m_zoom = delegate { };

        public event UnityAction<bool> m_pan = delegate { };
        public event UnityAction<bool> m_select = delegate { };
        public event UnityAction<bool> m_options = delegate { };

        public CustomInputActions m_inputActions;

        /* Z : Direct values if necessary (prefer to not expose them at first)
        public Vector2 m_direction => m_inputActions.Slate.Move.ReadValue<Vector2>().normalized;
        public Vector2 m_zoomValue => m_inputActions.Slate.Zoom.ReadValue<Vector2>();

        public bool m_isPressingPan => m_inputActions.Slate.Pan.ReadValue<float>() > 0f;
        public bool m_isPressingSelect => m_inputActions.Slate.Select.ReadValue<float>() > 0f;
        public bool m_isPressingOptions => m_inputActions.Slate.Options.ReadValue<float>() > 0f;
        */


        #region Slate Actions

        #region Public methods
        public void EnableInputsHandling()
        {
            if (m_inputActions == null)
            {
                m_inputActions = new CustomInputActions();
                m_inputActions.Slate.SetCallbacks(this);
            }
            m_inputActions?.Enable();
        }

        public void DisableInputsHandling()
        {
            m_inputActions?.Disable();
        }

        public void OnMove(InputAction.CallbackContext context) => OnVector2Update(context, m_move);
        public void OnZoom(InputAction.CallbackContext context) => m_zoom.Invoke(context.ReadValue<float>());

        public void OnPan(InputAction.CallbackContext context)
        {
            bool isPressed = context.ReadValue<float>() > 0f;
            m_pan.Invoke(isPressed);
        }

        public void OnSelect(InputAction.CallbackContext context)
        {
            bool isPressed = context.ReadValue<float>() > 0f;
            m_select.Invoke(isPressed);
        }

        public void OnOptions(InputAction.CallbackContext context)
        {
            bool isPressed = context.ReadValue<float>() > 0f;
            m_options.Invoke(isPressed);
        }
        #endregion

        #region Private Methods

        // Z : Ideally create a float & bool version of this method but couldn't get it working for some reason
        private void OnVector2Update(InputAction.CallbackContext context, UnityAction<Vector2> customEvent) => customEvent.Invoke(context.ReadValue<Vector2>());
        #endregion

        #endregion

    }
}
