using UnityEngine;

namespace PlayerWeapons
{
    public class PlayerWeaponLaser : PlayerWeapon
    {
        public override float GetStartCooldownSec() => 0.5f;

        protected float GetMaxRange() => 400;

        protected int GetDamage() => 1;

        protected override void DoUse(IForWeaponUse iForWeaponUse, Vector2 playerPos, Vector2 mousePos)
        {
            float angleRad = Mathf.Atan2(mousePos.y - playerPos.y, mousePos.x - playerPos.x);
            iForWeaponUse.DoLaserWeaponWithDamageFirstAndVisualEffect(playerPos, angleRad, GetMaxRange(), GetDamage());
        }
    }
}