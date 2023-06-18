using UnityEngine;

namespace Bullets
{
    /// <summary>
    /// Bullet with splash damage in radius (when bullet is destroyed)
    /// </summary>
    public class BulletWithSplashDamage : Bullet
    {
        private readonly float _splashDamageRange;

        public BulletWithSplashDamage(Vector2 pos, float angleRad, float speedPerSec,
            float durationSecBeforeDestroy, float circleColliderRadius, int damage, float splashDamageRange)
            : base(pos, angleRad, speedPerSec, durationSecBeforeDestroy, circleColliderRadius, damage)
        {
            _splashDamageRange = splashDamageRange;
        }

        public override void OnReachTargetDestroy(IForBulletOnReachTargetDestroy iForBulletOnReachTargetDestroy)
        {
            iForBulletOnReachTargetDestroy.SplashDamageEnemiesOrAsteroids(Pos, _splashDamageRange, Damage);
        }
    }
}