using System;
using Leopotam.EcsLite;
using OdinGames.EcsLite.Native.Extensions;
using OdinGames.EcsLite.Native.WrappedData;

namespace OdinGames.EcsLite.Native.NativeOperations.FilterWrapper
{
    public struct NativeFilterWrapper : IDisposable
    {
        private ReadOnlyNativeWrappedData<int> _filter;

        public void Init(EcsFilter filter)
        {
            _filter = filter.GetRawEntities().WrapToReadOnlyNative();
        }

        public int this[int index]
            => _filter.Array[index];

        public void Dispose()
        {
            _filter.UnwrapFromNative();
        }
    }
}