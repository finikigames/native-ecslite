using OdinGames.EcsLite.Native.NativeOperations;
using OdinGames.EcsLite.Native.NativeOperationsWrapper.Base;
using OdinGames.EcsLite.Native.WrappedData;

namespace OdinGames.EcsLite.Native.NativeOperationsWrapper
{
    public class NativeReadOnlyOperationsWrapper<T> : INativeOperationsWrapper<T> where T : unmanaged
    {
        public ReadOnlyNativeEntityOperations<T> Operations;

        public void Init(ReadOnlyNativeWrappedData<int> sparse,
                         ReadOnlyNativeWrappedData<T> dense)
        {
            Operations = default;

            Operations.Init(sparse,
                            dense);
        }

        public void Dispose()
        {
            Operations.Dispose();
        }
    }
}