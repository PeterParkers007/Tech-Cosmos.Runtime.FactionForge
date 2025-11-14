#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using TechCosmos.FactionForge.Runtime;

namespace TechCosmos.FactionForge.Editor
{
    [CustomEditor(typeof(FactionMember))]
    public class FactionMemberEditor : UnityEditor.Editor
    {
        private FactionMember factionMember;
        private SerializedProperty factionNameProperty;
        private SerializedProperty canChangeFactionProperty;

        private void OnEnable()
        {
            factionMember = (FactionMember)target;
            factionNameProperty = serializedObject.FindProperty("factionName");
            canChangeFactionProperty = serializedObject.FindProperty("canChangeFaction");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // 1. 显示基础属性
            EditorGUILayout.PropertyField(factionNameProperty);
            EditorGUILayout.PropertyField(canChangeFactionProperty);

            EditorGUILayout.Space(10);

            // 2. 阵营关系预览
            DrawRelationshipPreview();

            EditorGUILayout.Space(10);

            // 3. 快速操作按钮
            DrawQuickActions();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawRelationshipPreview()
        {
            EditorGUILayout.LabelField("阵营关系预览", EditorStyles.boldLabel);

            // 在编辑器模式下查找 FactionManager
            FactionManager manager = FindFactionManager();

            if (manager == null)
            {
                EditorGUILayout.HelpBox("场景中未找到 FactionManager", MessageType.Warning);

                // 添加创建按钮
                if (GUILayout.Button("创建 FactionManager"))
                {
                    CreateFactionManager();
                }
                return;
            }

            if (string.IsNullOrEmpty(factionMember.FactionName))
            {
                EditorGUILayout.HelpBox("请先设置阵营名称", MessageType.Info);
                return;
            }

            // 显示与其他所有阵营的关系
            foreach (var faction in manager.factions)
            {
                if (faction.factionName == factionMember.FactionName) continue;

                var relationship = manager.GetRelationship(
                    factionMember.FactionName, faction.factionName);

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField(faction.factionName, GUILayout.Width(80));

                    var style = new GUIStyle(EditorStyles.label);
                    style.normal.textColor = GetRelationshipColor(relationship);
                    EditorGUILayout.LabelField(relationship.ToString(), style);
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawQuickActions()
        {
            EditorGUILayout.LabelField("快速操作", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            {
                // 快速测试按钮
                if (GUILayout.Button("测试关系"))
                {
                    TestAllRelationships();
                }

                // 在场景中定位FactionManager
                FactionManager manager = FindFactionManager();
                if (manager != null && GUILayout.Button("定位管理器"))
                {
                    Selection.activeObject = manager.gameObject;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        // 在编辑器模式下查找 FactionManager
        private FactionManager FindFactionManager()
        {
            // 方法1: 先尝试通过单例获取（运行时）
            if (FactionManager.Instance != null)
                return FactionManager.Instance;

            // 方法2: 在场景中查找（编辑时）
            return FindObjectOfType<FactionManager>();
        }

        // 创建 FactionManager
        private void CreateFactionManager()
        {
            var managerObj = new GameObject("FactionManager");
            managerObj.AddComponent<FactionManager>();
            EditorUtility.SetDirty(managerObj);
            Debug.Log("已创建 FactionManager");
        }

        private Color GetRelationshipColor(FactionRelationship relationship)
        {
            return relationship switch
            {
                FactionRelationship.Friendly => Color.green,
                FactionRelationship.Hostile => Color.red,
                FactionRelationship.Allied => Color.cyan,
                FactionRelationship.Neutral => Color.gray,
                _ => Color.white
            };
        }

        private void TestAllRelationships()
        {
            FactionManager manager = FindFactionManager();
            if (manager == null)
            {
                Debug.LogError("测试失败：场景中未找到 FactionManager");
                return;
            }

            Debug.Log($"=== {factionMember.name} 的阵营关系测试 ===");

            foreach (var faction in manager.factions)
            {
                if (faction.factionName != factionMember.FactionName)
                {
                    var rel = manager.GetRelationship(
                        factionMember.FactionName, faction.factionName);
                    Debug.Log($"{factionMember.FactionName} ←→ {faction.factionName} : {rel}");
                }
            }
        }
    }
}
#endif