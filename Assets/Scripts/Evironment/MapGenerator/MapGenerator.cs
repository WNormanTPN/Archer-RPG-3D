using System;
using System.Collections.Generic;
using Config;
using Generic;
using MyEditor;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Evironment.MapGenerator
{
    [Serializable]
    public class MapDetail
    {
        public int detailId;
        public int viewDistance;
        public int unloadDistance;
        public int tileSpacing;
        public float obstacleSpawnRatio;
        public string objectPoolAssetsPath;
    }

    [Serializable]
    public class MapDetailDataCollection : IConfigCollection
    {
        public Dictionary<string, MapDetail> MapDetails;
        public static MapDetailDataCollection mapDetailDataCollection
        {
            get
            {
                if (_mapDetailDataCollection == null)
                {
                    _mapDetailDataCollection = new MapDetailDataCollection(ConfigDataManager.Instance.GetConfigData<MapDetailDataCollection>());
                }

                return _mapDetailDataCollection;
            }
            private set => _mapDetailDataCollection = value;
        }
        private static MapDetailDataCollection _mapDetailDataCollection;
    
        public MapDetailDataCollection() {}

        [JsonConstructor]
        public MapDetailDataCollection(Dictionary<string, MapDetail> mapDetails)
        {
            this.MapDetails = mapDetails;
        }

        public MapDetailDataCollection(MapDetailDataCollection data)
        {
            var copied = data.DeepCopy();
            MapDetails = copied.MapDetails;
        }

        public void FromJson(string json)
        {
            try
            {
                MapDetails = JsonConvert.DeserializeObject<Dictionary<string, MapDetail>>(json);
            }
            catch (JsonException ex)
            {
                Debug.LogError($"Failed to deserialize MapDetailDataCollection: {ex.Message}");
            }
        }
        
        public MapDetailDataCollection DeepCopy()
        {
            string serializedObject = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<MapDetailDataCollection>(serializedObject);
        }

    }



    
    public class MapGenerator : MonoBehaviour
    {
        [System.Serializable]
        public struct TileType
        {
            public GameObject tilePrefab;
            [Range(0f, 10f)]
            public float ratio;
        }

        [System.Serializable]
        public struct ObstacleType
        {
            public GameObject obstaclePrefab;
            [Range(0f, 10f)]
            public float ratio;
            public Vector2 size;
            public bool canConnect;
        }

        [HideInInspector] public Transform player;
        [HideInInspector]public ObjectPool objectPool;
        
        public bool isLimitedMap = false;
        
        [InspectorGroup("Map Generation Settings")]
        public int viewDistance = 5;
        public int unloadDistance = 10;
        public int tileSpacing = 1;
        [Range(0, 1)] public float obstacleSpawnRatio = 0.1f;
        
        [NonGroup]
        [ShowWhen("isLimitedMap", true, "Fence Settings")] 
        public GameObject fencePrefab;
        [ShowWhen("isLimitedMap", true, "Fence Settings")] 
        public Transform fenceParent;
        
        [InspectorGroup("Pooling Object Settings")]
        public TileType[] tileTypes;
        public Transform tileParent;
        public ObstacleType[] obstacleTypes;
        public Transform obstacleParent;

        
        
        private Vector2Int playerPos;
        private Dictionary<Vector2Int, GameObject> activeTiles;
        private Dictionary<Vector2Int, int> tilePrefabIndexes;
        private float totalTileRatio;
        private Dictionary<Vector2Int, GameObject> activeObstacles;
        private Dictionary<Vector2Int, int> obstaclePrefabIndexes;
        private float totalObstacleRatio;
        private System.Random random = new System.Random();
        
        void Start()
        {
            objectPool = objectPool ?? GameObject.FindGameObjectWithTag("ObjectPool").GetComponent<ObjectPool>();
            player = player ?? GameObject.FindGameObjectWithTag("Player").transform;
            InitializeMap();
            GenerateInitialTilesAndObstacles();
        }

        void FixedUpdate()
        {
            if (isLimitedMap) return;
            if (!player)
            {
                player = GameObject.FindGameObjectWithTag("Player").transform;
            }
            else
            {
                UpdatePlayerPosition();
            }
        }

        void InitializeMap()
        {
            if(!player) player = GameObject.FindGameObjectWithTag("Player").transform;
            isLimitedMap = PlayerPrefs.GetInt("IsLimitedMap", isLimitedMap ? 1 : 0) == 1;
            viewDistance = PlayerPrefs.GetInt("ViewDistance", viewDistance);
            unloadDistance = PlayerPrefs.GetInt("UnloadDistance", unloadDistance);
            tileSpacing = PlayerPrefs.GetInt("TileSpacing", tileSpacing);
            obstacleSpawnRatio = PlayerPrefs.GetFloat("ObstacleSpawnRatio", obstacleSpawnRatio);
            string objectPoolAssetsPath = PlayerPrefs.GetString("ObjectPoolAssetsPath", "");
            var loadAsync = Addressables.LoadAssetAsync<ScriptableObjectPooling>(objectPoolAssetsPath);
            loadAsync.Completed += handle =>
            {
                tileTypes = handle.Result.tiles;
                obstacleTypes = handle.Result.obstacles;
                fencePrefab = handle.Result.fencePrefab;
            };
            playerPos = GetPlayerTilePosition();
            activeTiles = new Dictionary<Vector2Int, GameObject>();
            tilePrefabIndexes = new Dictionary<Vector2Int, int>();
            activeObstacles = new Dictionary<Vector2Int, GameObject>();
            obstaclePrefabIndexes = new Dictionary<Vector2Int, int>();
            
            loadAsync.WaitForCompletion();
            totalTileRatio = CalculateTotalRatio(tileTypes);
            if (obstacleTypes.Length > 0)
            {
                totalObstacleRatio = CalculateTotalRatio(obstacleTypes);
            }
            InitObstaclesSize();

            if (isLimitedMap)
            {
                CreateFenceAroundMap();
            }
        }

        void InitObstaclesSize()
        {
            for (int i = 0; i < obstacleTypes.Length; i++)
            {
                if (!obstacleTypes[i].canConnect || obstacleTypes[i].size != Vector2.zero) continue;

                Renderer[] renderers = obstacleTypes[i].obstaclePrefab.GetComponentsInChildren<Renderer>();
                if (renderers.Length > 0)
                {
                    Bounds totalBounds = renderers[0].bounds;
                    foreach (Renderer renderer in renderers)
                    {
                        totalBounds.Encapsulate(renderer.bounds);
                    }
                    obstacleTypes[i].size = new Vector2(totalBounds.size.x, totalBounds.size.z);
                }
            }
        }

        Vector2Int GetPlayerTilePosition()
        {
            return new Vector2Int(
                Mathf.RoundToInt(player.position.x / tileSpacing),
                Mathf.RoundToInt(player.position.z / tileSpacing)
            );
        }

        void UpdatePlayerPosition()
        {
            Vector2Int newPlayerPos = GetPlayerTilePosition();
            if (newPlayerPos != playerPos)
            {
                playerPos = newPlayerPos;
                if (!isLimitedMap)
                {
                    UpdateTilesAndObstacles();
                }
            }
        }

        float CalculateTotalRatio<T>(T[] types) where T : struct
        {
            float total = 0f;
            foreach (var type in types)
            {
                if (type is TileType tileType)
                {
                    total += tileType.ratio;
                }
                else if (type is ObstacleType obstacleType)
                {
                    total += obstacleType.ratio;
                }
            }
            return total;
        }

        void GenerateInitialTilesAndObstacles()
        {
            int startX = -viewDistance;
            int endX = viewDistance;
            int startY = -viewDistance;
            int endY = viewDistance;
            if (isLimitedMap)
            {
                startX /= 2;
                endX = Mathf.CeilToInt(endX / 2.0f);
                startY /= 2;
                endY = Mathf.CeilToInt(endY / 2.0f);
            }
        
            int offset = unloadDistance - viewDistance;
            for (int x = startX-offset; x < endX+offset; x++)
            {
                for (int y = startY-offset; y < endY+offset; y++)
                {
                    Vector2Int tilePos = new Vector2Int(playerPos.x + x, playerPos.y + y);
                    CreateTileAndObstacleAtPosition(tilePos);
                }
            }
        }

        void UpdateTilesAndObstacles()
        {
            if (isLimitedMap) return;

            RemoveDistantTiles();
            RemoveDistantObstacles();
            CreateTilesAndObstaclesInViewRange();
        }

        void CreateTilesAndObstaclesInViewRange()
        {
            for (int x = -viewDistance; x <= viewDistance; x++)
            {
                for (int y = -viewDistance; y <= viewDistance; y++)
                {
                    Vector2Int tilePos = new Vector2Int(playerPos.x + x, playerPos.y + y);
                    if (!activeTiles.ContainsKey(tilePos))
                    {
                        CreateTileAndObstacleAtPosition(tilePos);
                    }
                }
            }
        }

        void RemoveDistantTiles()
        {
            List<Vector2Int> tilesToRemove = new List<Vector2Int>();
            foreach (var tilePos in activeTiles.Keys)
            {
                if (Vector2Int.Distance(tilePos, playerPos) > unloadDistance)
                {
                    tilesToRemove.Add(tilePos);
                }
            }

            foreach (Vector2Int tilePos in tilesToRemove)
            {
                int tilePrefabIndex = GetTilePrefabIndexFromPosition(tilePos);
                if (tilePrefabIndex == -1) continue;

                GameObject tilePrefab = GetTilePrefab(tilePrefabIndex);
                objectPool.ReturnObject(tilePrefab, activeTiles[tilePos]);
                activeTiles.Remove(tilePos);
            }
        }

        void RemoveDistantObstacles()
        {
            List<Vector2Int> obstaclesToRemove = new List<Vector2Int>();
            foreach (var obstaclePos in activeObstacles.Keys)
            {
                if (Vector2Int.Distance(obstaclePos, playerPos) > unloadDistance)
                {
                    obstaclesToRemove.Add(obstaclePos);
                }
            }

            foreach (Vector2Int obstaclePos in obstaclesToRemove)
            {
                int obstaclePrefabIndex = GetObstaclePrefabIndexFromPosition(obstaclePos);
                if (obstaclePrefabIndex == -1) continue;

                GameObject obstaclePrefab = GetObstaclePrefab(obstaclePrefabIndex);
                objectPool.ReturnObject(obstaclePrefab, activeObstacles[obstaclePos]);
                activeObstacles.Remove(obstaclePos);
            }
        }

        void CreateTileAndObstacleAtPosition(Vector2Int tilePos)
        {
            if (!tilePrefabIndexes.TryGetValue(tilePos, out int tilePrefabIndex))
            {
                tilePrefabIndex = GetRandomIndexByRatio(tileTypes, totalTileRatio);
                tilePrefabIndexes[tilePos] = tilePrefabIndex;

                if (obstacleTypes.Length > 0)
                {
                    float obstacleRandomValue = (float)random.NextDouble();
                    if (obstacleRandomValue <= obstacleSpawnRatio)
                    {
                        int obstaclePrefabIndex = GetRandomIndexByRatio(obstacleTypes, totalObstacleRatio);
                        obstaclePrefabIndexes[tilePos] = obstaclePrefabIndex;
                    }
                }
            }

            GameObject tilePrefab = GetTilePrefab(tilePrefabIndex);
            GameObject tile = objectPool.GetObject(tilePrefab, tileParent);
            tile.transform.position = new Vector3(tilePos.x * tileSpacing, 0, tilePos.y * tileSpacing);
            activeTiles[tilePos] = tile;

            if (obstacleTypes.Length > 0 && obstaclePrefabIndexes.TryGetValue(tilePos, out int obstacleIndex))
            {
                CreateObstacleAtPosition(tilePos, obstacleIndex);
            }
        }

        void CreateObstacleAtPosition(Vector2Int obstaclePos, int obstacleIndex)
        {
            GameObject obstaclePrefab = GetObstaclePrefab(obstacleIndex);
            GameObject obstacle = objectPool.GetObject(obstaclePrefab, obstacleParent);

            Vector3 obstaclePosition = new Vector3(obstaclePos.x * tileSpacing, 0, obstaclePos.y * tileSpacing);
            Vector2 outSize = Vector2.one;
            if (obstacleTypes[obstacleIndex].canConnect)
            {
                AdjustObstacleSize(obstaclePos, obstaclePosition, obstacleTypes[obstacleIndex].size, out outSize);
            }

            obstacle.transform.position = obstaclePosition;
            obstacle.transform.localScale = new Vector3(outSize.x, 1, outSize.y);
            obstaclePrefabIndexes[obstaclePos] = obstacleIndex;
            activeObstacles[obstaclePos] = obstacle;
        }

        void AdjustObstacleSize(Vector2Int obstaclePos, Vector3 position, Vector2 size, out Vector2 outSize)
        {
            outSize = Vector2.one;
            Vector2Int[] adjacentPositions = GetAdjacentTilePositions(obstaclePos);
            foreach (var adjPos in adjacentPositions)
            {
                if (activeObstacles.TryGetValue(adjPos, out GameObject adjacentObstacle))
                {
                    int adjacentIndex = GetObstaclePrefabIndexFromPosition(adjPos);
                    if (adjacentIndex != -1 && obstacleTypes[adjacentIndex].canConnect)
                    {
                        // Adjust the scale of the obstacle to connect together by X or Z axis
                        if (obstaclePos.x == adjPos.x)
                        {
                            outSize = new Vector2(size.x, tileSpacing / size.y);
                            AdjustAdjacentSize(adjPos, outSize);
                        }
                        else if (obstaclePos.y == adjPos.y)
                        {
                            outSize = new Vector2( tileSpacing / size.x , size.y);
                            AdjustAdjacentSize(adjPos, outSize);
                        }
                    }
                }
            }
        }
    
        void AdjustAdjacentSize(Vector2Int adjPos, Vector2 size)
        {
            if (activeObstacles.TryGetValue(adjPos, out GameObject adjacentObstacle))
            {
                if (adjacentObstacle.transform.localScale != Vector3.one) return;
            
                adjacentObstacle.transform.localScale = new Vector3(size.x, 1, size.y);
            }
        }

        Vector2Int[] GetAdjacentTilePositions(Vector2Int center)
        {
            return new Vector2Int[]
            {
                new Vector2Int(center.x + 1, center.y),
                new Vector2Int(center.x - 1, center.y),
                new Vector2Int(center.x, center.y + 1),
                new Vector2Int(center.x, center.y - 1)
            };
        }

        int GetRandomIndexByRatio<T>(T[] types, float totalRatio) where T : struct
        {
            float randomValue = (float)random.NextDouble() * totalRatio;
            float cumulativeProbability = 0f;

            for (int i = 0; i < types.Length; i++)
            {
                if (types[i] is TileType tileType)
                {
                    cumulativeProbability += tileType.ratio;
                }
                else if (types[i] is ObstacleType obstacleType)
                {
                    cumulativeProbability += obstacleType.ratio;
                }

                if (randomValue <= cumulativeProbability)
                {
                    return i;
                }
            }

            return 0; // Fallback in case of rounding errors
        }

        float GetRatio(TileType type)
        {
            return type.ratio;
        }

        float GetRatio(ObstacleType type)
        {
            return type.ratio;
        }

        GameObject GetTilePrefab(int index)
        {
            return tileTypes[index].tilePrefab;
        }

        GameObject GetObstaclePrefab(int index)
        {
            return obstacleTypes[index].obstaclePrefab;
        }

        int GetTilePrefabIndexFromPosition(Vector2Int tilePos)
        {
            if (tilePrefabIndexes.TryGetValue(tilePos, out var index))
            {
                return index;
            }
            return -1;
        }

        int GetObstaclePrefabIndexFromPosition(Vector2Int obstaclePos)
        {
            if (obstaclePrefabIndexes.TryGetValue(obstaclePos, out var index))
            {
                return index;
            }
            return -1;
        }

        void CreateFenceAroundMap()
        {
            int fenceDiameter = viewDistance + 1;
            int startX = -fenceDiameter / 2;
            int startY = -fenceDiameter / 2;
            int endX = Mathf.CeilToInt(fenceDiameter / 2.0f);
            int endY = Mathf.CeilToInt(fenceDiameter / 2.0f);
            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    if (x == startX || x == endX || y == startY || y == endY)
                    {
                        Vector3 fencePos = new Vector3(x * tileSpacing, 0, y * tileSpacing);
                        GameObject fence = objectPool.GetObject(fencePrefab, fenceParent);

                        // Determine rotation based on position
                        Quaternion rotation = Quaternion.identity;
                        if (x == startX || x == endX) // Vertical fences
                        {
                            rotation = Quaternion.Euler(0, 90, 0);
                        }
                        else if (y == startY || y == endY) // Horizontal fences
                        {
                            rotation = Quaternion.Euler(0, 0, 0);
                        }

                        fence.transform.position = fencePos;
                        fence.transform.rotation = rotation;

                        // Create an additional fence for the corner points
                        if ((x == startX || x == endX) && (y == startY || y == endY))
                        {
                            // Create a fence for the other axis at the same position
                            GameObject cornerFence = objectPool.GetObject(fencePrefab, fenceParent);
                            cornerFence.transform.position = fencePos;
                            cornerFence.transform.rotation = (rotation == Quaternion.Euler(0, 90, 0)) ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 90, 0);
                        }
                    }
                }
            }
        }

    }
}
