using Bullets;

namespace PlayerWeapons
{
    public interface IForWeaponUse
    {
        void AddBullet(Bullet bullet, int bulletViewIndex);

        // todo Laser (create VisualEffect + Raycast with Damage first enemy\asteroid on ray)
    }
}