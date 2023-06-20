using GridForColliders;
using UnityEngine;

public class Enemy : ObjectInGridWithColliderCircle<ObjectTypeInGrid>
{
    public int Health { get; private set; }

    protected float SpeedPerSec { get; private set; }

    /// <summary>
    /// Remained time to recalculate target (for optimization - not every frame)
    /// </summary>
    private float _durationSecBeforeRecalculateTarget;

    public Enemy(Vector2 pos, float circleColliderRadius, int healthStart, float speedPerSec)
        : base(pos, ObjectTypeInGrid.Enemy,
            circleColliderRadius, CircleApproximationPrecision.Point8 // (8 точек т.к. средний объект)
        )
    {
        Health = healthStart;
        SpeedPerSec = speedPerSec;
        _durationSecBeforeRecalculateTarget = 0; // (first recalculation will be immediately)
    }

    public void TryRecalculateTargetEnemyToPlayerIfDurationExpired(float deltaTimeSec,
        float recalculateTargetPeriodSec, Vector2 playerPos)
    {
        _durationSecBeforeRecalculateTarget -= deltaTimeSec;
        if (_durationSecBeforeRecalculateTarget <= 0)
        {
            _durationSecBeforeRecalculateTarget = recalculateTargetPeriodSec;
            RecalculateTargetEnemyToPlayer(playerPos);
        }
    }

    private void RecalculateTargetEnemyToPlayer(Vector2 playerPos)
    {
        float angleRad = Mathf.Atan2(playerPos.y - Pos.y, playerPos.x - Pos.x);
        DeltaPosPerSec = new Vector2(
            Mathf.Cos(angleRad) * SpeedPerSec,
            Mathf.Sin(angleRad) * SpeedPerSec
        );
    }

    // todo NOTE: better to use 'default interface methods' \ 'TRAIT' C# 8.0; but I'm afraid to use it because of bugs in new versions of Unity
    public void AddHealth(int delta)
    {
        Health += delta;
    }

    public bool IsAlive()
    {
        return Health > 0;
    }
}