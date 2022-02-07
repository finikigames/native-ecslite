using Leopotam.EcsLite;
using OdinGames.EcsLite.Native.NativeOperationsWrapper;
using OdinGames.EcsLite.Native.WrappedData;
using Unity.Burst;
using Unity.Collections;

namespace OdinGames.EcsLite.Native.NativeOperations.ReadWriteOperationsData.Base
{
    public interface IReadWriteOperationsInternalData<T> 
        where T : unmanaged
    {
        void Init(NativeWrappedData<int> sparseItems,
                  NativeWrappedData<T> denseItems, 
                  NativeWrappedData<int> recycledItems, 
                  NativeWrappedData<EntityData> entities, 
                  ref int recycledItemsCount, 
                  ref int denseItemsCount,
                  int id);

        void InitCollections(NativeHashMap<int, int> componentsToRemove,
                             NativeHashMap<int, int> componentsToAdd, 
                             NativeHashSet<int> entitiesToRemove, 
                             bool isAutoReset = false, 
                             FunctionPointer<NativeReadWriteOperationsWrapper<T>.AutoResetDelegate> resetDelegate = default);
    }
}