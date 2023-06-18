using GridForColliders;
using UnityEngine;

namespace Bullets
{
    /// <summary>
    /// Basic bullet
    /// </summary>
    public class Bullet : ObjectInGridWithColliderCircle<ObjectTypeInGrid>
    {
        // todo better ?? to use 'default interface' \ 'TRAIT' C# 8.0
        public float DurationSecBeforeDestroy { get; private set; }

        /// <summary>
        /// Damage (to enemy\asteroid) on collide
        /// </summary>
        public readonly int Damage;

        public Bullet(Vector2 pos, float angleRad, float speedPerSec,
            float durationSecBeforeDestroy, float circleColliderRadius, int damage)
            : base(pos, ObjectTypeInGrid.Bullet,
                circleColliderRadius, CircleApproximationPrecision.Point4 // (4 due to small object size)
            )
        {
            DurationSecBeforeDestroy = durationSecBeforeDestroy;
            Damage = damage;

            // (does not change direction, so assign once)
            DeltaPosPerSec = new Vector2(
                Mathf.Cos(angleRad) * speedPerSec,
                Mathf.Sin(angleRad) * speedPerSec
            );
        }

        /// <returns>needDestroyBulletDueToDuration</returns>
        public bool UpdateDurationBeforeDestroy(float deltaTimeSec)
        {
            DurationSecBeforeDestroy -= deltaTimeSec;
            return (DurationSecBeforeDestroy <= 0);
        }

        public virtual void OnReachTargetDestroy(IForBulletOnReachTargetDestroy iForBulletOnReachTargetDestroy)
        {
        }
    }
}