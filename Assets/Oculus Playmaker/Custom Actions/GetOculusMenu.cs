using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("Oculus")]
    [Tooltip("Detects if the Oculus has gone into the menu ")]
    public class GetOculusMenu : FsmStateAction
    {
        public FsmBool vrFocus;

        public override void OnUpdate()
        {
            if (OVRManager.hasInputFocus && OVRManager.hasVrFocus)
            { 
                    vrFocus.Value = true;
                }
                else
                {
                    vrFocus.Value = false;
                }
         }
     }
}
