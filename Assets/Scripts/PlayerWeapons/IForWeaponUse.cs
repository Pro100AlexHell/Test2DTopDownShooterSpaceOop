using Bullets;
using UnityEngine;

namespace PlayerWeapons
{
    public interface IForWeaponUse
    {
        void AddBullet(Bullet bullet, int bulletViewIndex);

        void DoLaserWeaponWithDamageFirstAndVisualEffect(Vector2 playerPos, float angleRad, float maxRange, int damage);
    }
}