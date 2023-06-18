using Bullets;
using UnityEngine;

namespace Presenters
{
    public class BulletPresenter : BasePresenter<Bullet>
    {
        public BulletPresenter(Bullet model, GameObject view)
            : base(model, view)
        {
        }

        public bool AdditionalCheckDestroyBullet(float deltaTimeSec, Rect mapRect)
        {
            bool needDestroyBulletDueToOutOfBounds = !mapRect.Contains(Model.Pos);
            if (needDestroyBulletDueToOutOfBounds)
            {
                Debug.LogWarning("needDestroyBulletDueToOutOfBounds");
                return true;
            }

            bool needDestroyBulletDueToDuration = Model.UpdateDurationBeforeDestroy(deltaTimeSec);
            if (needDestroyBulletDueToDuration)
            {
                Debug.LogWarning("needDestroyBulletDueToDuration");
                return true;
            }

            return false;
        }
    }
}