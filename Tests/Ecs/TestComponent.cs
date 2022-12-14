using Morpheus.Ecs;

namespace Morpheus.Tests.Ecs
{
    public class TestComponent : EcsComponent
    {
        public int I = 0;

        public override string ToString()
        {
            return $"{GetType()}: i:{I}";
        }

        public override void Dispose()
        {
            base.Dispose();
            I = 0;
        }
    }

    public class TestComponent2 : EcsComponent
    {
        public float F = 0;

        public override string ToString()
        {
            return $"{GetType()}: f:{F}";
        }

        public override void Dispose()
        {
            base.Dispose();
            F = 0;
        }
    }

    public class TestComponent3 : EcsComponent
    {
        public ulong U = 0;

        public override string ToString()
        {
            return $"{GetType()}: u:{U}";
        }

        public override void Dispose()
        {
            base.Dispose();
            U = 0;
        }
    }
}