using Unity.Entities;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Transforms;
using UnityEngine;

namespace Example
{
    // [BurstCompile]
    // public partial struct SpawnerSystem : ISystem
    // {
    //     public void OnCreate(ref SystemState state) { }

    //     public void OnDestroy(ref SystemState state) { }

    //     [BurstCompile]
    //     public void OnUpdate(ref SystemState state)
    //     {
    //         // Queries for all Spawner components. Uses RefRW because this system wants
    //         // to read from and write to the component. If the system only needed read-only
    //         // access, it would use RefRO instead.
    //         foreach (RefRW<Spawner> spawner in SystemAPI.Query<RefRW<Spawner>>())
    //         {
    //             ProcessSpawner(ref state, spawner);
    //         }
    //     }

    //     private void ProcessSpawner(ref SystemState state, RefRW<Spawner> spawner)
    //     {
    //         // If the next spawn time has passed.
    //         if (spawner.ValueRO.NextSpawnTime < SystemAPI.Time.ElapsedTime)
    //         {
    //             // Spawns a new entity and positions it at the spawner.
    //             Entity newEntity = state.EntityManager.Instantiate(spawner.ValueRO.Prefab);
    //             // LocalPosition.FromPosition returns a Transform initialized with the given position.
    //             float3 randomPositionOffset = new float3
    //             (
    //                 UnityEngine.Random.Range(0f, 10f),
    //                 UnityEngine.Random.Range(0f, 10f),
    //                 UnityEngine.Random.Range(0f, 10f)
    //             );
    //             state.EntityManager.SetComponentData(newEntity, LocalTransform.FromPosition(spawner.ValueRO.SpawnPosition + randomPositionOffset));

    //             // Resets the next spawn time.
    //             spawner.ValueRW.NextSpawnTime = (float)SystemAPI.Time.ElapsedTime + spawner.ValueRO.SpawnRate;
    //         }
    //     }
    // }
    [BurstCompile]
    public partial struct OptimizedSpawnerSystem : ISystem
    {
        private Unity.Mathematics.Random _random;
        public void OnCreate(ref SystemState state)
        {
            _random = new Unity.Mathematics.Random(1);
        }

        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer.ParallelWriter ecb = GetEntityCommandBuffer(ref state);
            _random.InitState((uint)UnityEngine.Random.Range(1, int.MaxValue));

            // Creates a new instance of the job, assigns the necessary data, and schedules the job in parallel.
            new ProcessSpawnerJob
            {
                Random = _random,
                ElapsedTime = SystemAPI.Time.ElapsedTime,
                Ecb = ecb
            }.ScheduleParallel();
        }

        private EntityCommandBuffer.ParallelWriter GetEntityCommandBuffer(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
            return ecb.AsParallelWriter();
        }
    }

    [BurstCompile]
    public partial struct ProcessSpawnerJob : IJobEntity
    {
        public Unity.Mathematics.Random Random;
        public EntityCommandBuffer.ParallelWriter Ecb;
        public double ElapsedTime;

        // IJobEntity generates a component data query based on the parameters of its `Execute` method.
        // This example queries for all Spawner components and uses `ref` to specify that the operation
        // requires read and write access. Unity processes `Execute` for each entity that matches the
        // component data query.
        private void Execute([ChunkIndexInQuery] int chunkIndex, ref Spawner spawner)
        {
            // If the next spawn time has passed.
            if (spawner.NextSpawnTime < ElapsedTime)
            {
                // Spawns a new entity and positions it at the spawner.
                Entity newEntity = Ecb.Instantiate(chunkIndex, spawner.Prefab);
                float3 randomPositionOffset = new float3
                (
                    Random.NextFloat(-10, 10),
                    Random.NextFloat(-10, 10),
                    Random.NextFloat(-10, 10)
                );
                Ecb.SetComponent(chunkIndex, newEntity, LocalTransform.FromPosition(spawner.SpawnPosition + randomPositionOffset));
                Ecb.AddComponent(chunkIndex, newEntity, new Rotation { });
                // Resets the next spawn time.
                spawner.NextSpawnTime = (float)ElapsedTime + spawner.SpawnRate;
            }
        }
    }

}
