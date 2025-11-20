using System;
using UnityEngine;
using UnityEngine.Events;

namespace SharedData.Runtime
{
    
    [CreateAssetMenu(fileName = "ToolbarSharedState", menuName = "ScriptableObjects/ToolbarSharedState")]
    public class ToolbarSharedState : ScriptableObject
    {
        [Serializable]
        public class MenuNode
        {
            public string m_label = "Item";
            public bool m_separator = false;
            public string m_commandId;
            public UnityEvent m_onClick;
        }

        [Header("Runtime State")]
        public bool m_isToolbarDisplayed = false;
        public float m_y = 0f;
        public bool m_isAnyMenuOpen = false;
        public bool m_isPointerInToolbar = false;
        public float m_menusTotalWidth = 0f;
        public float m_popupMaxHeight = 280f;

        [Header("Menus")]
        public float m_menuPreviewWidth = 110f;
        public float m_menuItemSpacing = 10f;
        public float m_menuPopupMaxWidth = 360f;
        
        [Header("Menu Commands")]
        [NonSerialized] public bool m_menuCommandPending = false;
        [NonSerialized] public string m_menuCommandId = null;

        [Header("Buttons")]
        public string m_btnMinLabel = "_";
        public string m_btnBorderlessLabel = "□";
        public string m_btnQuitLabel = "X";

        [Header("Requests")]
        [NonSerialized] public bool m_requestMinimize = false;
        [NonSerialized] public bool m_requestToggleBorderless = false;
        [NonSerialized] public bool m_requestQuit = false;
    }
    
}

