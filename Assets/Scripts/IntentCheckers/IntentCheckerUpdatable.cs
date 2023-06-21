using System;

namespace IntentCheckers
{
    /// <summary>
    /// Concept of "user intention" that can be checked, with decoupled enabling logic, and decoupled event logic
    /// </summary>
    public abstract class IntentCheckerUpdatable
    {
        private readonly Action _onWant;

        public bool IsEnabled = true;

        protected IntentCheckerUpdatable(Action onWant)
        {
            _onWant = onWant;
        }

        /// <returns>isEventDetected</returns>
        public bool Update()
        {
            if (!IsEnabled) return false;

            if (Check())
            {
                _onWant.Invoke();
                return true;
            }

            return false;
        }

        protected abstract bool Check();
    }
}