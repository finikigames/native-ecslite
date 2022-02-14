using System;
using Leopotam.EcsLite;
using OdinGames.EcsLite.Native.Context;
using OdinGames.EcsLite.Native.NativeOperations;
using OdinGames.EcsLite.Native.NativeOperations.FilterWrapper;
using Unity.Collections;

namespace OdinGames.EcsLite.Native.NativeOperationsService.Base
{
    public interface INativeOperationsService : IDisposable
    {
        void ApplyOperations(EcsSystems systems);

        void ChangeContext(OperationsContext context);

        void ChangeToDefaultContext();
        
        ReadOnlyNativeEntityOperations<T> GetReadOnlyOperations<T>(EcsSystems systems)
            where T : unmanaged;

        ReadWriteNativeEntityOperations<T> GetReadWriteOperations<T>(EcsSystems systems, 
                                                                     Allocator allocator = Allocator.TempJob,
                                                                     int internalBuffersCapacity = 30) 
            where T : unmanaged;

        LazyReadWriteNativeEntityOperations<T> GetLazyReadWriteOperations<T>(EcsSystems systems,
                                                                             Allocator allocator = Allocator.TempJob, 
                                                                             int internalBuffersCapacity = 30)
            where T : unmanaged;

        NativeFilterWrapper GetFilterWrapper(EcsFilter filter);
    }
}
