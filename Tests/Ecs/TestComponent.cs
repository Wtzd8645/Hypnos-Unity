using Morpheus.Ecs;

namespace Morpheus.Tests.Ecs
{
    public class TestComponent : EcsComponent
    {
        public int i = 0;

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
        public float f = 0;

        public override string ToString()
        {
            return $"{GetType()}: f:{f}";
        }

        public override void Dispose()
        {
            base.Dispose();
            f = 0;
        }
    }

    public class TestComponent3 : EcsComponent
    {
        public ulong u = 0;

        public override string ToString()
        {
            return $"{GetType()}: u:{u}";
        }

        public override void Dispose()
        {
            base.Dispose();
            u = 0;
        }
    }
}