using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Assets.ThirdParty.Behavior_Designer.Editor
{
    public static class BehaviorTreeExporter
    {
        static string luaAIPath = Application.dataPath + "lua/game/actor/BehaviorTree/LuaBehaviorTree/LogicTree";
        [MenuItem("Tools/Behavior Designer/ExportToLua")]
        static void ExportToLua()
        {
            EditorApplication.ExecuteMenuItem("File/Save Project");
            string path = "";
            var obj = Selection.activeObject;
            if (obj != null)
            {
                path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(obj.GetInstanceID()));
            }
            UnityEngine.Object[] arr = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.TopLevel);
            foreach (var o in arr)
            {
                var gameObject = o as GameObject;
                var behaviorTree = gameObject.GetComponent<BehaviorTree>();
                var exportPath = $"{luaAIPath}/{gameObject.name}.lua";
                StreamWriter sw = new StreamWriter(exportPath);
                BehaviorTreeToLua(behaviorTree, sw);
                sw.Close();
                Debug.LogWarning(gameObject.name + " Export to " + exportPath);
            }
        }

        static void BehaviorTreeToLua(BehaviorTree behaviorTree, StreamWriter sw)
        {
            sw.WriteLine($@"local Tree = BehTree.TaskRoot:New()");

            var root = behaviorTree.GetBehaviorSource().RootTask;
            Dictionary<Task, List<Task>> dic = new Dictionary<Task, List<Task>>();
            Traverse(dic, root);

            var allTasks = new List<Task>() { root };
            dic.Values.ToList().ForEach(list => allTasks.AddRange(list));
            allTasks = allTasks.Distinct().ToList();

            sw.WriteLine();

            foreach (var task in allTasks)
            {
                sw.WriteLine($"local {task.LuaName()} = BehTree.{task.LuaTypeName()}:New()");

                foreach (var property in task.GetType().GetProperties())
                {
                    var attributes = property.GetCustomAttributes(true);
                    foreach (var attr in attributes)
                    {
                        if (attr is ToLuaAttribute toLua)
                        {
                            var conversionType = property.PropertyType;
                            var value = property.GetValue(task);
                            sw.WriteLine($"{task.LuaName()}.{toLua.luaName} = {ParseToLua(toLua, conversionType, value)}");
                        }
                    }
                }
                foreach (var fileld in task.GetType().GetFields())
                {
                    var attributes = fileld.GetCustomAttributes(true);
                    foreach (var attr in attributes)
                    {
                        if (attr is ToLuaAttribute toLua)
                        {
                            var conversionType = fileld.FieldType;
                            var value = fileld.GetValue(task);
                            sw.WriteLine($"{task.LuaName()}.{toLua.luaName} = {ParseToLua(toLua, conversionType, value)}");
                        }
                    }
                }

                sw.WriteLine();
            }

            sw.WriteLine("----- build tree -----");

            sw.WriteLine($"Tree:PushTask({root.LuaName()})");
            sw.WriteLine();

            foreach (var pair in dic)
            {
                foreach (var child in pair.Value)
                {
                    sw.WriteLine($"{pair.Key.LuaName()}:AddChild({child.LuaName()})");
                }
                sw.WriteLine();
            }

            sw.WriteLine($"return Tree");

        }

        static void Traverse(Dictionary<Task, List<Task>> dic, Task task)
        {
            var parent = task as ParentTask;
            if (parent != null)
            {
                dic.Add(parent, parent.Children);
                foreach (var child in parent.Children)
                {
                    Traverse(dic, child);
                }
            }
        }

        public static string LuaName(this Task task)
        {
            return (task.FriendlyName + task.ID).Replace(" ", "");
        }

        public static string LuaTypeName(this Task task)
        {
            return task.FriendlyName.Replace(" ", "");
        }

        static string ParseToLua(ToLuaAttribute attr, Type conversionType, object value)
        {
            switch (attr.type)
            {
                case ToLuaType.Normal:
                    {
                        switch (value)
                        {
                            case Vector2 vec:
                                return $"{{x = {vec.x}, y = {vec.y}}}";
                            case Vector3 vec:
                                return $"{{x = {vec.x}, y = {vec.y}, z = {vec.z}}}";
                            case string str:
                                return '"' + str + '"';
                        }
                    }
                    return value.ToString();
                case ToLuaType.Function:
                    //todo
                    return value.ToString();
                default:
                    return value.ToString();
            }
        }
    }
}
