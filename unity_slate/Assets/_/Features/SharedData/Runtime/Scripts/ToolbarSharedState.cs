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
            public UnityEvent m_onClick; // laissé null si noeud avec children
        }

        [Header("Runtime State (écrit/lu à chaud)")]
        public bool m_isToolbarDisplayed = false;             // état de la barre (logic -> view)
        public float m_y = 0f;                    // offset vertical animé (logic -> view)
        public bool m_isAnyMenuOpen = false;      // un popup est ouvert (menu -> logic)
        public bool m_isPointerInToolbar = false; // pointeur dans la zone (logic -> autres)
        public float m_menusTotalWidth = 0f;      // largeur cumulée preview des menus (menu -> logic)
        public float m_popupMaxHeight = 280f;     // borne verticale pour zone dynamique

        [Header("Menus dynamiques (éditables)")]
        public float m_menuPreviewWidth = 110f;   // largeur fixe de preview (px)
        public float m_menuItemSpacing = 10f;
        public float m_menuPopupMaxWidth = 360f;
        public MenuNode[] m_menuOne;              // ex. "Fichier"
        public MenuNode[] m_menuTwo;              // ex. "Affichage"

        [Header("Boutons (libellés + events)")]
        public string m_btnMinLabel = "_";
        public string m_btnBorderlessLabel = "□";
        public string m_btnQuitLabel = "X";

        [Header("Requêtes boutons (logic polling)")]
        [NonSerialized] public bool m_requestMinimize = false;
        [NonSerialized] public bool m_requestToggleBorderless = false;
        [NonSerialized] public bool m_requestQuit = false;

        [Header("Debug")]
        public bool m_debugHUD = false;
    }
    
}

