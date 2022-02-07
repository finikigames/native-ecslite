namespace OdinGames.EcsLite.Native.Base
{
    public interface IReadWriteNativeEntityOperations<T> : ILazyReadWriteNativeEntityOperations<T> where T : unmanaged
    {
        void Add(int entity, T value);
        
        ref T Add(int entity);
    }
}