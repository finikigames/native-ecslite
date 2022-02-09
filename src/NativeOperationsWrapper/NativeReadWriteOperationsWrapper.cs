using OdinGames.EcsLite.Native.NativeOperations;
using OdinGames.EcsLite.Native.NativeOperations.ReadWriteOperationsData;
using OdinGames.EcsLite.Native.NativeOperationsWrapper.Base;
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

        public int ID { get; private set; }
        public NativeList<int> DeleteCache { get; private set; }
        public NativeList<int> AddCache { get; private set; }
        public NativeList<int> EntitiesToRemove { get; private set; }

        public void Init(ReadWriteOperationsInternalData<T> internalData,
                         Allocator operationAllocator = Allocator.TempJob, 
                         int defaultCapacity = 30)
        {
            ID = internalData.ID;
            DeleteCache = new NativeList<int>(defaultCapacity, operationAllocator);
            AddCache = new NativeList<int>(defaultCapacity, operationAllocator);
            EntitiesToRemove = new NativeList<int>(defaultCapacity, operationAllocator);
            
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