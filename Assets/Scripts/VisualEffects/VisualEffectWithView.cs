using UnityEngine;

namespace VisualEffects
{
    public class VisualEffectWithView : VisualEffect
    {
        private readonly GameObject _gameObjectView;

        public VisualEffectWithView(GameObject gameObjectView, float durationSec)
            : base(durationSec)
        {
            _gameObjectView = gameObjectView;
        }

        public override void OnDestroy()
        {
            Object.Destroy(_gameObjectView);
        }
    }
}