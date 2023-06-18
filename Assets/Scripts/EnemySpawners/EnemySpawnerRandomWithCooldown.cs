using UnityEngine;

namespace EnemySpawners
{
    /// <summary>
    /// Every N seconds, creates 1 enemy at a random point in a radius around the player
    /// </summary>
    public class EnemySpawnerRandomWithCooldown : EnemySpawner
    {
        private float _spawnCooldownRemainedSec;

        private readonly float _spawnIntervalSec;

        private readonly Rect _mapRect;

        private readonly float _minRadiusFromPlayer;
        private readonly float _maxRadiusFromPlayer;

        private readonly float _probabilityOfEnemyType1; // [0 -- 1]

        public EnemySpawnerRandomWithCooldown(float spawnIntervalSec, Rect mapRect,
            float minRadiusFromPlayer, float maxRadiusFromPlayer, int probabilityPercentOfEnemyType1)
        {
            _spawnIntervalSec = spawnIntervalSec;
            _spawnCooldownRemainedSec = _spawnIntervalSec;
            _mapRect = mapRect;
            _minRadiusFromPlayer = minRadiusFromPlayer;
            _maxRadiusFromPlayer = maxRadiusFromPlayer;

            if (probabilityPercentOfEnemyType1 <= 0)
            {
                _probabilityOfEnemyType1 = 0;
            }
            else if (probabilityPercentOfEnemyType1 >= 100)
            {
                _probabilityOfEnemyType1 = 1;
            }
            else
            {
                _probabilityOfEnemyType1 = probabilityPercentOfEnemyType1 * 0.01f;
            }
            Debug.Log("probabilityPercentOfEnemyType1 = " + probabilityPercentOfEnemyType1);
            Debug.Log("_probabilityOfEnemyType1 = " + _probabilityOfEnemyType1);
        }

        public override void TrySpawnEnemies(float deltaTimeSec, Vector2 playerPos,
            IEnemyCreatorForSpawner iEnemyCreatorForSpawner)
        {
            _spawnCooldownRemainedSec -= deltaTimeSec;
            if (_spawnCooldownRemainedSec <= 0)
            {
                _spawnCooldownRemainedSec = _spawnIntervalSec;
                DoSpawnEnemies(playerPos, iEnemyCreatorForSpawner);
            }
        }

        protected virtual void DoSpawnEnemies(Vector2 playerPos, IEnemyCreatorForSpawner enemyCreatorForSpawner)
        {
            Vector2 pos = GeneratePos(playerPos);
            int enemyTypeIndex = GenerateEnemyTypeIndex();
            enemyCreatorForSpawner.SpawnEnemy(pos, enemyTypeIndex);
        }

        private Vector2 GeneratePos(Vector2 playerPos)
        {
            // random point in a radius around the player (_minRadiusFromPlayer -- _maxRadiusFromPlayer)
            // todo NOTE: while - due to possible out of bounds (not infinite loop, for correct parameters)
            while (true)
            {
                float angleRad = Random.value * Mathf.PI * 2;
                float range = Random.value * (_maxRadiusFromPlayer - _minRadiusFromPlayer) + _minRadiusFromPlayer;
                Vector2 p = new Vector2(
                    Mathf.Cos(angleRad) * range + playerPos.x,
                    Mathf.Sin(angleRad) * range + playerPos.y
                );

                if (_mapRect.Contains(p))
                {
                    return p;
                }
            }
        }

        private int GenerateEnemyTypeIndex()
        {
            return (Random.value <= _probabilityOfEnemyType1) ? 1 : 2;
        }
    }
}