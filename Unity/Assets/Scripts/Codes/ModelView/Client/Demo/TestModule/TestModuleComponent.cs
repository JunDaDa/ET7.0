using System;
using System.Collections.Generic;

namespace ET.Client
{
    [ObjectSystem]
    public class TestModuleComponentAwakeSystem : AwakeSystem<TestModuleComponent>
    {
        protected override void Awake(TestModuleComponent self)
        {
            Log.Info("Awake TestModule");
        }
    }

    [ObjectSystem]
    public class TestModuleComponentDestroySystem : DestroySystem<TestModuleComponent>
    {
        protected override void Destroy(TestModuleComponent self)
        {
            Log.Info("Destroy TestModule");
        }
    }

    [ComponentOf]
    public class TestModuleComponent : Entity, IAwake, IDestroy
    {

    }
}
