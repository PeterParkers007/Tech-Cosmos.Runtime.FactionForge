#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using TechCosmos.FactionForge.Runtime;

namespace TechCosmos.FactionForge.Editor
{
    [CustomEditor(typeof(FactionManager))]
    public class FactionSystemEditor : UnityEditor.Editor
    {
        private FactionManager manager;
        private SerializedProperty factionsProperty;
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

            DrawEditModeSettings();
            DrawFactionsList();

            EditorGUILayout.Space(10);
            DrawRelationshipMatrix();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawEditModeSettings()
        {
            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.LabelField("编辑模式", EditorStyles.boldLabel);

                bool bidirectional = FactionEditorSettings.BidirectionalEditMode;
                bool newBidirectional = EditorGUILayout.ToggleLeft("双向绑定（修改一侧时自动同步另一侧）", bidirectional);
                if (newBidirectional != bidirectional)
                    FactionEditorSettings.BidirectionalEditMode = newBidirectional;

                if (newBidirectional)
                    EditorGUILayout.HelpBox("已启用双向绑定：设置 A→B 的关系时会同时设置 B→A。", MessageType.Info);
                else
                    EditorGUILayout.HelpBox("单向编辑：A→B 与 B→A 可分别设置，适合非对称关系设计。", MessageType.None);

                if (manager.HasDuplicateFactionNames())
                    EditorGUILayout.HelpBox("存在重复的阵营名称，关系编辑可能指向错误阵营，请先修正名称。", MessageType.Error);

                int asymmetricCount = CountAsymmetricRelationships();
                if (asymmetricCount > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.HelpBox($"检测到 {asymmetricCount} 对非对称关系。", MessageType.Warning);
                        if (GUILayout.Button("全部同步", GUILayout.Width(80), GUILayout.Height(38)))
                        {
                            Undo.RecordObject(manager, "Sync Faction Relationships");
                            int synced = manager.SyncAllRelationshipsBidirectional();
                            serializedObject.Update();
                            EditorUtility.SetDirty(manager);
                            Debug.Log($"已将 {synced} 对非对称关系同步为双向一致。");
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        private int CountAsymmetricRelationships()
        {
            if (manager == null || manager.factions.Count < 2)
                return 0;

            int count = 0;
            for (int i = 0; i < manager.factions.Count; i++)
            {
                for (int j = i + 1; j < manager.factions.Count; j++)
                {
                    if (!manager.AreRelationshipsSymmetric(
                            manager.factions[i].factionName,
                            manager.factions[j].factionName))
                        count++;
                }
            }

            return count;
        }

        private void DrawFactionsList()
        {
            EditorGUILayout.LabelField("阵营配置", EditorStyles.boldLabel);

            for (int i = 0; i < factionsProperty.arraySize; i++)
            {
                EditorGUILayout.BeginVertical("box");
                {
                    var factionProperty = factionsProperty.GetArrayElementAtIndex(i);
                    var nameProperty = factionProperty.FindPropertyRelative("factionName");

                    string factionName = string.IsNullOrEmpty(nameProperty.stringValue) ? "未命名阵营" : nameProperty.stringValue;
                    string foldoutKey = $"{factionName}_{i}";

                    if (!factionFoldouts.ContainsKey(foldoutKey))
                        factionFoldouts[foldoutKey] = true;

                    factionFoldouts[foldoutKey] = EditorGUILayout.Foldout(factionFoldouts[foldoutKey], $"阵营: {factionName}", true);

                    if (factionFoldouts[foldoutKey])
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(nameProperty, new GUIContent("阵营名称"));
                            if (EditorGUI.EndChangeCheck())
                                HandleFactionRename(i, nameProperty);

                            if (GUILayout.Button("删除", GUILayout.Width(60)))
                            {
                                Undo.RecordObject(manager, "Remove Faction");
                                factionsProperty.DeleteArrayElementAtIndex(i);
                                serializedObject.ApplyModifiedProperties();
                                manager.RefreshAllRelationships();
                                EditorUtility.SetDirty(manager);
                                serializedObject.Update();
                                return;
                            }
                        }
                        EditorGUILayout.EndHorizontal();

                        if (i < manager.factions.Count &&
                            !FactionManager.IsValidFactionName(manager.factions[i].factionName))
                        {
                            EditorGUILayout.HelpBox("请先填写阵营名称，再配置关系。", MessageType.Warning);
                        }

                        var relationshipsProperty = factionProperty.FindPropertyRelative("relationships");
                        EditorGUILayout.LabelField($"关系数量: {GetRelationshipCount(relationshipsProperty)}", EditorStyles.miniLabel);
                    }
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("+ 添加新阵营", GUILayout.Width(120)))
                {
                    Undo.RecordObject(manager, "Add Faction");
                    int newIndex = factionsProperty.arraySize;
                    factionsProperty.arraySize++;
                    var newFactionProperty = factionsProperty.GetArrayElementAtIndex(newIndex);
                    var newNameProperty = newFactionProperty.FindPropertyRelative("factionName");
                    newNameProperty.stringValue = FactionEditorUtility.GetUniqueFactionName(manager, "NewFaction");
                    serializedObject.ApplyModifiedProperties();
                    manager.RefreshAllRelationships();
                    EditorUtility.SetDirty(manager);
                    serializedObject.Update();
                }
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void HandleFactionRename(int index, SerializedProperty nameProperty)
        {
            if (index >= manager.factions.Count)
                return;

            string newName = nameProperty.stringValue?.Trim() ?? string.Empty;
            string oldName = manager.factions[index].factionName ?? string.Empty;

            if (newName == oldName)
                return;

            if (!FactionManager.IsValidFactionName(newName))
            {
                if (FactionManager.IsValidFactionName(oldName))
                    nameProperty.stringValue = oldName;
                return;
            }

            if (FactionEditorUtility.FactionNameExistsExcept(manager, newName, index))
            {
                Debug.LogWarning($"阵营名称 \"{newName}\" 已存在，请使用其他名称。");
                nameProperty.stringValue = oldName;
                return;
            }

            Undo.RecordObject(manager, "Rename Faction");

            if (FactionManager.IsValidFactionName(oldName))
            {
                if (!manager.RenameFaction(oldName, newName))
                    nameProperty.stringValue = oldName;
            }
            else
            {
                manager.factions[index].factionName = newName;
                manager.RefreshAllRelationships();
            }

            serializedObject.Update();
            EditorUtility.SetDirty(manager);
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

            for (int i = 0; i < manager.factions.Count; i++)
            {
                var currentFaction = manager.factions[i];

                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.LabelField($"{currentFaction.factionName} 对其他阵营的关系:", EditorStyles.boldLabel);

                    if (!FactionManager.IsValidFactionName(currentFaction.factionName))
                    {
                        EditorGUILayout.HelpBox("该阵营尚未命名，无法编辑关系。", MessageType.Info);
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.Space(5);
                        continue;
                    }

                    for (int j = 0; j < manager.factions.Count; j++)
                    {
                        if (i == j)
                            continue;

                        var otherFaction = manager.factions[j];

                        EditorGUILayout.BeginHorizontal();
                        {
                            if (!FactionManager.IsValidFactionName(otherFaction.factionName))
                            {
                                EditorGUILayout.LabelField("(未命名阵营)", GUILayout.Width(100));
                                EditorGUILayout.LabelField("-", EditorStyles.miniLabel);
                                EditorGUILayout.EndHorizontal();
                                continue;
                            }

                            var currentRelationship = manager.GetRelationship(
                                currentFaction.factionName,
                                otherFaction.factionName);

                            EditorGUILayout.LabelField(otherFaction.factionName, GUILayout.Width(100));

                            EditorGUI.BeginChangeCheck();
                            var newRelationship = (FactionRelationship)EditorGUILayout.EnumPopup(currentRelationship);
                            if (EditorGUI.EndChangeCheck() &&
                                FactionEditorUtility.TrySetRelationship(
                                    manager,
                                    currentFaction.factionName,
                                    otherFaction.factionName,
                                    newRelationship))
                            {
                                serializedObject.Update();
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
            var keysProperty = relationshipsProperty.FindPropertyRelative("keys");
            return keysProperty?.arraySize ?? 0;
        }
    }
}
#endif