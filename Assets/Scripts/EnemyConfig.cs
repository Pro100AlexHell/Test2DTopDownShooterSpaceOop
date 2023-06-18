using System;
using UnityEngine;
using Views;

public class EnemyConfig
{
    public readonly int Health;

    public readonly float CircleColliderRadius;

    public readonly float MoveSpeedPerSec;

    public readonly Func<EnemyViewPrefabCollection, GameObject> ViewPrefabFunc;

    public EnemyConfig(int health, float circleColliderRadius, float moveSpeedPerSec,
        Func<EnemyViewPrefabCollection, GameObject> viewPrefabFunc)
    {
        Health = health;
        CircleColliderRadius = circleColliderRadius;
        MoveSpeedPerSec = moveSpeedPerSec;
        ViewPrefabFunc = viewPrefabFunc;
    }
}