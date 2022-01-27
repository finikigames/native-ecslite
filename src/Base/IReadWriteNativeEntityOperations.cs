using Leopotam.EcsLite;
using OdinGames.EcsLite.Native.NativeOperationsWrapper;
using OdinGames.EcsLite.Native.WrappedData;
using Unity.Burst;
using Unity.Collections;

namespace OdinGames.EcsLite.Native.Base
{
    public interface IReadWriteNativeEntityOperations<T> : INativeEntityOperations<T> where T : unmanaged
    {
        void Init(NativeWrappedData<int> sparseItems,
                  NativeWrappedData<T> denseItems, 
                  NativeWrappedData<int> recycledItems, 
                  NativeWrappedData<EntityData> entities,
                  ref int recycledItemsCount, 
                  ref int denseItemsCount,
                  NativeHashMap<int, int> componentsToRemove, 
                  NativeHashMap<int, int> componentsToAdd,
                  NativeHashSet<int> entitiesToRemove,
                  int id,
                  bool isAutoReset, 
                  FunctionPointer<NativeReadWriteOperationsWrapper<T>.AutoResetDelegate> autoReset = default);
        
        void Add(int entity, T value);
        
        ref T Add(int entity);
        
        void Del(int entity);
        
        void DelEntity(int entity);
        
        ref T GetRef(int entity);
    }
}