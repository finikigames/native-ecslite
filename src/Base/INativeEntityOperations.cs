namespace OdinGames.EcsLite.Native.Base
{
    public interface INativeEntityOperations<out T> : INativeEntityOperationsTypeless where T : unmanaged
    {
        T Get(int entity);
    }
}