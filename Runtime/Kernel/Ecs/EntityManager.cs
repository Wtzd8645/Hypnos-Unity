namespace Blanketmen.Hypnos
{
    public class EntityManager : Singleton<EntityManager>
    {
        public ulong GetNewEntity()
        {
            return IDGenerator.Get();
        }
    }
}