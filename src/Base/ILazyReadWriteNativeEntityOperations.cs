using Leopotam.EcsLite;
using OdinGames.EcsLite.Native.NativeOperations.ReadWriteOperationsData;
using OdinGames.EcsLite.Native.NativeOperationsWrapper;
using OdinGames.EcsLite.Native.WrappedData;
using Unity.Burst;
using Unity.Collections;

namespace OdinGames.EcsLite.Native.Base
{
    public interface ILazyReadWriteNativeEntityOperations<T> : INativeEntityOperations<T> where T : unmanaged
    {
        public void Init(ReadWriteOperationsInternalData<T> internalData);
        
        void Del(int entity);
        
        void DelEntity(int entity);
        
        ref T GetRef(int entity);
    }
}