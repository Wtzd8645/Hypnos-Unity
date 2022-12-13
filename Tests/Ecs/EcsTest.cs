using Morpheus.Ecs;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Morpheus.Tests.Ecs
{
    public class EcsTest
    {
        private struct TestSystemConfig : ISystemConfig
        {
            public Type[] UpdateSystems {get;}
            public Type[] LateUpdateSystems {get;}
            public Type[] FixedUpdateSystems {get;}

            public TestSystemConfig(Type[] updateSystems = null, Type[] lateUpdateSystems = null, Type[] fixedUpdateSystems = null)
            {
                UpdateSystems = updateSystems;
                LateUpdateSystems = lateUpdateSystems;
                FixedUpdateSystems = fixedUpdateSystems;
            }
        }

        private void entityCreateTest()
        {
            IDGenerator.Reset();
            ComponentManager.CreateInstance();
            SystemManager.CreateInstance();
            EntityManager.CreateInstance();

            SystemManager.Instance.Initialize(new TestSystemConfig(
                updateSystems: new Type[] { typeof(TestSystem) }
            ));

            EntityConfig config = new(
                new ComponentConfig<TestComponent>((c)=>c.i = 1),
                new ComponentConfig<TestComponent3>()
            );

            List<TestComponent> testComponent1 = new();
            List<TestComponent2> testComponent2 = new();
            List<TestComponent3> testComponent3 = new();

            ComponentManager.Instance.OnComponentAdd += (c) => 
            {
                if (c is TestComponent t1)
                {
                    testComponent1.Add(t1);
                }
                else if (c is TestComponent2 t2)
                {
                    testComponent2.Add(t2);
                }
                else if (c is TestComponent3 t3)
                {
                    testComponent3.Add(t3);
                }
            };

            EntityManager.Instance.SpawnEntity(config);

            Assert.AreEqual(testComponent1[0].i, 1);
            Assert.AreEqual(testComponent3[0].u, 0);

            SystemManager.Instance.Update();
            
            Assert.AreEqual(testComponent1[0].i, 1);
            Assert.AreEqual(testComponent3[0].u, 0);

            ComponentManager.Instance.AddComponent<TestComponent2>(
                1,
                new ComponentConfig<TestComponent2>((c)=>c.f = 2)
            );

            SystemManager.Instance.Update();
            
            Assert.AreEqual(testComponent1[0].i, 2);
            Assert.AreEqual(testComponent2[0].f, 3);
            Assert.AreEqual(testComponent3[0].u, 1);

            config += new EntityConfig(
                new ComponentConfig<TestComponent2>()
            );
            config += new EntityConfig(
                new ComponentConfig<TestComponent2>((c)=>c.f = 2)
            );
            
            SystemManager.Instance.Update();
            
            Assert.AreEqual(testComponent1[0].i, 3);
            Assert.AreEqual(testComponent2[0].f, 4);
            Assert.AreEqual(testComponent3[0].u, 2);
            
            ComponentManager.Instance.RemoveComponent<TestComponent3>(1);
            
            SystemManager.Instance.Update();
            
            Assert.AreEqual(testComponent1[0].i, 4);
            Assert.AreEqual(testComponent2[0].f, 5);
            Assert.AreEqual(testComponent3[0].u, 0);
            
            EntityManager.Instance.SpawnEntity(config);
            
            SystemManager.Instance.Update();
            
            Assert.AreEqual(testComponent1[0].i, 5);
            Assert.AreEqual(testComponent2[0].f, 6);
            
            Assert.AreEqual(testComponent1[1].i, 2);
            Assert.AreEqual(testComponent2[1].f, 3);
            // 被拿來回收了
            Assert.AreEqual(testComponent3[1].u, 1);
            ComponentManager.Instance.RemoveComponent<TestComponent>(1);

            EntityManager.Instance.SpawnEntity(config);
            
            SystemManager.Instance.Update();

            Assert.AreEqual(testComponent2[0].f, 6);
            
            Assert.AreEqual(testComponent1[1].i, 3);
            Assert.AreEqual(testComponent2[1].f, 4);
            Assert.AreEqual(testComponent3[1].u, 2);

            Assert.AreEqual(testComponent1[2].i, 2);
            Assert.AreEqual(testComponent2[2].f, 3);
            Assert.AreEqual(testComponent3[2].u, 1);

            ComponentManager.Instance.RemoveComponent<TestComponent>(3);
            
            SystemManager.Instance.Update();

            Assert.AreEqual(testComponent2[0].f, 6);
            
            Assert.AreEqual(testComponent1[1].i, 4);
            Assert.AreEqual(testComponent2[1].f, 5);
            Assert.AreEqual(testComponent3[1].u, 3);
            
            Assert.AreEqual(testComponent1[2].i, 0);
            Assert.AreEqual(testComponent2[2].f, 3);
            Assert.AreEqual(testComponent3[2].u, 1);
        }

        [Test]
        public void EntityCreateTest()
        {
            entityCreateTest();
        }
    }
}