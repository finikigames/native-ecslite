using System.Runtime.CompilerServices;
using Leopotam.EcsLite;
using OdinGames.EcsLite.Native.Base;
using OdinGames.EcsLite.Native.Extensions;
using OdinGames.EcsLite.Native.NativeOperationsWrapper;
using OdinGames.EcsLite.Native.WrappedData;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

namespace OdinGames.EcsLite.Native.NativeOperations
{
    public struct ReadWriteNativeEntityOperations<T> : IReadWriteNativeEntityOperations<T> where T : unmanaged
    {
        private NativeWrappedData<int> _sparseItems;
        private NativeWrappedData<T> _denseItems;
        private NativeWrappedData<int> _recycledItems;
        private NativeWrappedData<EntityData> _entities;

        private UnsafeExtensions.SharedIndex _recycledItemsCount;
        private UnsafeExtensions.SharedIndex _denseItemsCount;

        private NativeHashMap<int, int>.ParallelWriter _componentsToRemove;
        private NativeHashMap<int, int>.ParallelWriter _componentsToAdd;

        private NativeHashSet<int>.ParallelWriter _entitiesToRemove;

        private int _id;

        private bool _isAutoReset;
        private FunctionPointer<NativeReadWriteOperationsWrapper<T>.AutoResetDelegate> _autoReset;

        public void Init(NativeWrappedData<int> sparseItems,
                         NativeWrappedData<T> denseItems,
                         NativeWrappedData<int> recycledItems,
                         NativeWrappedData<EntityData> entities,
                         ref int recycledItemsCount,
                         ref int denseItemsCount,
                         NativeHashMap<int, int> componentsToRemove,
                         NativeHashMap<int, int> componentsToAdd,
                         NativeHashSet<int> entitiesToRemove,
                         int id,
                         bool isAutoReset = false,
                         FunctionPointer<NativeReadWriteOperationsWrapper<T>.AutoResetDelegate> autoReset = default)
        {
            _sparseItems = sparseItems;
            _denseItems = denseItems;
            _recycledItems = recycledItems;
            _entities = entities;

            _recycledItemsCount = new UnsafeExtensions.SharedIndex(ref recycledItemsCount);
            _denseItemsCount = new UnsafeExtensions.SharedIndex(ref denseItemsCount);

            _componentsToRemove = componentsToRemove.AsParallelWriter();
            _componentsToAdd = componentsToAdd.AsParallelWriter();
            _entitiesToRemove = entitiesToRemove.AsParallelWriter();
            _id = id;
            
            //_isAutoReset = isAutoReset;
            //_autoReset = autoReset;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(int entity)
        {
            return _sparseItems.Array[entity] > 0;
        }
        
        // Auto reset to default should be enough because in burst we only have unmanaged value types, which 
        // overwrites automatically
        public void Del(int entity)
        {
            ref var sparseData = ref _sparseItems.Array.GetRef(entity);
            if (sparseData > 0) 
            {
                _componentsToRemove.TryAdd(entity, _id);
                _recycledItems.Array[_recycledItemsCount.PostIncrement()] = sparseData;

                if (_recycledItemsCount.Value == _recycledItems.Array.Length) {
                    Debug.LogError("Try to increase RecycledItemsCount for pools in EcsWorld Config");
                }
                //if (_isAutoReset)
                {
                  //  _autoReset.Invoke(ref _denseItems.Array.GetRef(sparseData));
                }
               // else
                {
                    _denseItems.Array[sparseData] = default;
                }

                sparseData = 0;
                ref var entityData = ref _entities.Array.GetRef(entity);
                entityData.ComponentsCount--;
                
                if (entityData.ComponentsCount == 0) 
                {
                    _entitiesToRemove.Add(entity);
                }
            }
        }

        public void DelEntity(int entity)
        {
            _entitiesToRemove.Add(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get(int entity)
        {
            var realIndex = _sparseItems.Array[entity];
            return _denseItems.Array[realIndex];
        }

        public void Add(int entity, T value)
        {
            int idx;
            if (_recycledItemsCount.Value > 0)
            {
                idx = _recycledItems.Array[_recycledItemsCount.PreIncrement()];
            }
            else
            {
                idx = _denseItemsCount.Value;
                if (_denseItemsCount.Value == _denseItems.Array.Length) 
                {
                    Debug.LogError("For now i have no idea is it possible to resize managed array treated as a NativeArray from job. Try to increase entities count in WorldConfig");
                }
                
                _denseItemsCount++;

                _denseItems.Array[idx] = value;
            }

            _sparseItems[entity] = idx;
            _componentsToAdd.TryAdd(entity, _id);
            _entities[entity].ComponentsCount++;
        }
        
        public ref T Add(int entity)
        {
            int idx;
            if (_recycledItemsCount.Value > 0) 
            {
                idx = _recycledItems.Array[_recycledItemsCount.PreDecrement()];
            } 
            else 
            {
                idx = _denseItemsCount.Value;
                if (_denseItemsCount.Value == _denseItems.Array.Length) 
                {
                    Debug.LogError("For now i have no idea is it possible to resize managed array treated as a NativeArray from job. Try to increase entities count in WorldConfig");
                }
                _denseItemsCount++;

                //if (_isAutoReset)
                   // _autoReset.Invoke(ref _denseItems.Array.GetRef(idx));
            }
            _sparseItems[entity] = idx;
            _componentsToAdd.TryAdd(entity, _id);
            _entities[entity].ComponentsCount++;
            return ref _denseItems.Array.GetRef(idx);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetRef(int entity)
        {
            var realIndex = _sparseItems.Array[entity];
            return ref _denseItems.Array.GetRef(realIndex);
        }

        public void Dispose()
        {
            _sparseItems.UnwrapFromNative();
            _denseItems.UnwrapFromNative();
            _recycledItems.UnwrapFromNative();
            _entities.UnwrapFromNative();
            
            _recycledItemsCount.Dispose();
            _denseItemsCount.Dispose();
        }
    }
}