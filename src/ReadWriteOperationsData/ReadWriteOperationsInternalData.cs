using Leopotam.EcsLite;
using OdinGames.EcsLite.Native.Extensions;
using OdinGames.EcsLite.Native.NativeOperations.ReadWriteOperationsData.Base;
using OdinGames.EcsLite.Native.NativeOperationsWrapper;
using OdinGames.EcsLite.Native.WrappedData;
using Unity.Burst;
using Unity.Collections;

namespace OdinGames.EcsLite.Native.NativeOperations.ReadWriteOperationsData
{
    public struct ReadWriteOperationsInternalData<T> : IReadWriteOperationsInternalData<T>
        where T : unmanaged
    {
        public NativeWrappedData<int> SparseItems;
        public NativeWrappedData<T> DenseItems;
        public NativeWrappedData<int> RecycledItems;
        public NativeWrappedData<EcsWorld.EntityData> Entities;

        public UnsafeExtensions.SharedIndex RecycledItemsCount;
        public UnsafeExtensions.SharedIndex DenseItemsCount;

        public NativeList<int>.ParallelWriter ComponentsToRemove;
        public NativeList<int>.ParallelWriter ComponentsToAdd;

        public NativeList<int>.ParallelWriter EntitiesToRemove;

        public int ID;

        public bool IsAutoReset;
        public FunctionPointer<NativeReadWriteOperationsWrapper<T>.AutoResetDelegate> AutoReset;

        public void Init(NativeWrappedData<int> sparseItems,
                         NativeWrappedData<T> denseItems,
                         NativeWrappedData<int> recycledItems,
                         NativeWrappedData<EcsWorld.EntityData> entities,
                         ref int recycledItemsCount,
                         ref int denseItemsCount,
                         int id)
        {
            SparseItems = sparseItems;
            DenseItems = denseItems;
            RecycledItems = recycledItems;
            Entities = entities;

            RecycledItemsCount = new UnsafeExtensions.SharedIndex(ref recycledItemsCount);
            DenseItemsCount = new UnsafeExtensions.SharedIndex(ref denseItemsCount);

            ID = id;
        }

        public void InitCollections(NativeList<int> componentsToRemove,
                                    NativeList<int> componentsToAdd, 
                                    NativeList<int> entitiesToRemove, 
                                    bool isAutoReset = false, 
                                    FunctionPointer<NativeReadWriteOperationsWrapper<T>.AutoResetDelegate> resetDelegate = default)
        {
            ComponentsToRemove = componentsToRemove.AsParallelWriter();
            ComponentsToAdd = componentsToAdd.AsParallelWriter();
            EntitiesToRemove = entitiesToRemove.AsParallelWriter();
            //_isAutoReset = isAutoReset;
            //_autoReset = autoReset;
        }
    }
}