using Morpheus.Ecs;

namespace Morpheus.Test.Ecs
{
    public class TestNode : EcsNode
    {
        public TestComponent testComponent1 => GetComponent<TestComponent>();
        public TestComponent2 testComponent2 => GetComponent<TestComponent2>();
    }
}