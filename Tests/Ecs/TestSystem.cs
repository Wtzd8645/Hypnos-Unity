using Morpheus.Ecs;

namespace Morpheus.Tests.Ecs
{
    public class TestNode : EcsNode
    {
        public TestComponent TestComponent1 => GetComponent<TestComponent>();
        public TestComponent2 TestComponent2 => GetComponent<TestComponent2>();
        
        [OptionalComponent]
        public TestComponent3 TestComponent3 => GetComponent<TestComponent3>();
    }

    public class TestSystem : EcsSystem<TestNode>
    {
        protected override void Process(TestNode node)
        {
            node.TestComponent1.I += 1;
            node.TestComponent2.F += 1;

            if (node.TestComponent3 != null)
            {
                node.TestComponent3.U += 1;
            }
        }
    }
}