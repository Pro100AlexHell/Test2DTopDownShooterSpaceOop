using UnityEngine;

namespace EnemySpawners
{
    public interface IEnemyCreatorForSpawner
    {
        void SpawnEnemy(Vector2 pos, int enemyTypeIndex);
    }
}