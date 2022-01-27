using System;
using Leopotam.EcsLite;
using OdinGames.EcsLite.Native.NativeOperations;
using Unity.Collections;

namespace OdinGames.EcsLite.Native.NativeOperationsService.Base
{
    public interface INativeOperationsService : IDisposable
    {
        void ApplyOperations(EcsSystems systems);
        
        ReadOnlyNativeEntityOperations<T> GetReadOnlyOperations<T>(EcsSystems systems)
            where T : unmanaged;

        ReadWriteNativeEntityOperations<T> GetReadWriteOperations<T>(EcsSystems systems, 
                                                                Allocator allocator = Allocator.TempJob) 
            where T : unmanaged;
    }
}
