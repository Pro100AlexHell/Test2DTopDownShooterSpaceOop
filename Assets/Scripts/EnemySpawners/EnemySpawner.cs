using UnityEngine;

namespace EnemySpawners
{
    public abstract class EnemySpawner
    {
        public abstract void TrySpawnEnemies(float deltaTimeSec, Vector2 playerPos,
            IEnemyCreatorForSpawner iEnemyCreatorForSpawner);
    }
}