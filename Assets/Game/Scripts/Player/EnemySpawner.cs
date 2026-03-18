using System.Collections.Generic;
using UnityEngine;

namespace BelgianAI
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] 
        private StageManager _stageManager;
        [SerializeField] 
        private Transform _playerTransform;

        [Header("Enemy Prefabs")]
        [SerializeField] 
        private GameObject _soldierPrefab;
        [SerializeField] 
        private GameObject _trollPrefab;

        [Header("Spawn Settings")]
        [SerializeField] 
        private int _soldierCount = 4;
        [SerializeField] 
        private int _trollCount = 1;
        [SerializeField] 
        private float _spawnRadius = 12f;

        private readonly List<GameObject> _spawnedEnemies = new();

        private void Start()
        {
            SpawnEnemies();
        }

        private void SpawnEnemies()
        {
            for (int i = 0; i < _soldierCount; i++)
            {
                SpawnEnemy(_soldierPrefab, $"Soldier_{i}");
            }

            for (int i = 0; i < _trollCount; i++)
            {
                SpawnEnemy(_trollPrefab, $"Troll_{i}");
            }
        }

        private void SpawnEnemy(GameObject prefab, string name)
        {
            Vector3 randomPos = _playerTransform.position +
                                Random.insideUnitSphere.normalized * _spawnRadius;
            randomPos.y = _playerTransform.position.y;

            GameObject enemy = Instantiate(prefab, randomPos, Quaternion.identity);
            enemy.name = name;
            
            var attackBehaviour = enemy.GetComponent<AttackBehaviour>();
            if (attackBehaviour != null)
            {
                attackBehaviour.SetStageManager(_stageManager);
            }

            _spawnedEnemies.Add(enemy);
        }
    }
}