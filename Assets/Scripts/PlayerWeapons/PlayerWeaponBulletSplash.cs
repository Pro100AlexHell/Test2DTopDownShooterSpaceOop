using Bullets;
using UnityEngine;

namespace PlayerWeapons
{
    /// <summary>
    /// Weapon with: splash-damage bullet, slow cooldown
    /// </summary>
    public class PlayerWeaponBulletSplash : PlayerWeaponLaunchBullets
    {
        public override float GetStartCooldownSec() => 0.5f;

        private float GetDurationSecBeforeDestroy() => 6f;

        private int GetBulletViewIndex() => 2;

        protected override float GetBulletCircleColliderRadius() => 10;

        private float GetSplashDamageRange() => 100;

        protected override void CreateBullets(IForWeaponUse iForWeaponUse, Vector2 playerPos, float angleRad)
        {
            iForWeaponUse.AddBullet(new BulletWithSplashDamage(
                    playerPos, angleRad, GetBulletSpeedPerSec(),
                    GetDurationSecBeforeDestroy(), GetBulletCircleColliderRadius(), GetDamage(),
                    GetSplashDamageRange()
                    ),
                GetBulletViewIndex()
                );
        }
    }
}