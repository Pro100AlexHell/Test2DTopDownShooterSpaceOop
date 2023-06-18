using UnityEngine;

namespace PlayerWeapons
{
    public abstract class PlayerWeapon
    {
        public float CooldownRemainedSec { get; protected set; }

        public abstract float GetStartCooldownSec();

        public virtual void Init()
        {
            CooldownRemainedSec = 0;
        }

        public void TryUpdateCooldown(float deltaTimeSec)
        {
            if (CooldownRemainedSec > 0)
            {
                CooldownRemainedSec -= deltaTimeSec;
                if (CooldownRemainedSec <= 0)
                {
                    CooldownRemainedSec = 0;
                }
            }
        }

        public virtual bool CanUse()
        {
            return (CooldownRemainedSec <= 0);
        }

        public void TryUse(IForWeaponUse iForWeaponUse, Vector2 playerPos, Vector2 mousePos)
        {
            if (CanUse())
            {
                CooldownRemainedSec = GetStartCooldownSec();
                DoUse(iForWeaponUse, playerPos, mousePos);
            }
        }

        protected abstract void DoUse(IForWeaponUse iForWeaponUse, Vector2 playerPos, Vector2 mousePos);
    }
}