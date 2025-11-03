using Foundation.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Inputs.Runtime
{
    /// <summary>
    /// Enables/Disables SO containing all the inputs
    /// </summary>
    public class InputsBinder : FBehaviour
    {

        private void Awake()
        {
            _inputsHandler.EnableInputsHandling();
        }

        private void OnDestroy()
        {
            _inputsHandler.DisableInputsHandling();
        }

        #region Private Variables
        [SerializeField] private InputsHandler _inputsHandler;
        #endregion
    }
}
