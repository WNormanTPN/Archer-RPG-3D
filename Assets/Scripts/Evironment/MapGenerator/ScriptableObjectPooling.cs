using System.Collections.Generic;
using UnityEngine;

namespace Evironment.MapGenerator
{
    [CreateAssetMenu(fileName = "ScriptableObjectPooling", menuName = "ScriptableObjects/ScriptableObjectPooling", order = 1)]
    public class ScriptableObjectPooling : ScriptableObject
    {
        public MapGenerator.TileType[] tiles;
        public MapGenerator.ObstacleType[] obstacles;
        public GameObject fencePrefab;
    }
}
