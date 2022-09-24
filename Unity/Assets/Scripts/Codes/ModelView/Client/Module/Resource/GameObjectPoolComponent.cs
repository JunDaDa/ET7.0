using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ET.Client
{
    //public static class GoInfoSystem
    //{
    //    [ObjectSystem]
    //    public class GoInfoAwakeSystem : AwakeSystem<GoInfo, string, string, GameObject>
    //    {
    //        protected override void Awake(GoInfo self, string abName, string prefabName, GameObject go)
    //        {
    //            self.ABName = abName;
    //            self.PrefabName = abName;
    //            self.GameObject = go;
    //        }
    //    }

    //    [ObjectSystem]
    //    public class GoInfoDestroySystem : DestroySystem<GoInfo>
    //    {
    //        protected override void Destroy(GoInfo self)
    //        {
    //            self.ABName = "";
    //            self.PrefabName = "";
    //            UnityEngine.GameObject.Destroy(self.GameObject);
    //        }
    //    }
    //}

    //public class GoInfo : Entity, IAwake<string, string, GameObject>, IDestroy
    //{
    //    public GameObject GameObject;
    //    public string ABName;
    //    public string PrefabName;
    //}

    [FriendOf(typeof(GameObjectPoolComponent))]
    public static class GameObjectPoolComponentSystem
    {
        [ObjectSystem]
        public class GameObjectPoolComponentAwakeSystem : AwakeSystem<GameObjectPoolComponent>
        {
            protected override void Awake(GameObjectPoolComponent self)
            {
                // todo_sj
            }
        }

        [ObjectSystem]
        public class GameObjectPoolComponentDestroySystem : DestroySystem<GameObjectPoolComponent>
        {
            protected override void Destroy(GameObjectPoolComponent self)
            {
                // todo_sj
            }
        }
    }

    

    [ComponentOf]
    public class GameObjectPoolComponent : Entity, IAwake, IDestroy
    {
        public static GameObjectPoolComponent Instance { get; set; }

        // prefabName 对应的GameObject，todo_sj 后续AB打包的时候，检测所有的prefab不允许重名
        private Dictionary<string, Queue<GameObject>> goPoolDict = new Dictionary<string, Queue<GameObject>>(1024);

        private Dictionary<GameObject, string> allGoDict = new Dictionary<GameObject, string>(1024);

        private Dictionary<Entity, Dictionary<string, GameObject>> goUsedCache = new Dictionary<Entity, Dictionary<string, GameObject>>();

        private GameObject PopFromPool(string prefabName)
        {
            if (!goPoolDict.TryGetValue(prefabName, out var goPool))
            {
                return null;
            }

            if (goPool.Count <= 0)
                return null;

            return goPool.Dequeue();
        }

        private void PushToPool(string prefab, GameObject go)
        {
            if (!goPoolDict.TryGetValue(prefab, out var goPool))
            {
                goPool = new Queue<GameObject>(32);
                goPool.Enqueue(go);
                goPoolDict.Add(prefab, goPool);
                return;
            }

            goPool.Enqueue(go);
        }

        private void PushToPool(GameObject go)
        {
            if (allGoDict.TryGetValue(go, out var prefabName))
                PushToPool(prefabName, go);
            else
                Log.Error("GameObject不存在AllGoDict中！");
        }

        private void AddToAllGoDict(string prefabName, GameObject go)
        {
            if (allGoDict.TryGetValue(go, out var curPrefabName))
            {
                Log.Error("AddToAllGoDict 重复添加GameObject！" + prefabName + ", " + curPrefabName);
                return;
            }
            allGoDict.Add(go, prefabName);
        }

        private void RemoveFromAllGoDict(GameObject go)
        {
            if (allGoDict.TryGetValue(go, out var result))
            {
                allGoDict.Remove(go);
            }
            else
            {
                Log.Error("RemoveFromAllGoDict Fail！找不到go");
            }
        }

        #region 公共方法
        public async ETTask<GameObject> SpwanGo(string prefabName, Transform parent = null)
        {
            // todo_sj 分帧实例化降低CPU毛刺
            var scene = this.ClientScene();
            string abName = prefabName.PrefabNameToABName();
            var resLoadComponent = scene.GetComponent<ResourcesLoaderComponent>();

            await resLoadComponent.LoadAsync(abName);
            GameObject bundleGo = (GameObject)ResourcesComponent.Instance.GetAsset(abName, prefabName);
            var go = PopFromPool(prefabName);
            if (go == null)
            {
                go = GameObject.Instantiate(bundleGo, parent);
#if UNITY_EDITOR
                go.name = prefabName;
#endif
                AddToAllGoDict(prefabName, go);
            }
            else
            {
                if (parent != null && go.transform.parent != parent)
                {
                    go.transform.parent = parent;
                }
            }
                
            return go;
        }

        public async ETTask UnSpawnGo(GameObject go, bool isClean = false)
        {
            if (isClean)
            {
                RemoveFromAllGoDict(go);
                GameObject.DestroyImmediate(go, true);
            }
            else
            {
                PushToPool(go);
            }
            
            await ETTask.CompletedTask;
        }

        #endregion
    }
}
