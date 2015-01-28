using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEngine;
using UnityEditor;
namespace Editor.Serialization
{
	public class SerialTransformEncoder
	{
		public static void EncodeByXML(Transform Source, XmlDocument Doc, XmlNode Node)
		{
			if (null != Source)
			{
				
				XmlNode CurrentNode = Doc.CreateElement("Transform");
				if (null != Node)
				{
					Node.AppendChild(CurrentNode);
				}
				else
				{
					Doc.AppendChild(CurrentNode);
				}
				XmlAttribute Attr = Doc.CreateAttribute("posX");
				Attr.Value = Source.position.x + "";
				CurrentNode.Attributes.Append(Attr);

				Attr = Doc.CreateAttribute("posY");
				Attr.Value = Source.position.y + "";
				CurrentNode.Attributes.Append(Attr);

				Attr = Doc.CreateAttribute("posZ");
				Attr.Value = Source.position.z + "";
				CurrentNode.Attributes.Append(Attr);

				Attr = Doc.CreateAttribute("scaleX");
				Attr.Value = Source.localScale.x + "";
				CurrentNode.Attributes.Append(Attr);

				Attr = Doc.CreateAttribute("scaleY");
				Attr.Value = Source.localScale.y + "";
				CurrentNode.Attributes.Append(Attr);

				Attr = Doc.CreateAttribute("scaleZ");
				Attr.Value = Source.localScale.z + "";
				CurrentNode.Attributes.Append(Attr);

				Attr = Doc.CreateAttribute("rotationX");
				Attr.Value = Source.rotation.eulerAngles.x + "";
				CurrentNode.Attributes.Append(Attr);

				Attr = Doc.CreateAttribute("rotationY");
				Attr.Value = Source.rotation.eulerAngles.y + "";
				CurrentNode.Attributes.Append(Attr);

				Attr = Doc.CreateAttribute("rotationZ");
				Attr.Value = Source.rotation.eulerAngles.z + "";
				CurrentNode.Attributes.Append(Attr);

				Attr = Doc.CreateAttribute("name");
				Attr.Value = Source.name;
				CurrentNode.Attributes.Append(Attr);

				Attr = Doc.CreateAttribute("tag");
				Attr.Value = Source.tag;
				CurrentNode.Attributes.Append(Attr);

				if (EditorTools.IsPrefabIns(Source))
				{
					UnityEngine.Object Prototype = EditorTools.GetPrefabPrototype(Source);
					if (null != Prototype)
					{
						Attr = Doc.CreateAttribute("prefab");
						Attr.Value = AssetDatabase.GetAssetPath(Prototype);
						
						CurrentNode.Attributes.Append(Attr);
					}
				}
				/*
				Transform[] Children = Source.GetComponentsInChildren<Transform>();
				if (Children.Length > 0)
				{
					XmlNode ChildrenNode = Doc.CreateElement("Children");
					CurrentNode.AppendChild(ChildrenNode);
					for (int Index = 0; Index < Children.Length; Index++)
					{
						SerialTransform.EncodeByXML(Children[Index], Doc, ChildrenNode);
					}
				}
				*/
			}
		}
	}
}
