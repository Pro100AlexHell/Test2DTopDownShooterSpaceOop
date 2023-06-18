using UnityEngine;

namespace Bullets
{
    public interface IForBulletOnReachTargetDestroy
    {
        void SplashDamageEnemiesOrAsteroids(Vector2 pos, float range, int damage);
    }
}