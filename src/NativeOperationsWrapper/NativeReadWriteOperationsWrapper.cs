using Leopotam.EcsLite;
using OdinGames.EcsLite.Native.Base;
using OdinGames.EcsLite.Native.NativeOperations;
using OdinGames.EcsLite.Native.NativeOperationsWrapper.Base;
using OdinGames.EcsLite.Native.WrappedData;
using Unity.Burst;
using Unity.Collections;

namespace OdinGames.EcsLite.Native.NativeOperationsWrapper
{
    public class NativeReadWriteOperationsWrapper<T> : INativeReadWriteOperationsWrapper, 
                                                       INativeOperationsWrapper<T>
        where T : unmanaged
    {
        public ReadWriteNativeEntityOperations<T> Operations;
        
        public delegate void AutoResetDelegate(ref T value);

        public AutoResetDelegate ResetDelegate { get; set; }
        
        public FunctionPointer<AutoResetDelegate> NativeResetDelegate { get; set; }

        public NativeHashMap<int, int> DeleteCache { get; set; }
        public NativeHashMap<int, int> AddCache { get; set; }
        public NativeHashSet<int> EntitiesToRemove { get; set; }

        public void Init(NativeWrappedData<int> sparseItems,
                         NativeWrappedData<T> denseItems, 
                         NativeWrappedData<int> recycledItems, 
                         NativeWrappedData<EntityData> entities, 
                         ref int recycledItemsCount, 
                         ref int denseItemsCount, 
                         int id, 
                         Allocator operationAllocator = Allocator.TempJob, 
                         int defaultCapacity = 30)
        {
            DeleteCache = new NativeHashMap<int, int>(defaultCapacity, operationAllocator);
            AddCache = new NativeHashMap<int, int>(defaultCapacity, operationAllocator);
            EntitiesToRemove = new NativeHashSet<int>(defaultCapacity, operationAllocator);
            
            Operations = default;

            //AutoResetHelper.GetResetMethod(this);

            //var isAutoReset = ResetDelegate == null;

            Operations.Init(sparseItems,
                            denseItems, 
                            recycledItems, 
                            entities, 
                            ref recycledItemsCount, 
                            ref denseItemsCount, 
                            DeleteCache, 
                            AddCache, 
                            EntitiesToRemove, 
                            id);
        }

        public void Dispose()
        {
            Operations.Dispose();
            DeleteCache.Dispose();
            AddCache.Dispose();
            EntitiesToRemove.Dispose();
        }
    }
}