using Hypnos.Ecs;

namespace Hypnos.Tests.Ecs
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