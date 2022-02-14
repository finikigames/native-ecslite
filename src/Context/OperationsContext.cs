using System;

namespace OdinGames.EcsLite.Native.Context
{
    public struct OperationsContext
    {
        private Guid _guid;

        public static OperationsContext Create()
        {
            return new OperationsContext
            {
                _guid = Guid.NewGuid()
            };
        }
    }
}