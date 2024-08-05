using System.Collections.Generic;
using UnityEngine;

namespace Evironment.MapGenerator
{
    [CreateAssetMenu(fileName = "ScriptableObjectPooling", menuName = "ScriptableObjects/ScriptableObjectPooling", order = 1)]
    public class ScriptableObjectPooling : ScriptableObject
    {
        public List<MapGenerator.TileType> tiles;
        public List<MapGenerator.ObstacleType> obstacles;
        public GameObject fencePrefab;
    }
}
