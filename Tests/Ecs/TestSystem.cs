using Morpheus.Ecs;

namespace Morpheus.Tests.Ecs
{
    public class TestNode : EcsNode
    {
        public TestComponent testComponent1 => GetComponent<TestComponent>();
        public TestComponent2 testComponent2 => GetComponent<TestComponent2>();
        
        [OptionalComponent]
        public TestComponent3 testComponent3 => GetComponent<TestComponent3>();
    }

    public class TestSystem : EcsSystem<TestNode>
    {
        protected override void Process(TestNode node)
        {
            node.testComponent1.i += 1;
            node.testComponent2.f += 1;

            if (node.testComponent3 != null)
            {
                node.testComponent3.u += 1;
            }
        }
    }
}