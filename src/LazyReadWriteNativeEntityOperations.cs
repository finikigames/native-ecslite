using System.Runtime.CompilerServices;
using OdinGames.EcsLite.Native.Base;
using OdinGames.EcsLite.Native.Extensions;
using OdinGames.EcsLite.Native.NativeOperations.ReadWriteOperationsData;
using UnityEngine;

namespace OdinGames.EcsLite.Native.NativeOperations
{
    public struct LazyReadWriteNativeEntityOperations<T> : ILazyReadWriteNativeEntityOperations<T> where T : unmanaged
    {
        private ReadWriteOperationsInternalData<T> _internalData;

        public void Init(ReadWriteOperationsInternalData<T> internalData)
        {
            _internalData = internalData;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(int entity)
        {
            return _internalData.SparseItems.Array[entity] > 0;
        }
        
        // Auto reset to default should be enough because in burst we only have unmanaged value types, which 
        // overwrites automatically
        public void Del(int entity)
        {
            ref var sparseData = ref _internalData.SparseItems.Array.GetRef(entity);
            if (sparseData > 0) 
            {
                _internalData.ComponentsToRemove.AddNoResize(entity);
                _internalData.RecycledItems.Array[_internalData.RecycledItemsCount.PostIncrement()] = sparseData;

                if (_internalData.RecycledItemsCount.Value == _internalData.RecycledItems.Array.Length) {
                    Debug.LogError("Try to increase RecycledItemsCount for pools in EcsWorld Config");
                }
                //if (_isAutoReset)
                {
                  //  _autoReset.Invoke(ref _denseItems.Array.GetRef(sparseData));
                }
               // else
                {
                    _internalData.DenseItems.Array[sparseData] = default;
                }

                sparseData = 0;
                ref var entityData = ref _internalData.Entities.Array.GetRef(entity);
                entityData.ComponentsCount--;
                
                if (entityData.ComponentsCount == 0) 
                {
                    _internalData.EntitiesToRemove.AddNoResize(entity);
                }
            }
        }

        public void DelEntity(int entity)
        {
            _internalData.EntitiesToRemove.AddNoResize(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get(int entity)
        {
            if (!Has(entity)) return Add(entity);
            var realIndex = _internalData.SparseItems.Array[entity];
            return _internalData.DenseItems.Array[realIndex];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetRef(int entity)
        {
            if (!Has(entity)) return ref Add(entity);
            var realIndex = _internalData.SparseItems.Array[entity];
            return ref _internalData.DenseItems.Array.GetRef(realIndex);
        }

        private ref T Add(int entity)
        {
            int idx;
            if (_internalData.RecycledItemsCount.Value > 0) 
            {
                idx = _internalData.RecycledItems.Array[_internalData.RecycledItemsCount.PreDecrement()];
            } 
            else 
            {
                idx = _internalData.DenseItemsCount.Value;
                if (_internalData.DenseItemsCount.Value == _internalData.DenseItems.Array.Length) 
                {
                    Debug.LogError("For now i have no idea is it possible to resize managed array treated as a NativeArray from job. Try to increase entities count in WorldConfig");
                }
                _internalData.DenseItemsCount++;

                //if (_isAutoReset)
                // _autoReset.Invoke(ref _denseItems.Array.GetRef(idx));
            }
            _internalData.SparseItems[entity] = idx;
            _internalData.ComponentsToAdd.AddNoResize(entity);
            _internalData.Entities[entity].ComponentsCount++;
            return ref _internalData.DenseItems.Array.GetRef(idx);
        }

        public void Dispose()
        {
            _internalData.SparseItems.UnwrapFromNative();
            _internalData.DenseItems.UnwrapFromNative();
            _internalData.RecycledItems.UnwrapFromNative();
            _internalData.Entities.UnwrapFromNative();
            
            _internalData.RecycledItemsCount.Dispose();
            _internalData.DenseItemsCount.Dispose();
        }
    }
}