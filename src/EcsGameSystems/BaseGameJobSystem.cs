using Leopotam.EcsLite;
using OdinGames.EcsLite.Native.NativeOperationsService.Base;
using Unity.Jobs;

namespace OdinGames.EcsLite.Native.EcsGameSystems
{
    /// <summary>
    /// Sample realization of base game system that Schedule job of specified type
    /// </summary>
    /// <typeparam name="TJob"></typeparam>
    public abstract class BaseGameJobSystem<TJob> : IEcsRunSystem where TJob : struct, IJobParallelFor
    {
        protected INativeOperationsService OperationsService;
    
        protected abstract INativeOperationsService ProvideNativeOperationsService();
        
        protected abstract void SetData(EcsSystems systems, ref TJob job);

        protected virtual void GetData(EcsSystems systems, ref TJob job) { }

        protected abstract int GetChunkSize(EcsSystems systems);

        protected abstract EcsFilter GetFilter(EcsWorld world);

        protected virtual bool RunConditions(EcsSystems systems) => true;

        public void Run(EcsSystems systems)
        {
            if (!RunConditions(systems)) return;
            if (OperationsService == null) OperationsService = ProvideNativeOperationsService();

            var filterEntitiesCount = GetFilter(systems.GetWorld()).GetEntitiesCount();
            if (filterEntitiesCount > 0)
            {
                TJob job = default;
                SetData(systems, ref job);

                job.Schedule(filterEntitiesCount, GetChunkSize(systems)).Complete();
                
                OperationsService.ApplyOperations(systems);
                
                GetData(systems, ref job);
            }
        }
    }
}
