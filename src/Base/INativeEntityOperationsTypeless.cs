using System;

namespace OdinGames.EcsLite.Native.Base
{
    public interface INativeEntityOperationsTypeless : IDisposable
    {
        bool Has(int entity);
    }
}