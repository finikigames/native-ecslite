using System;
using System.Collections.Generic;
using Leopotam.EcsLite;
using OdinGames.EcsLite.Native.Extensions;
using OdinGames.EcsLite.Native.NativeOperations;
using OdinGames.EcsLite.Native.NativeOperations.FilterWrapper;
using OdinGames.EcsLite.Native.NativeOperationsService.Base;
using OdinGames.EcsLite.Native.NativeOperationsWrapper;
using OdinGames.EcsLite.Native.NativeOperationsWrapper.Base;
using Unity.Collections;
using UnityEngine;

namespace OdinGames.EcsLite.Native.NativeOperationsService
{
    public class NativeOperationsService : INativeOperationsService
    {
        private readonly Dictionary<Type, INativeOperationsWrapperTypeless> _operations;

        private readonly Dictionary<Type, INativeReadWriteOperationsWrapper> _readWriteOperations;

        private readonly List<INativeReadWriteOperationsWrapper> _usedReadWriteOperationsWrappers;
        private readonly List<INativeOperationsWrapperTypeless> _usedReadOnlyOperationsWrappers;
        private readonly List<NativeFilterWrapper> _nativeFilters;

        public NativeOperationsService()
        {
            _operations = new Dictionary<Type, INativeOperationsWrapperTypeless>(20);
            _readWriteOperations = new Dictionary<Type, INativeReadWriteOperationsWrapper>(20);
            _usedReadWriteOperationsWrappers = new List<INativeReadWriteOperationsWrapper>(20);
#if UNITY_EDITOR
            _usedReadOnlyOperationsWrappers = new List<INativeOperationsWrapperTypeless>(20);
            _nativeFilters = new List<NativeFilterWrapper>();
#endif
        }
        
        public void ApplyOperations(EcsSystems systems)
        {
            foreach (var operations in _usedReadWriteOperationsWrappers)
            {
                try
                {
                    ApplyReadWriteOperations(systems, operations);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }

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
            _usedReadOnlyOperationsWrappers.Clear();
            _nativeFilters.Clear();
#endif
            
            _usedReadWriteOperationsWrappers.Clear();
        }

        public NativeFilterWrapper GetFilterWrapper(EcsFilter filter)
        {
            NativeFilterWrapper wrapper = default; ;
            
            wrapper.Init(filter);
            
            _nativeFilters.Add(wrapper);
            
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
                wrapper = (NativeReadOnlyOperationsWrapper<T>)_operations[typeofT];
            }
            else
            {
                wrapper = new NativeReadOnlyOperationsWrapper<T>();
                _operations.Add(typeofT, wrapper);
            }

            _usedReadOnlyOperationsWrappers.Add(wrapper);
            
            wrapper.Init(nativeSparse,
                nativeDense);

            return wrapper.Operations;
        }

        public ReadWriteNativeEntityOperations<T> GetReadWriteOperations<T>(EcsSystems systems, 
            Allocator operationAllocator = Allocator.TempJob) 
            where T : unmanaged
        {
            var typeofT = typeof(T);
            
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

            NativeReadWriteOperationsWrapper<T> wrapper;

            if (_readWriteOperations.ContainsKey(typeofT))
            {
                wrapper = (NativeReadWriteOperationsWrapper<T>)_readWriteOperations[typeofT];
            }
            else
            {
                wrapper = new NativeReadWriteOperationsWrapper<T>();
                _readWriteOperations.Add(typeofT, wrapper);
            }
            
            _usedReadWriteOperationsWrappers.Add(wrapper);
            
            wrapper.Init(nativeSparse,
                nativeDense, 
                nativeRecycled, 
                nativeEntities, 
                ref recycledItemsCount, 
                ref denseItemsCount, 
                poolId);

            return wrapper.Operations;
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

            foreach (var pair in readWriteNativeEntityOperationsWrapper.AddCache)
            {
                world.OnEntityChange(pair.Key, pair.Value, true);
#if UNITY_EDITOR            
                world.RaiseEntityChangeEvent(pair.Key);
#endif                
            }
        }

        private void ApplyRemoveComponents(EcsSystems systems, INativeReadWriteOperationsWrapper readWriteNativeEntityOperationsWrapper)
        {
            var world = systems.GetWorld();

            foreach (var pair in readWriteNativeEntityOperationsWrapper.DeleteCache)
            {
                world.OnEntityChange(pair.Key, pair.Value, false);
#if UNITY_EDITOR                
                world.RaiseEntityChangeEvent(pair.Key);
#endif                
            }
        }

        private void ApplyRemoveEntities(EcsSystems systems, INativeReadWriteOperationsWrapper nativeReadWriteOperationsWrapper)
        {
            var world = systems.GetWorld();
            foreach (int entity in nativeReadWriteOperationsWrapper.EntitiesToRemove)
            {
                world.DelEntity(entity);
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