using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace Example
{
    public class SpawnerAuthoring : MonoBehaviour
    {
        public GameObject Prefab;
        public float SpawnRate;
    }

    public class SpawnerBaker : Baker<SpawnerAuthoring>
    {
        public override void Bake(SpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Spawner
            {
                Prefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic),
                SpawnPosition = authoring.transform.position,
                NextSpawnTime = 0.0f,
                SpawnRate = authoring.SpawnRate
            });
        }
    }
}
