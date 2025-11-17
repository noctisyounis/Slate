using UnityEngine;
using UImGui;

namespace Style.Runtime
{
    [DisallowMultipleComponent]
    public class GlobalFontApplier : MonoBehaviour
    {
        #region Unity

            public void OnEnable()  => UImGuiUtility.Layout += OnLayout;
            public void OnDisable() => UImGuiUtility.Layout -= OnLayout;
            
        #endregion

        #region Layout

            private void OnLayout(UImGui.UImGui ui)
            {
                FontRegistry.ApplyAsDefault();
            }

        #endregion
    }
}