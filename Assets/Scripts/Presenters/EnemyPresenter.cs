using UnityEngine;

namespace Presenters
{
    public class EnemyPresenter : BasePresenter<Enemy>
    {
        public EnemyPresenter(Enemy model, GameObject view)
            : base(model, view)
        {
        }

        public void TryRecalculateTargetEnemyToPlayerIfDurationExpired(float deltaTimeSec,
            float recalculateTargetPeriodSec, Vector2 playerPos)
        {
            Model.TryRecalculateTargetEnemyToPlayerIfDurationExpired(deltaTimeSec,
                recalculateTargetPeriodSec, playerPos);
        }
    }
}