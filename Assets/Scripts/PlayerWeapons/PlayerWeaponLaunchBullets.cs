using UnityEngine;

namespace PlayerWeapons
{
    public abstract class PlayerWeaponLaunchBullets : PlayerWeapon
    {
        protected virtual float GetBulletSpeedPerSec() => 250f;

        protected virtual float GetBulletCircleColliderRadius() => 5;

        protected virtual int GetDamage() => 1;

        protected override void DoUse(IForWeaponUse iForWeaponUse, Vector2 playerPos, Vector2 mousePos)
        {
            float angleRad = Mathf.Atan2(mousePos.y - playerPos.y, mousePos.x - playerPos.x);
            CreateBullets(iForWeaponUse, playerPos, angleRad);
        }

        /// <summary>
        /// Actual creation of bullets (one or many)
        /// </summary>
        protected abstract void CreateBullets(IForWeaponUse iForWeaponUse, Vector2 playerPos, float angleRad);
    }
}