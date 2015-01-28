using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EditorToolkit.Editor.Domain
{
	/**
	 * 模型导入配置
	 **/
	public class DomainModelSetting
	{
		public string PrefabName { set; get; }//要生成Prefab的名字
		public string ModelPath { set;get;}//模型路径
		public string MaterialPath{set;get;}//材质路径
		public string MaterialShaderPath { set;get;}//材质使用的Shader路径
		public string[] Animations { set; get; }//动画文件路径
		public string SplitCfgPath { set; get; }//动画切割配置路径
	}
}
