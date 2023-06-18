using System.Collections.Generic;
using UnityEngine;

namespace MapGenerators
{
    public class MapGeneratorWithAsteroids : IMapGenerator
    {
        public void Generate(Rect nonGenerationRect, Rect mapRect,
            int asteroidCount, float asteroidCircleColliderRadius, List<Asteroid> resultAsteroids)
        {
            for (int i = 0; i < asteroidCount; i++)
            {
                Vector2 p = GeneratePos(nonGenerationRect, mapRect);
                Asteroid asteroid = new Asteroid(p, asteroidCircleColliderRadius);
                resultAsteroids.Add(asteroid);
            }
        }

        private Vector2 GeneratePos(Rect nonGenerationRect, Rect mapRect)
        {
            // random point in a mapRect
            // todo NOTE: while - due to nonGenerationRect (not infinite loop, for correct parameters)
            while (true)
            {
                Vector2 p = new Vector2(
                    Random.value * mapRect.width + mapRect.xMin,
                    Random.value * mapRect.height + mapRect.yMin
                );

                if (!nonGenerationRect.Contains(p))
                {
                    return p;
                }
            }
        }
    }
}