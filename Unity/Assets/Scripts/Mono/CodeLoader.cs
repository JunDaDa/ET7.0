﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace ET
{
	public class CodeLoader: IDisposable
	{
		public static CodeLoader Instance = new CodeLoader();

		public Action Update;
		public Action LateUpdate;
		public Action OnApplicationQuit;

		private Assembly assembly;

		public GlobalConfig GlobalConfig;

		private CodeLoader()
		{
		}

		public void Dispose()
		{
		}
		
		public void Start()
		{
			if (Define.EnableCodes)
			{
				this.GlobalConfig.LoadMode = LoadMode.Codes;
			}
			
			switch (this.GlobalConfig.LoadMode)
			{
				case LoadMode.Mono:
				{
					if (Define.EnableCodes)
					{
						throw new Exception("LoadMode.Mono must remove ENABLE_CODE define, please use ET/ChangeDefine/Remove ENABLE_CODE to Remove define");
					}
					
					Dictionary<string, UnityEngine.Object> dictionary = AssetsBundleHelper.LoadBundle("code.unity3d");
					byte[] assBytes = ((TextAsset)dictionary["Code.dll"]).bytes;
					byte[] pdbBytes = ((TextAsset)dictionary["Code.pdb"]).bytes;
					
					assembly = Assembly.Load(assBytes, pdbBytes);


					Dictionary<string, Type> types = AssemblyHelper.GetAssemblyTypes(typeof (Game).Assembly, this.assembly);
					Game.EventSystem.Add(types);
					
					IStaticMethod start = new StaticMethod(assembly, "ET.Client.Entry", "Start");
					start.Run();
					break;
				}
				case LoadMode.Reload:
				{
					if (Define.EnableCodes)
					{
						throw new Exception("LoadMode.Reload must remove ENABLE_CODE define, please use ET/ChangeDefine/Remove ENABLE_CODE to Remove define");
					}
					
					byte[] assBytes = File.ReadAllBytes(Path.Combine(Define.BuildOutputDir, "Model.dll"));
					byte[] pdbBytes = File.ReadAllBytes(Path.Combine(Define.BuildOutputDir, "Model.pdb"));
					
					assembly = Assembly.Load(assBytes, pdbBytes);
					this.LoadHotfix();
					IStaticMethod start = new StaticMethod(assembly, "ET.Client.Entry", "Start");
					start.Run();
					break;
				}
				case LoadMode.Codes:
				{
					if (!Define.EnableCodes)
					{
						throw new Exception("LoadMode.Codes must add ENABLE_CODE define, please use ET/ChangeDefine/Add ENABLE_CODE to add define");
					}

					Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
					Dictionary<string, Type> types = AssemblyHelper.GetAssemblyTypes(assemblies);
					Game.EventSystem.Add(types);
					foreach (Assembly ass in assemblies)
					{
						string name = ass.GetName().Name;
						if (name == "Unity.Codes")
						{
							this.assembly = ass;
						}
					}
					IStaticMethod start = new StaticMethod(assembly, "ET.Client.Entry", "Start");
					start.Run();
					break;
				}
			}
		}

		// 热重载调用下面两个方法
		// CodeLoader.Instance.LoadLogic();
		// Game.EventSystem.Load();
		public void LoadHotfix()
		{
			if (this.GlobalConfig.LoadMode != LoadMode.Reload)
			{
				throw new Exception("CodeMode != Reload!");
			}
			
			// 傻屌Unity在这里搞了个傻逼优化，认为同一个路径的dll，返回的程序集就一样。所以这里每次编译都要随机名字
			string[] logicFiles = Directory.GetFiles(Define.BuildOutputDir, "Hotfix_*.dll");
			if (logicFiles.Length != 1)
			{
				throw new Exception("Logic dll count != 1");
			}

			string logicName = Path.GetFileNameWithoutExtension(logicFiles[0]);
			byte[] assBytes = File.ReadAllBytes(Path.Combine(Define.BuildOutputDir, $"{logicName}.dll"));
			byte[] pdbBytes = File.ReadAllBytes(Path.Combine(Define.BuildOutputDir, $"{logicName}.pdb"));

			Assembly hotfixAssembly = Assembly.Load(assBytes, pdbBytes);
			
			Dictionary<string, Type> types = AssemblyHelper.GetAssemblyTypes(typeof (Game).Assembly, this.assembly, hotfixAssembly);
			
			Game.EventSystem.Add(types);
		}
	}
}