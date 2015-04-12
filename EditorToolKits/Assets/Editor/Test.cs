using UnityEngine;
using System.Collections;
using UnityEditor;
using ToolKits.Window;
using ToolKits;
using ToolKits.Domain;

public class Test
{
    [MenuItem("Tools/Config")]
    static void show()
    {
        UISettingWindow.ShowWindow<UISettingWindow>("test");
    }
	
}
