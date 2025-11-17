using UnityEngine;

using UImGui;
using Foundation.Runtime;
using Inputs.Runtime;

namespace Slate.Runtime
{
    public class SampleWindow : FBehaviour
    {
        private void OnEnable() => UImGuiUtility.Layout += OnLayout;
        private void OnDisable() => UImGuiUtility.Layout -= OnLayout;

        private void OnLayout(UImGui.UImGui uImGui)
        {

        }

        private void Awake()
        {
            _camera =  Camera.main;
            _cameraPan = new CameraPan(_camera,_panSettings, _panCursor);

            if (_panCursor is null)
                Error("Pan Cursor texture reference is missing", this);

            RegisterEvents(true);

            SetFact<float>("midi/01", _cameraPan.m_midiInput, false);
            _camera.orthographicSize = _panSettings.m_startZoom;
            _cameraPan.CustomAwake();
        }

        private void Update()
        {
            _cameraPan.UpdatePan();
        }

        private void OnDestroy()
        {
            RegisterEvents(false);
        }

        private void RegisterEvents(bool v)
        {
            if (v)
                _inputsHandler.m_pan += OnPanInputChange;
            else
                _inputsHandler.m_pan -= OnPanInputChange;
        }

        private void OnPanInputChange(bool isPanning)
        {
            Cursor.SetCursor(isPanning ?  _panCursor : null, Vector2.zero, CursorMode.Auto);
            // Cursor.visible = isPanning;
            Cursor.lockState = CursorLockMode.None;
        }
        
        [SerializeField] private Texture2D _panCursor;
        [SerializeField] private CameraPanSettings _panSettings;
        [SerializeField] private InputsHandler _inputsHandler;
        
        private CameraPan _cameraPan;
        private Camera _camera;
    }
}
