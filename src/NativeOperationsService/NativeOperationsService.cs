using System.Collections.Generic;
using Codice.Client.BaseCommands.BranchExplorer.ExplorerData;
using Leopotam.EcsLite;
using OdinGames.EcsLite.Native.Context;
using OdinGames.EcsLite.Native.Extensions;
using OdinGames.EcsLite.Native.NativeOperations;
using OdinGames.EcsLite.Native.NativeOperations.FilterWrapper;
using OdinGames.EcsLite.Native.NativeOperations.ReadWriteOperationsData;
using OdinGames.EcsLite.Native.NativeOperationsService.Base;
using Unity.Collections;

namespace OdinGames.EcsLite.Native.NativeOperationsService
{
    public class NativeOperationsService : INativeOperationsService
    {
        private readonly Dictionary<OperationsContext, ContextOperationsHolder> _contextOperations;
        private readonly OperationsContext _defaultContext;
        private OperationsContext _currentContext;

        [UnityEngine.Scripting.Preserve]
        public NativeOperationsService()
        {
            _contextOperations = new Dictionary<OperationsContext, ContextOperationsHolder>();
            _defaultContext = OperationsContext.Create();
            _currentContext = _defaultContext;
        }

        public void ApplyOperations(EcsSystems systems)
        {
            var context = _contextOperations[_currentContext];
            context.ApplyContextOperations(systems);   
        }

        public NativeFilterWrapper GetFilterWrapper(EcsFilter filter)
        {
            NativeFilterWrapper wrapper = default;

            wrapper.Init(filter);
            
            ContextOperationsHolder holder = ResolveHolder();
            holder.RegisterFilter(wrapper);

            return wrapper;
        }

        public void ChangeContext(OperationsContext context) 
            => _currentContext = context;

        public void ChangeToDefaultContext()
            => _currentContext = _defaultContext;

        public ReadOnlyNativeEntityOperations<T> GetReadOnlyOperations<T>(EcsSystems systems)
            where T : unmanaged
        {
            var pool = systems.GetWorld().GetPool<T>();
            var sparse = pool.GetRawSparseItems();
            var dense = pool.GetRawDenseItems();

            var nativeSparse = sparse.WrapToReadOnlyNative();
            var nativeDense = dense.WrapToReadOnlyNative();
            
            ContextOperationsHolder holder = ResolveHolder();

            var wrapper = holder.GetReadOnlyOperationsWrapper<T>();
            wrapper.Init(nativeSparse,
                nativeDense);

            return wrapper.Operations;
        }

        public LazyReadWriteNativeEntityOperations<T> GetLazyReadWriteOperations<T>(EcsSystems systems,
            Allocator operationAllocator = Allocator.TempJob, 
            int internalBuffersCapacity = 30) 
            where T : unmanaged
        {
            ContextOperationsHolder holder = ResolveHolder();

            var wrapper = holder.GetLazyReadWriteOperationsWrapper<T>();
            
            var internalData = GetReadWriteOperationsInternalData<T>(systems);
    
            wrapper.Init(internalData, operationAllocator, internalBuffersCapacity);

            return wrapper.Operations;
        }

        public ReadWriteNativeEntityOperations<T> GetReadWriteOperations<T>(EcsSystems systems,
            Allocator operationAllocator = Allocator.TempJob,
            int internalBuffersCapacity = 30)
            where T : unmanaged
        {
            ContextOperationsHolder holder = ResolveHolder();

            var wrapper = holder.GetReadWriteOperationsWrapper<T>();

            var internalData = GetReadWriteOperationsInternalData<T>(systems);
    
            wrapper.Init(internalData, operationAllocator, internalBuffersCapacity);

            return wrapper.Operations;
        }

        private ContextOperationsHolder ResolveHolder()
        {
            ContextOperationsHolder holder;
            
            if (_contextOperations.ContainsKey(_currentContext))
            {
                holder = _contextOperations[_currentContext];
            }
            else
            {
                holder = new ContextOperationsHolder();
                _contextOperations.Add(_currentContext, holder);
            }

            return holder;
        }

        private ReadWriteOperationsInternalData<T> GetReadWriteOperationsInternalData<T>(EcsSystems systems)
            where T : unmanaged
        {
            var world = systems.GetWorld();
            var pool = world.GetPool<T>();

            var sparse = pool.GetRawSparseItems();
            var dense = pool.GetRawDenseItems();
            var recycled = pool.GetRawRecycledItems();

            var entities = world.GetRawEntities();

            var nativeSparse = sparse.WrapToNative();
            var nativeDense = dense.WrapToNative();
            var nativeRecycled = recycled.WrapToNative();

            var nativeEntities = entities.WrapToNative();

            ref var recycledItemsCount = ref pool.GetRawRecycledItemsCount();
            ref var denseItemsCount = ref pool.GetRawDenseItemsCount();
            var poolId = pool.GetId();
            
            ReadWriteOperationsInternalData<T> internalData = default;
            
            internalData.Init(nativeSparse,
                              nativeDense, 
                              nativeRecycled, 
                              nativeEntities, 
                              ref recycledItemsCount, 
                              ref denseItemsCount, 
                              poolId);

            return internalData;
        }

        public void Dispose()
        {
            foreach (var pair in _contextOperations)
            {
                pair.Value.Dispose();
            }
        }
    }
}
