using UnityEngine;

[CreateAssetMenu(fileName = "CameraPanSettings", menuName = "Slate/Camera/Camera Pan Settings")]
public class CameraPanSettings : ScriptableObject
{
    [Header("Pan Settings")] 
    [Tooltip("Vitesse de déplacement au clavier (WASD ou flèches")]
    public float m_panSpeed = 10f;
    
    [Tooltip("Vitesse de déplcament avec la souris (clic milieu")]
    public float m_mousePanSpeed = 10f;
    
    [Header("Zoom Settings")]
    [Tooltip("Vitesse de zoom")]
    public float m_zoomSpeed = 100f;
    
    [Header("Limite de Zoom (Perspective)")]
    public float m_minZoom = -50f;
    public float m_maxZoom = -2f;
    
    [Header("Limite de Zoom (Orthographique)")]
    public float m_minOrthoZoom = 2f;
    public float m_maxOrthoZoom = 50f;
}
