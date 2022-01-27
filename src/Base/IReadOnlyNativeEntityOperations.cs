using OdinGames.EcsLite.Native.WrappedData;

namespace OdinGames.EcsLite.Native.Base
{
    public interface IReadOnlyNativeEntityOperations<T> : INativeEntityOperations<T> where T : unmanaged
    {
        void Init(ReadOnlyNativeWrappedData<int> sparseItems,
                  ReadOnlyNativeWrappedData<T> denseItems);
    }
}