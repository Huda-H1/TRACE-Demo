using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("Oculus")]
    [Tooltip("Gets the Local Position of the Controller.")]
    public class GetTouchControllerPosition : FsmStateAction
    {
        [Tooltip("Specify Controller Type.")]
        public controllerEnum touchController;

        [UIHint(UIHint.Variable)]
        [Tooltip("Vector3 variable of the Controller Local Position.")]
        public FsmVector3 controllerPosition;

        private OVRInput.Controller controllerInput;

        public enum controllerEnum
        {
            LeftController,
            RightController,
        }

        public bool everyFrame;

        public override void OnEnter()
        {
            switch (touchController)
            {
                case controllerEnum.LeftController:
                    controllerInput = OVRInput.Controller.LTouch;
                    break;
                case controllerEnum.RightController:
                    controllerInput = OVRInput.Controller.RTouch;
                    break;
            }

            if (!everyFrame)
            {
                Finish();
            }

        }

        public override void Reset()
        {
            controllerPosition = null;
        }

        public override void OnUpdate()
        {
            DoGetPosition();
        }
        void DoGetPosition()
        {
            controllerPosition.Value = OVRInput.GetLocalControllerPosition(controllerInput);
        }
    }
}
