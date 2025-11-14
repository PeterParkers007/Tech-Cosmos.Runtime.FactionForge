#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using TechCosmos.FactionForge.Runtime;
using static UnityEngine.GraphicsBuffer;
namespace TechCosmos.FactionForge.Editor
{
    [CustomEditor(typeof(FactionManager))]
    public class FactionSystemEditor : UnityEditor.Editor
    {
        private FactionManager manager;
        private SerializedProperty factionsProperty;

        // 用于跟踪每个阵营的折叠状态
        private Dictionary<string, bool> factionFoldouts = new Dictionary<string, bool>();

        private void OnEnable()
        {
            manager = (FactionManager)target;
            factionsProperty = serializedObject.FindProperty("factions");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("阵营关系系统", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            DrawFactionsList();

            EditorGUILayout.Space(10);
            DrawRelationshipMatrix();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawFactionsList()
        {
            EditorGUILayout.LabelField("阵营配置", EditorStyles.boldLabel);

            // 阵营列表
            for (int i = 0; i < factionsProperty.arraySize; i++)
            {
                EditorGUILayout.BeginVertical("box");
                {
                    var factionProperty = factionsProperty.GetArrayElementAtIndex(i);
                    var nameProperty = factionProperty.FindPropertyRelative("factionName");

                    // 折叠标题
                    string factionName = string.IsNullOrEmpty(nameProperty.stringValue) ? "未命名阵营" : nameProperty.stringValue;
                    string foldoutKey = $"{factionName}_{i}";

                    if (!factionFoldouts.ContainsKey(foldoutKey))
                        factionFoldouts[foldoutKey] = true;

                    factionFoldouts[foldoutKey] = EditorGUILayout.Foldout(factionFoldouts[foldoutKey], $"阵营: {factionName}", true);

                    if (factionFoldouts[foldoutKey])
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            // 阵营名称输入
                            EditorGUILayout.PropertyField(nameProperty, new GUIContent("阵营名称"));

                            // 删除按钮
                            if (GUILayout.Button("删除", GUILayout.Width(60)))
                            {
                                factionsProperty.DeleteArrayElementAtIndex(i);
                                serializedObject.ApplyModifiedProperties();
                                manager.RefreshAllRelationships();
                                return;
                            }
                        }
                        EditorGUILayout.EndHorizontal();

                        // 显示关系数量
                        var relationshipsProperty = factionProperty.FindPropertyRelative("relationships");
                        EditorGUILayout.LabelField($"关系数量: {GetRelationshipCount(relationshipsProperty)}", EditorStyles.miniLabel);
                    }
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }

            // 添加新阵营按钮
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("+ 添加新阵营", GUILayout.Width(120)))
                {
                    factionsProperty.arraySize++;
                    serializedObject.ApplyModifiedProperties();
                    manager.RefreshAllRelationships();
                }
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawRelationshipMatrix()
        {
            if (manager == null || manager.factions.Count < 2)
            {
                EditorGUILayout.HelpBox("至少需要2个阵营才能显示关系配置", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField("阵营关系矩阵", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // 绘制关系矩阵
            for (int i = 0; i < manager.factions.Count; i++)
            {
                var currentFaction = manager.factions[i];

                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.LabelField($"{currentFaction.factionName} 对其他阵营的关系:", EditorStyles.boldLabel);

                    for (int j = 0; j < manager.factions.Count; j++)
                    {
                        if (i == j) continue; // 跳过自己

                        var otherFaction = manager.factions[j];
                        var currentRelationship = currentFaction.relationships.ContainsKey(otherFaction.factionName)
                            ? currentFaction.relationships[otherFaction.factionName]
                            : FactionRelationship.Neutral;

                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField(otherFaction.factionName, GUILayout.Width(100));

                            var newRelationship = (FactionRelationship)EditorGUILayout.EnumPopup(currentRelationship);

                            if (newRelationship != currentRelationship)
                            {
                                currentFaction.relationships[otherFaction.factionName] = newRelationship;
                                EditorUtility.SetDirty(manager);
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }
        }

        private int GetRelationshipCount(SerializedProperty relationshipsProperty)
        {
            // 获取序列化字典中的关系数量
            var keysProperty = relationshipsProperty.FindPropertyRelative("keys");
            return keysProperty?.arraySize ?? 0;
        }
    }
}
#endif