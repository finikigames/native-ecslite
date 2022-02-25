using System;
using System.Collections.Generic;
using Leopotam.EcsLite;
using OdinGames.EcsLite.Native.NativeOperations.FilterWrapper;
using OdinGames.EcsLite.Native.NativeOperationsWrapper;
using OdinGames.EcsLite.Native.NativeOperationsWrapper.Base;

namespace OdinGames.EcsLite.Native.Context
{
    public class ContextOperationsHolder : IDisposable
    {
        private readonly Dictionary<Type, INativeOperationsWrapperTypeless> _readOnlyOperations;

        private readonly Dictionary<Type, INativeReadWriteOperationsWrapper> _readWriteOperations;

        private readonly HashSet<INativeReadWriteOperationsWrapper> _usedReadWriteOperationsWrappers;
        private readonly HashSet<INativeOperationsWrapperTypeless> _usedReadOnlyOperationsWrappers;
        
        private readonly List<NativeFilterWrapper> _nativeFilters;

        public ContextOperationsHolder()
        {
            _readOnlyOperations = new Dictionary<Type, INativeOperationsWrapperTypeless>(20);
            _readWriteOperations = new Dictionary<Type, INativeReadWriteOperationsWrapper>(20);
            _usedReadWriteOperationsWrappers = new HashSet<INativeReadWriteOperationsWrapper>();
            _usedReadOnlyOperationsWrappers = new HashSet<INativeOperationsWrapperTypeless>();
            _nativeFilters = new List<NativeFilterWrapper>();
        }

        public void RegisterFilter(NativeFilterWrapper filterWrapper)
        {
#if UNITY_EDITOR
            _nativeFilters.Add(filterWrapper);
#endif
        }   
        
        public NativeReadOnlyOperationsWrapper<T> GetReadOnlyOperationsWrapper<T>()
            where T : unmanaged
        {
            var typeofT = typeof(T);
         
            NativeReadOnlyOperationsWrapper<T> wrapper;
            if (_readOnlyOperations.ContainsKey(typeofT))
            {
                wrapper = (NativeReadOnlyOperationsWrapper<T>) _readOnlyOperations[typeofT];
            }
            else
            {
                wrapper = new NativeReadOnlyOperationsWrapper<T>();
                _readOnlyOperations.Add(typeofT, wrapper);
            }
            
#if UNITY_EDITOR
            _usedReadOnlyOperationsWrappers.Add(wrapper);
#endif

            return wrapper;
        }
        
        public NativeLazyReadWriteOperationsWrapper<T> GetLazyReadWriteOperationsWrapper<T>()
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
            
            _usedReadWriteOperationsWrappers.Add(wrapper);

            return wrapper;
        }
        
        public NativeReadWriteOperationsWrapper<T> GetReadWriteOperationsWrapper<T>()
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
            
            _usedReadWriteOperationsWrappers.Add(wrapper);

            return wrapper;
        }

        public void ApplyContextOperations(EcsSystems systems)
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
            
            _usedReadOnlyOperationsWrappers.Clear();
            _nativeFilters.Clear();
#endif

            _usedReadWriteOperationsWrappers.Clear();
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
                world.OnEntityChangeInternal(entity, id, true);
#if DEBUG || LEOECSLITE_WORLD_EVENTS
                world.RaiseEntityChangeEvent(entity);
#endif
            }
        }

        private void ApplyRemoveComponents(EcsSystems systems, INativeReadWriteOperationsWrapper readWriteNativeEntityOperationsWrapper)
        {
            var world = systems.GetWorld();

            var id = readWriteNativeEntityOperationsWrapper.ID;
            foreach (var entity in readWriteNativeEntityOperationsWrapper.DeleteCache)
            {
                world.OnEntityChangeInternal(entity, id, false);
#if DEBUG || LEOECSLITE_WORLD_EVENTS
                world.RaiseEntityChangeEvent(entity);
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