using Bullets;
using UnityEngine;

namespace PlayerWeapons
{
    /// <summary>
    /// Weapon with: simple bullet, fast cooldown
    /// </summary>
    public class PlayerWeaponBulletFastCooldown : PlayerWeaponLaunchBullets
    {
        public override float GetStartCooldownSec() => 0.1f;

        private float GetDurationSecBeforeDestroy() => 3f;

        private int GetBulletViewIndex() => 1;

        protected override void CreateBullets(IForWeaponUse iForWeaponUse, Vector2 playerPos, float angleRad)
        {
            iForWeaponUse.AddBullet(new Bullet(
                    playerPos, angleRad, GetBulletSpeedPerSec(),
                    GetDurationSecBeforeDestroy(), GetBulletCircleColliderRadius(), GetDamage()
                    ),
                GetBulletViewIndex()
                );
        }
    }
}