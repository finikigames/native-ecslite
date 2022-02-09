using System;
using Unity.Collections;

namespace OdinGames.EcsLite.Native.NativeOperationsWrapper.Base
{
    public interface INativeReadWriteOperationsWrapper : IDisposable
    {
        int ID { get; }
        NativeList<int> DeleteCache { get; }
        NativeList<int> AddCache { get; }
        NativeList<int> EntitiesToRemove { get; }
    }
}