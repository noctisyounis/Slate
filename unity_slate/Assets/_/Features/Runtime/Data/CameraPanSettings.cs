using UnityEngine;

[CreateAssetMenu(fileName = "CameraPanSettings", menuName = "Slate/Camera/Camera Pan Settings")]
public class CameraPanSettings : ScriptableObject
{
    [Header("Pan Settings")] 
    [Tooltip("Vitesse de déplacement au clavier (WASD ou flèches")]
    public float m_panSpeed = 10f;
    
    [Tooltip("Movement speed with mouse")]
    public float m_mousePanSpeed = 10f;
    
    [Header("Zoom settings")]
    [Tooltip("Vitesse de zoom")]
    public float m_zoomSpeed = 100f;
    
    [Header("Zoom limits")]
    [Tooltip("Minimum limit for zoom on orthographic cam")] public float m_minOrthoZoom = 2f;
    [Tooltip("Maximum limit for zoom on orthographic cam")] public float m_maxOrthoZoom = 50f;
    [Space(5)]
    [Tooltip("Minimum limit for zoom on perspective cam")] public float m_minZoom = -50f;
    [Tooltip("Maximum limit for zoom on perspective cam")] public float m_maxZoom = -2f;
}
