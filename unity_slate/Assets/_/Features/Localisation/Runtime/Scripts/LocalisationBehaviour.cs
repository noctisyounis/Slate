using System;
using Slate.Runtime;
using UnityEngine;

namespace Localisation.Runtime
{
    public class LocalisationBehaviour : WindowBaseBehaviour
    {

        protected override void WindowLayout()
        {
            _localisation.DrawUI();
            _localisation.DrawDebug();
        }
        
        #region Privates
    
            private Localisation _localisation = new Localisation();
            
        #endregion
    }
}
