using System;
using UnityEngine;

namespace IntentCheckers
{
    public class IntentCheckerThroughKeyCode : IntentCheckerUpdatable
    {
        private readonly KeyCode _keyCode;

        public IntentCheckerThroughKeyCode(Action onWant, KeyCode keyCode)
            : base(onWant)
        {
            _keyCode = keyCode;
        }

        protected override bool Check()
        {
            return Input.GetKeyDown(_keyCode);
        }
    }
}