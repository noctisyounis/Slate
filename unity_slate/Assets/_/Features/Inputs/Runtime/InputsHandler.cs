using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

using static CustomInputActions;

namespace Inputs.Runtime
{
    [CreateAssetMenu(fileName = "InputsHandlerSO", menuName = "ScriptableObjects/Inputs/InputsHandler")]
    public class InputsHandler : ScriptableObject, ISlateActions
    {
        /// <summary>
        /// Normalised value for the movement
        /// </summary>
        public event UnityAction<Vector2> m_move = delegate { };
        /// <summary>
        /// Raw value (not normalised) for the movement
        /// </summary>
        public event UnityAction<Vector2> m_moveRaw = delegate { };
        public event UnityAction<float> m_zoom = delegate { };

        public event UnityAction<bool> m_pan = delegate { };
        public event UnityAction<bool> m_select = delegate { };
        public event UnityAction<bool> m_options = delegate { };
        
        // public event UnityAction<bool> m_hide = delegate { };
        // public event UnityAction<bool> m_border = delegate { };

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

        public void OnMove(InputAction.CallbackContext context)
        {
            Vector2 rawMove = context.ReadValue<Vector2>();
            m_moveRaw.Invoke(rawMove);
            m_move.Invoke(rawMove.normalized);
        }

        public void OnZoom(InputAction.CallbackContext context)
            => OnFloatUpdate(context, m_zoom);

        public void OnPan(InputAction.CallbackContext context)
            => OnBoolUpdate(context, m_pan);

        public void OnSelect(InputAction.CallbackContext context)
            => OnBoolUpdate(context, m_select);

        public void OnOptions(InputAction.CallbackContext context)
            => OnBoolUpdate(context, m_options);
        #endregion

        #region Private Methods

        private void OnVector2Update(InputAction.CallbackContext context, UnityAction<Vector2> customEvent, bool normalised = true)
            => customEvent.Invoke(normalised ? context.ReadValue<Vector2>().normalized : context.ReadValue<Vector2>());

        private void OnFloatUpdate(InputAction.CallbackContext context, UnityAction<float> customEvent)
            => customEvent.Invoke(context.ReadValue<float>());

        private void OnBoolUpdate(InputAction.CallbackContext context, UnityAction<bool> customEvent)
            => customEvent.Invoke(context.ReadValue<float>() > 0f);
        #endregion

        #endregion

    }
}
