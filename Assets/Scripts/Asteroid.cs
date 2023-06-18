using GridForColliders;
using UnityEngine;

public class Asteroid : ObjectInGridWithColliderCircle<ObjectTypeInGrid>
{
    // todo NOTE: without health, like health == 1

    public Asteroid(Vector2 pos, float circleColliderRadius)
        : base(pos, ObjectTypeInGrid.Asteroid,
            circleColliderRadius, CircleApproximationPrecision.Point8 // (8 точек т.к. средний объект)
        )
    {
    }
}