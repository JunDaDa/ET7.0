using System;
using UnityEngine;

namespace ET.Client
{
	[UIEvent(UIType.UIHelp)]
    public class UIHelpEvent: AUIEvent
    {
        public override async ETTask<UI> OnCreate(UIComponent uiComponent, UILayer uiLayer)
        {
	        try
	        {
                //await uiComponent.Domain.GetComponent<ResourcesLoaderComponent>().LoadAsync(UIType.UIHelp.StringToAB());
                //GameObject bundleGameObject = (GameObject) ResourcesComponent.Instance.GetAsset(UIType.UIHelp.StringToAB(), UIType.UIHelp);
		        //GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject, UIEventComponent.Instance.GetLayer((int)uiLayer));

				string abName = UIType.UIHelp.StringToAB();
				string prefabName = UIType.UIHelp;

				var go = await GameObjectPoolComponent.Instance.SpwanGo(prefabName, UIEventComponent.Instance.GetLayer((int)uiLayer));
				UI ui = uiComponent.AddChild<UI, string, GameObject>(UIType.UIHelp, go);

				ui.AddComponent<UIHelpComponent>();
				return ui;
	        }
	        catch (Exception e)
	        {
				Log.Error(e);
		        return null;
	        }
		}

        public override void OnRemove(UIComponent uiComponent)
        {
        }
    }
}