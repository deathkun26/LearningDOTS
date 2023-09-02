using Unity.Entities;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Transforms;
using Unity.Jobs;
using UnityEngine;

namespace Example
{
    [BurstCompile]
    public partial struct RotateSystem : ISystem
    {
        public void OnCreate(ref SystemState state) { }

        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer.ParallelWriter ecb = GetEntityCommandBuffer(ref state);

            new ProcessRotateJob
            {
                DeltaTime = Time.deltaTime,
                RotateSpeed = 5f
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
    public partial struct ProcessRotateJob : IJobEntity
    {
        public float DeltaTime;
        public float RotateSpeed;
        private void Execute(ref LocalTransform localTransform, Rotation rotation)
        {
            localTransform = localTransform.RotateY(DeltaTime * RotateSpeed);
        }
    }
}
