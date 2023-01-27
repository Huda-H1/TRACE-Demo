//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//namespace HutongGames.PlayMaker.Actions
//{
//    [ActionCategory("Oculus")]
//    [Tooltip("Gets Bool if Active.")]
//    public class GetIsControllerActive : FsmStateAction
//    {

//        [Tooltip("Select the controller Status")]
//        public setController controller;

//        public enum setController
//        {
//            leftController,
//            rightController,
//        };


//        [Tooltip("Set to True if controller is active.")]
//        [UIHint(UIHint.Variable)]
//        public FsmBool storeResult;

//        private OVRInput.Controller controllerInput;

//        public override void Reset()
//        {
//            storeResult = null;
//        }

//        public override void OnEnter()
//        {
//            controllerInput = OVRInput.Controller.RTrackedRemote;
//        }

//        public override void OnUpdate()
//        {
            
//            switch (controller)
//            {
//                case setController.leftController:
//                    if(OVRInput.IsControllerConnected(OVRInput.Controller.LTrackedRemote))
//                        storeResult.Value = true;
//                    else storeResult.Value = false;
//                    break;
//                case setController.rightController:
//                    if (OVRInput.IsControllerConnected(OVRInput.Controller.RTrackedRemote))
//                        storeResult.Value = true;
//                    else storeResult.Value = false;
//                    break;
//            }
//        }
//    }
//}
