namespace Blanketmen.Hypnos.Tests.Ecs
{
    public class TestComponent : EcsComponent
    {
        public int i;

        public override string ToString()
        {
            return $"{GetType()}: i:{i}";
        }

        public override void Dispose()
        {
            base.Dispose();
            i = 0;
        }
    }

    public class TestComponent2 : EcsComponent
    {
        public float f;

        public override string ToString()
        {
            return $"{GetType()}: f:{f}";
        }
    }
}