using System.Collections.Generic;
using UnityEngine;

namespace MapGenerators
{
    public interface IMapGenerator
    {
        void Generate(Rect nonGenerationRect, Rect mapRect,
            int asteroidCount, float asteroidCircleColliderRadius, List<Asteroid> resultAsteroids);
    }
}