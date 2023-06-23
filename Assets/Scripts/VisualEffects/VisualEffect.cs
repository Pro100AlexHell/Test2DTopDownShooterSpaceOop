namespace VisualEffects
{
    public abstract class VisualEffect
    {
        private float _expireRemainedSec;

        protected VisualEffect(float durationSec)
        {
            _expireRemainedSec = durationSec;
        }

        /// <returns>needDestroy</returns>
        public bool UpdateExpire(float deltaTimeSec)
        {
            _expireRemainedSec -= deltaTimeSec;
            return (_expireRemainedSec <= 0);
        }

        public abstract void OnDestroy();
    }
}