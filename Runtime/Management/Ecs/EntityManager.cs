namespace Blanketmen.Hypnos
{
    public class EntityManager
    {
        public static EntityManager Instance { get; private set; }

        public ulong GetNewEntity()
        {
            return IdGenerator.Get();
        }
    }
}