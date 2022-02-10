using System;
using Leopotam.EcsLite;
using OdinGames.EcsLite.Native.Extensions;
using OdinGames.EcsLite.Native.WrappedData;

namespace OdinGames.EcsLite.Native.NativeOperations.FilterWrapper
{
    public struct NativeFilterWrapper : IDisposable
    {
        private ReadOnlyNativeWrappedData<int> _filter;
        private int _length;
        
        public void Init(EcsFilter filter)
        {
            _filter = filter.GetRawEntities().WrapToReadOnlyNative();
            _length = filter.GetEntitiesCount();
        }

        public int this[int index]
            => _filter.Array[index];

        public int Count()
            => _length;
        
        public void Dispose()
        {
            _filter.UnwrapFromNative();
        }
    }
}
