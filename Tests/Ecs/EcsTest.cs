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

        [Test]
        public  void EntityCreateTest()
        {
            IdGenerator.Reset();
            ComponentManager.CreateInstance();
            SystemManager.CreateInstance();
            EntityManager.CreateInstance();

            SystemManager.Instance.Initialize(new TestSystemConfig(
                updateSystems: new Type[] { typeof(TestSystem) }
            ));

            EntityConfig config = new(
                new ComponentConfig<TestComponent>((c)=>c.I = 1),
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

            Assert.AreEqual(testComponent1[0].I, 1);
            Assert.AreEqual(testComponent3[0].U, 0);

            SystemManager.Instance.Update();
            
            Assert.AreEqual(testComponent1[0].I, 1);
            Assert.AreEqual(testComponent3[0].U, 0);

            ComponentManager.Instance.AddComponent<TestComponent2>(
                1,
                new ComponentConfig<TestComponent2>((c)=>c.F = 2)
            );

            SystemManager.Instance.Update();
            
            Assert.AreEqual(testComponent1[0].I, 2);
            Assert.AreEqual(testComponent2[0].F, 3);
            Assert.AreEqual(testComponent3[0].U, 1);

            config += new EntityConfig(
                new ComponentConfig<TestComponent2>()
            );
            config += new EntityConfig(
                new ComponentConfig<TestComponent2>((c)=>c.F = 2)
            );
            
            SystemManager.Instance.Update();
            
            Assert.AreEqual(testComponent1[0].I, 3);
            Assert.AreEqual(testComponent2[0].F, 4);
            Assert.AreEqual(testComponent3[0].U, 2);
            
            ComponentManager.Instance.RemoveComponent<TestComponent3>(1);
            
            SystemManager.Instance.Update();
            
            Assert.AreEqual(testComponent1[0].I, 4);
            Assert.AreEqual(testComponent2[0].F, 5);
            Assert.AreEqual(testComponent3[0].U, 0);
            
            EntityManager.Instance.SpawnEntity(config);
            
            SystemManager.Instance.Update();
            
            Assert.AreEqual(testComponent1[0].I, 5);
            Assert.AreEqual(testComponent2[0].F, 6);
            
            Assert.AreEqual(testComponent1[1].I, 2);
            Assert.AreEqual(testComponent2[1].F, 3);
            // 被拿來回收了
            Assert.AreEqual(testComponent3[1].U, 1);
            ComponentManager.Instance.RemoveComponent<TestComponent>(1);

            EntityManager.Instance.SpawnEntity(config);
            
            SystemManager.Instance.Update();

            Assert.AreEqual(testComponent2[0].F, 6);
            
            Assert.AreEqual(testComponent1[1].I, 3);
            Assert.AreEqual(testComponent2[1].F, 4);
            Assert.AreEqual(testComponent3[1].U, 2);

            Assert.AreEqual(testComponent1[2].I, 2);
            Assert.AreEqual(testComponent2[2].F, 3);
            Assert.AreEqual(testComponent3[2].U, 1);

            ComponentManager.Instance.RemoveComponent<TestComponent>(3);
            
            SystemManager.Instance.Update();

            Assert.AreEqual(testComponent2[0].F, 6);
            
            Assert.AreEqual(testComponent1[1].I, 4);
            Assert.AreEqual(testComponent2[1].F, 5);
            Assert.AreEqual(testComponent3[1].U, 3);
            
            Assert.AreEqual(testComponent1[2].I, 0);
            Assert.AreEqual(testComponent2[2].F, 3);
            Assert.AreEqual(testComponent3[2].U, 1);
        }
    }
}