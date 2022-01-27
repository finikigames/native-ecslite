using OdinGames.EcsLite.Native.Base;
using OdinGames.EcsLite.Native.Extensions;
using OdinGames.EcsLite.Native.WrappedData;

namespace OdinGames.EcsLite.Native.NativeOperations
{
    // Use this if you don't want to add/remove components from entity dynamically
    public struct ReadOnlyNativeEntityOperations<T> : IReadOnlyNativeEntityOperations<T> where T : unmanaged
    {
        private ReadOnlyNativeWrappedData<int> _sparseItems;
        private ReadOnlyNativeWrappedData<T> _denseItems;

        public void Init(ReadOnlyNativeWrappedData<int> sparseItems,
                         ReadOnlyNativeWrappedData<T> denseItems)
        {
            _sparseItems = sparseItems;
            _denseItems = denseItems;
        }

        public bool Has(int entity)
        {
            return _sparseItems.Array[entity] > 0;
        }

        public T Get(int entity)
        {
            var realIndex = _sparseItems.Array[entity];
            return _denseItems.Array[realIndex];
        }

        public void Dispose()
        {
            _sparseItems.UnwrapFromNative();
            _denseItems.UnwrapFromNative();
        }
    }
}