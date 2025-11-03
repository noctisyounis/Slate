using UnityEngine;

using Foundation.Runtime;
using Inputs.Runtime;

public class TestInputs : FBehaviour
{
    [SerializeField] private InputsHandler _inputsHandler;


    #region Monobehaviour Methods
    private void Awake()
    {
        // Z : Ideally would have a method to register events that prevents manual errors
        _inputsHandler.m_move += OnMoveInput;
        _inputsHandler.m_zoom += OnZoomInput;

        _inputsHandler.m_pan += OnPanInput;
        _inputsHandler.m_select += OnSelectInput;
        _inputsHandler.m_options += OnOptionsInput;
    }

    private void OnDestroy()
    {
        // Z : Ideally would have a method to register events that prevents manual errors
        _inputsHandler.m_move -= OnMoveInput;
        _inputsHandler.m_zoom -= OnZoomInput;

        _inputsHandler.m_pan -= OnPanInput;
        _inputsHandler.m_select -= OnSelectInput;
        _inputsHandler.m_options -= OnOptionsInput;
    }
    #endregion

    #region Custom Methods
    private void OnMoveInput(Vector2 moveInput) => _moveHeld = moveInput;
    private void OnZoomInput(float zoomInput) => _zoomHeld = zoomInput;

    private void OnPanInput(bool panInput) => _panHeld = panInput;
    private void OnSelectInput(bool selectInput) => _selectHeld = selectInput;
    private void OnOptionsInput(bool optionsInput) => _optionsHeld = optionsInput;
    #endregion

    #region Private Variables
    [Space(15)]
    [SerializeField] private Vector2 _moveHeld = Vector2.zero;
    [SerializeField] private float _zoomHeld = 0.0f;
    [SerializeField] private bool _panHeld = false;
    [SerializeField] private bool _selectHeld = false;
    [SerializeField] private bool _optionsHeld = false;
    #endregion
}
