using Morpheus.Ecs;

namespace Morpheus.Test.Ecs
{
    public class TestSystem : EcsSystem<TestNode>
    {
        protected override void Process(TestNode node)
        {
            node.testComponent1.i += 1;
            node.testComponent2.f += 1;
        }
    }
}