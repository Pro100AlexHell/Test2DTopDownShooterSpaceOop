using System.Collections.Generic;

public class EnemyConfigCollection
{
    public readonly List<EnemyConfig> EnemyConfigByEnemyTypeIndex = new List<EnemyConfig>()
    {
        null, // ([0] reserved)
        new EnemyConfig( // [1] (simple)
            1,
            33, // todo choose based on image size !!
            100,
            collection => collection.Enemy1ViewPrefab
        ),
        new EnemyConfig( // [2] (healthier, slower)
            2,
            48, // todo choose based on image size !!
            75,
            collection => collection.Enemy2ViewPrefab
        ),
    };
}