using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

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
        private AttackerBehaviour _soldierPrefab;
        [SerializeField] 
        private AttackerBehaviour _trollPrefab;

        [Header("Spawn Settings")]
        [SerializeField] 
        private int _soldierCount = 4;
        [SerializeField] 
        private int _trollCount = 1;
        [SerializeField] 
        private float _spawnRadius = 12f;

        private readonly List<AttackerBehaviour> _spawnedEnemies = new();

        private void Start()
        {
            SpawnEnemies();
        }

        private void Update()
        {
            foreach (AttackerBehaviour attackerBehaviour in _spawnedEnemies)
            {
                attackerBehaviour.Update();
            }
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

        private void SpawnEnemy(AttackerBehaviour attacker, string name)
        {
            Vector3 randomPos = _playerTransform.position +
                                Random.insideUnitSphere.normalized * _spawnRadius;
            randomPos.y = _playerTransform.position.y;

            AttackerBehaviour enemy = Instantiate(attacker, randomPos, Quaternion.identity);
            enemy.name = name;
            enemy.SetStageManager(_stageManager);
            enemy.Enable();

            _spawnedEnemies.Add(enemy);
        }
    }
}