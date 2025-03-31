using NUnit.Framework;
using System;

namespace Blanketmen.Hypnos.Tests.Ecs
{
    public class EcsTest
    {
        [Test]
        public void EntityCreateTest()
        {
            //ComponentManager.CreateInstance();
            //SystemManager.CreateInstance();
            //EntityManager.CreateInstance();

            SystemManager.Instance.Init(new SystemConfig(
                updateSystems: new Type[] { typeof(TestSystem) }
            ));

            NodeManager nodeManager = new NodeManager();
            ComponentManager.Instance.OnComponentAdd += nodeManager.OnComponentAdd;
            ComponentManager.Instance.OnComponentRemove += nodeManager.OnComponentRemove;

            ulong testEntity = EntityManager.Instance.GetNewEntity();

            TestComponent testComponent;
            TestComponent2 testComponent2;

            testComponent = ComponentManager.Instance.AddComponent<TestComponent>(testEntity);
            SystemManager.Instance.Update();
            Assert.AreEqual(testComponent.i, 0);

            testComponent2 = ComponentManager.Instance.AddComponent<TestComponent2>(testEntity);
            SystemManager.Instance.Update();
            Assert.AreEqual(testComponent.i, 1);
            Assert.AreEqual(testComponent2.f, 1);

            SystemManager.Instance.Update();
            Assert.AreEqual(testComponent.i, 2);
            Assert.AreEqual(testComponent2.f, 2);
            ComponentManager.Instance.RemoveComponent<TestComponent>(testEntity);
            SystemManager.Instance.Update();
            Assert.AreEqual(testComponent.i, 0);
            Assert.AreEqual(testComponent2.f, 2);

            testComponent = ComponentManager.Instance.AddComponent<TestComponent>(testEntity);
            SystemManager.Instance.Update();
            Assert.AreEqual(testComponent.i, 1);
            Assert.AreEqual(testComponent2.f, 3);

            SystemManager.Instance.Update();
            Assert.AreEqual(testComponent.i, 2);
            Assert.AreEqual(testComponent2.f, 4);

            ComponentManager.Instance.RemoveComponent<TestComponent>(testEntity);
        }
    }
}