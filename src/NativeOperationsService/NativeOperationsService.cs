using System;
using System.Collections.Generic;
using Leopotam.EcsLite;
using OdinGames.EcsLite.Native.Extensions;
using OdinGames.EcsLite.Native.NativeOperations;
using OdinGames.EcsLite.Native.NativeOperations.FilterWrapper;
using OdinGames.EcsLite.Native.NativeOperations.ReadWriteOperationsData;
using OdinGames.EcsLite.Native.NativeOperationsService.Base;
using OdinGames.EcsLite.Native.NativeOperationsWrapper;
using OdinGames.EcsLite.Native.NativeOperationsWrapper.Base;
using Unity.Collections;

namespace OdinGames.EcsLite.Native.NativeOperationsService
{
    public class NativeOperationsService : INativeOperationsService
    {
        private readonly Dictionary<Type, INativeOperationsWrapperTypeless> _operations;

        private readonly Dictionary<Type, INativeReadWriteOperationsWrapper> _readWriteOperations;

        private readonly HashSet<INativeReadWriteOperationsWrapper> _usedReadWriteOperationsWrappers;
        private readonly HashSet<INativeOperationsWrapperTypeless> _usedReadOnlyOperationsWrappers;
        private readonly List<NativeFilterWrapper> _nativeFilters;

        [UnityEngine.Scripting.Preserve]
        public NativeOperationsService()
        {
            _operations = new Dictionary<Type, INativeOperationsWrapperTypeless>(20);
            _readWriteOperations = new Dictionary<Type, INativeReadWriteOperationsWrapper>(20);
            _usedReadWriteOperationsWrappers = new HashSet<INativeReadWriteOperationsWrapper>();
            _usedReadOnlyOperationsWrappers = new HashSet<INativeOperationsWrapperTypeless>();
            _nativeFilters = new List<NativeFilterWrapper>();
        }

        public void ApplyOperations(EcsSystems systems)
        {
            foreach (var operations in _usedReadWriteOperationsWrappers)
            {
                ApplyReadWriteOperations(systems, operations);

                operations.Dispose();
            }

#if UNITY_EDITOR
            foreach (var operationsPair in _usedReadOnlyOperationsWrappers)
            {
                operationsPair.Dispose();
            }

            foreach (var filter in _nativeFilters)
            {
                filter.Dispose();
            }

#endif

            _usedReadOnlyOperationsWrappers.Clear();
            _nativeFilters.Clear();
            _usedReadWriteOperationsWrappers.Clear();
        }

        public NativeFilterWrapper GetFilterWrapper(EcsFilter filter)
        {
            NativeFilterWrapper wrapper = default;
            ;

            wrapper.Init(filter);

#if UNITY_EDITOR
            _nativeFilters.Add(wrapper);
#endif

            return wrapper;
        }

        public ReadOnlyNativeEntityOperations<T> GetReadOnlyOperations<T>(EcsSystems systems)
            where T : unmanaged
        {
            var typeofT = typeof(T);

            var pool = systems.GetWorld().GetPool<T>();
            var sparse = pool.GetRawSparseItems();
            var dense = pool.GetRawDenseItems();

            var nativeSparse = sparse.WrapToReadOnlyNative();
            var nativeDense = dense.WrapToReadOnlyNative();

            NativeReadOnlyOperationsWrapper<T> wrapper;

            if (_operations.ContainsKey(typeofT))
            {
                wrapper = (NativeReadOnlyOperationsWrapper<T>) _operations[typeofT];
            }
            else
            {
                wrapper = new NativeReadOnlyOperationsWrapper<T>();
                _operations.Add(typeofT, wrapper);
            }

            if (!_usedReadOnlyOperationsWrappers.Contains(wrapper))
                _usedReadOnlyOperationsWrappers.Add(wrapper);

            wrapper.Init(nativeSparse,
                nativeDense);

            return wrapper.Operations;
        }

        public LazyReadWriteNativeEntityOperations<T> GetLazyReadWriteOperations<T>(EcsSystems systems,
            Allocator operationAllocator = Allocator.TempJob, 
            int internalBuffersCapacity = 30) 
            where T : unmanaged
        {
            var typeofT = typeof(T);

            NativeLazyReadWriteOperationsWrapper<T> wrapper;

            if (_readWriteOperations.ContainsKey(typeofT))
            {
                wrapper = (NativeLazyReadWriteOperationsWrapper<T>) _readWriteOperations[typeofT];
            }
            else
            {
                wrapper = new NativeLazyReadWriteOperationsWrapper<T>();
                _readWriteOperations.Add(typeofT, wrapper);
            }
            
            if (!_usedReadWriteOperationsWrappers.Contains(wrapper))
                _usedReadWriteOperationsWrappers.Add(wrapper);

            var internalData = GetReadWriteOperationsInternalData<T>(systems);
    
            wrapper.Init(internalData, operationAllocator, internalBuffersCapacity);

            return wrapper.Operations;
        }

        public ReadWriteNativeEntityOperations<T> GetReadWriteOperations<T>(EcsSystems systems,
            Allocator operationAllocator = Allocator.TempJob,
            int internalBuffersCapacity = 30)
            where T : unmanaged
        {
            var typeofT = typeof(T);

            NativeReadWriteOperationsWrapper<T> wrapper;

            if (_readWriteOperations.ContainsKey(typeofT))
            {
                wrapper = (NativeReadWriteOperationsWrapper<T>) _readWriteOperations[typeofT];
            }
            else
            {
                wrapper = new NativeReadWriteOperationsWrapper<T>();
                _readWriteOperations.Add(typeofT, wrapper);
            }
            
            if (!_usedReadWriteOperationsWrappers.Contains(wrapper))
                _usedReadWriteOperationsWrappers.Add(wrapper);

            var internalData = GetReadWriteOperationsInternalData<T>(systems);
    
            wrapper.Init(internalData, operationAllocator, internalBuffersCapacity);

            return wrapper.Operations;
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

        private void ApplyReadWriteOperations(EcsSystems systems, INativeReadWriteOperationsWrapper operationsWrapper)
        {
            ApplyAddComponents(systems, operationsWrapper);
            ApplyRemoveComponents(systems, operationsWrapper);
            ApplyRemoveEntities(systems, operationsWrapper);
        }

        private void ApplyAddComponents(EcsSystems systems, INativeReadWriteOperationsWrapper readWriteNativeEntityOperationsWrapper)
        {
            var world = systems.GetWorld();

            var id = readWriteNativeEntityOperationsWrapper.ID;
            foreach (var entity in readWriteNativeEntityOperationsWrapper.AddCache)
            {
                try
                {
                    world.OnEntityChange(entity, id, true);
#if UNITY_EDITOR
                    world.RaiseEntityChangeEvent(entity);
#endif
                }
                catch
                {
                    // ignored
                }
            }
        }

        private void ApplyRemoveComponents(EcsSystems systems, INativeReadWriteOperationsWrapper readWriteNativeEntityOperationsWrapper)
        {
            var world = systems.GetWorld();

            var id = readWriteNativeEntityOperationsWrapper.ID;
            foreach (var entity in readWriteNativeEntityOperationsWrapper.DeleteCache)
            {
                try
                {
                    world.OnEntityChange(entity, id, false);
#if UNITY_EDITOR
                    world.RaiseEntityChangeEvent(entity);
#endif
                }
                catch
                {
                    // ignored
                }
            }
        }

        private void ApplyRemoveEntities(EcsSystems systems, INativeReadWriteOperationsWrapper nativeReadWriteOperationsWrapper)
        {
            var world = systems.GetWorld();
            foreach (int entity in nativeReadWriteOperationsWrapper.EntitiesToRemove)
            {
                try
                {
                    world.DelEntity(entity);
                }
                catch
                {
                    // ignored
                }
            }
        }

        public void Dispose()
        {
            foreach (var wrapper in _usedReadOnlyOperationsWrappers)
            {
                 wrapper.Dispose();
            }
            
            foreach (var wrapper in _usedReadWriteOperationsWrappers)
            {
                 wrapper.Dispose();
            }
        }
    }
}
