using OdinGames.EcsLite.Native.NativeOperations;
using OdinGames.EcsLite.Native.NativeOperations.ReadWriteOperationsData;
using OdinGames.EcsLite.Native.NativeOperationsWrapper.Base;
using Unity.Burst;
using Unity.Collections;

namespace OdinGames.EcsLite.Native.NativeOperationsWrapper
{
    public class NativeLazyReadWriteOperationsWrapper<T> : INativeReadWriteOperationsWrapper, 
                                                           INativeOperationsWrapper<T>
        where T : unmanaged
    {
        public LazyReadWriteNativeEntityOperations<T> Operations;
        
        public delegate void AutoResetDelegate(ref T value);

        public AutoResetDelegate ResetDelegate { get; set; }
        
        public FunctionPointer<AutoResetDelegate> NativeResetDelegate { get; set; }

        public NativeHashMap<int, int> DeleteCache { get; set; }
        public NativeHashMap<int, int> AddCache { get; set; }
        public NativeHashSet<int> EntitiesToRemove { get; set; }

        public void Init(ReadWriteOperationsInternalData<T> internalData,
                         Allocator operationAllocator = Allocator.TempJob, 
                         int defaultCapacity = 30)
        {
            DeleteCache = new NativeHashMap<int, int>(defaultCapacity, operationAllocator);
            AddCache = new NativeHashMap<int, int>(defaultCapacity, operationAllocator);
            EntitiesToRemove = new NativeHashSet<int>(defaultCapacity, operationAllocator);

            //AutoResetHelper.GetResetMethod(this);
            //var isAutoReset = ResetDelegate == null;
            
            internalData.InitCollections(DeleteCache,
                                         AddCache,
                                         EntitiesToRemove);
            
            Operations = default;

            Operations.Init(internalData);
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