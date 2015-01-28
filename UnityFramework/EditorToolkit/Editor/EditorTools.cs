using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace Editor
{
	/**
	 * 编辑器模式使用的工具类
	 * 
	 **/
	public class EditorTools
	{
		/**
		 * 比较两个对象的Prefab是不是同一个
		 **/
		public static bool EqualsObjectPrefab(Object Target1,Object Target2)
		{
			if (PrefabUtility.GetPrefabType(Target1) == PrefabType.PrefabInstance &&
				PrefabUtility.GetPrefabType(Target2) == PrefabType.PrefabInstance)
			{
				return (AssetDatabase.GetAssetPath(PrefabUtility.GetPrefabParent(Target1)) == AssetDatabase.GetAssetPath(PrefabUtility.GetPrefabParent(Target2)));
			}
			
			return false;
		}

		/**
		 * 对象是否Prefab
		 **/
		public static bool IsPrefabIns(Object Target)
		{
			return (PrefabUtility.GetPrefabType(Target) == PrefabType.PrefabInstance);
		}

		/**
		 * 获取对象的Prefab原型
		 **/
		public static Object GetPrefabPrototype(Object Target)
		{
			if (PrefabUtility.GetPrefabType(Target) == PrefabType.PrefabInstance)
			{
				return PrefabUtility.GetPrefabParent(Target);
			}
			return null;
		}

		public static string GetFileSuffix(string Path)
		{
			return Path.Substring(Path.LastIndexOf(".") + 1);
		}
	}
}
