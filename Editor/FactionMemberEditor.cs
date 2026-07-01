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

        private string[] factionNameArray = new string[0];
        private int selectedFactionIndex = 0;
        private bool showCustomInput = false;

        private void OnEnable()
        {
            factionMember = (FactionMember)target;
            factionNameProperty = serializedObject.FindProperty("factionName");
            canChangeFactionProperty = serializedObject.FindProperty("canChangeFaction");

            RefreshFactionList();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("阵营配置", EditorStyles.boldLabel);

            DrawSmartFactionSelector();

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(canChangeFactionProperty);

            EditorGUILayout.Space(10);

            DrawRelationshipPreview();

            EditorGUILayout.Space(10);

            DrawQuickActions();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSmartFactionSelector()
        {
            FactionManager manager = FindFactionManager();

            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.LabelField("阵营选择", EditorStyles.miniBoldLabel);

                if (manager == null || factionNameArray.Length == 0)
                {
                    EditorGUILayout.PropertyField(factionNameProperty, new GUIContent("阵营名称"));
                    EditorGUILayout.HelpBox("暂无可用阵营配置", MessageType.Info);
                }
                else
                {
                    RefreshSelectedIndex();

                    EditorGUILayout.BeginHorizontal();
                    {
                        int newIndex = EditorGUILayout.Popup("选择阵营", selectedFactionIndex, factionNameArray);

                        if (newIndex != selectedFactionIndex)
                        {
                            selectedFactionIndex = newIndex;
                            factionNameProperty.stringValue = factionNameArray[newIndex];
                        }

                        if (GUILayout.Button("刷新", GUILayout.Width(50)))
                        {
                            RefreshFactionList();
                            RefreshSelectedIndex();
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    {
                        showCustomInput = EditorGUILayout.Toggle("自定义输入", showCustomInput, GUILayout.Width(100));

                        if (showCustomInput)
                        {
                            EditorGUILayout.PropertyField(factionNameProperty, GUIContent.none);
                        }
                        else
                        {
                            EditorGUILayout.LabelField($"当前: {factionNameProperty.stringValue}", EditorStyles.miniLabel);
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.LabelField($"已配置 {factionNameArray.Length} 个阵营", EditorStyles.miniLabel);
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawRelationshipPreview()
        {
            EditorGUILayout.LabelField("阵营关系预览", EditorStyles.boldLabel);

            FactionManager manager = FindFactionManager();

            if (manager == null)
            {
                EditorGUILayout.HelpBox("场景中未找到 FactionManager", MessageType.Warning);

                if (GUILayout.Button("创建 FactionManager"))
                {
                    CreateFactionManager();
                    RefreshFactionList();
                }
                return;
            }

            if (string.IsNullOrEmpty(factionMember.FactionName))
            {
                EditorGUILayout.HelpBox("请先选择阵营", MessageType.Info);
                return;
            }

            EditorGUILayout.BeginVertical("box");
            {
                bool hasRelationships = false;

                foreach (var faction in manager.factions)
                {
                    if (faction.factionName == factionMember.FactionName) continue;

                    var relationship = manager.GetRelationship(factionMember.FactionName, faction.factionName);

                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField(faction.factionName, GUILayout.Width(80));

                        var style = new GUIStyle(EditorStyles.miniLabel);
                        style.normal.textColor = GetRelationshipColor(relationship);
                        style.alignment = TextAnchor.MiddleCenter;
                        style.padding = new RectOffset(4, 4, 2, 2);

                        EditorGUILayout.LabelField(relationship.ToString(), style, GUILayout.Width(60));

                        if (GUILayout.Button("...", GUILayout.Width(20)))
                        {
                            ShowRelationshipQuickMenu(faction.factionName, relationship);
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    hasRelationships = true;
                }

                if (!hasRelationships)
                {
                    EditorGUILayout.HelpBox("这是唯一的阵营，暂无其他关系", MessageType.Info);
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawQuickActions()
        {
            EditorGUILayout.LabelField("快速操作", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("测试关系"))
                    {
                        TestAllRelationships();
                    }

                    FactionManager manager = FindFactionManager();
                    if (manager != null && GUILayout.Button("打开管理器"))
                    {
                        Selection.activeObject = manager.gameObject;
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("刷新列表"))
                    {
                        RefreshFactionList();
                        RefreshSelectedIndex();
                    }

                    if (GUILayout.Button("创建新阵营"))
                    {
                        ShowCreateFactionDialog();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private void RefreshFactionList()
        {
            FactionManager manager = FindFactionManager();
            if (manager != null && manager.factions.Count > 0)
            {
                var names = new List<string>();
                foreach (var faction in manager.factions)
                {
                    if (!string.IsNullOrEmpty(faction.factionName))
                    {
                        names.Add(faction.factionName);
                    }
                }
                factionNameArray = names.ToArray();
            }
            else
            {
                factionNameArray = new string[0];
            }
        }

        private void RefreshSelectedIndex()
        {
            selectedFactionIndex = 0;
            for (int i = 0; i < factionNameArray.Length; i++)
            {
                if (factionNameArray[i] == factionNameProperty.stringValue)
                {
                    selectedFactionIndex = i;
                    break;
                }
            }
        }

        private void ShowRelationshipQuickMenu(string targetFaction, FactionRelationship currentRelationship)
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("设为敌对"), currentRelationship == FactionRelationship.Hostile,
                () => SetRelationship(targetFaction, FactionRelationship.Hostile));
            menu.AddItem(new GUIContent("设为中立"), currentRelationship == FactionRelationship.Neutral,
                () => SetRelationship(targetFaction, FactionRelationship.Neutral));
            menu.AddItem(new GUIContent("设为友好"), currentRelationship == FactionRelationship.Friendly,
                () => SetRelationship(targetFaction, FactionRelationship.Friendly));
            menu.AddItem(new GUIContent("设为同盟"), currentRelationship == FactionRelationship.Allied,
                () => SetRelationship(targetFaction, FactionRelationship.Allied));

            menu.ShowAsContext();
        }

        private void SetRelationship(string targetFaction, FactionRelationship relationship)
        {
            FactionManager manager = FindFactionManager();
            if (manager == null)
                return;

            if (!FactionEditorUtility.TrySetRelationship(
                    manager,
                    factionMember.FactionName,
                    targetFaction,
                    relationship))
            {
                Debug.LogWarning($"无法设置 {factionMember.FactionName} 与 {targetFaction} 的关系，请检查阵营名称是否有效。");
                return;
            }

            var syncHint = FactionEditorSettings.BidirectionalEditMode ? "（已同步双向）" : string.Empty;
            Debug.Log($"已设置 {factionMember.FactionName} 与 {targetFaction} 的关系为 {relationship}{syncHint}");
            Repaint();
        }

        private void ShowCreateFactionDialog()
        {
            FactionManager manager = FindFactionManager();
            if (manager == null)
            {
                manager = CreateFactionManager();
            }

            string newFactionName = EditorInputDialog.Show("新建阵营", "请输入新阵营名称:", "NewFaction");
            if (!string.IsNullOrEmpty(newFactionName))
            {
                bool exists = false;
                foreach (var faction in manager.factions)
                {
                    if (faction.factionName == newFactionName)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    Undo.RecordObject(manager, "Add Faction");
                    manager.factions.Add(new Faction { factionName = newFactionName });
                    manager.RefreshAllRelationships();
                    EditorUtility.SetDirty(manager);
                    RefreshFactionList();

                    factionNameProperty.stringValue = newFactionName;
                    RefreshSelectedIndex();

                    Debug.Log($"已创建新阵营: {newFactionName}");
                }
                else
                {
                    EditorUtility.DisplayDialog("错误", $"阵营 '{newFactionName}' 已存在!", "确定");
                }
            }
        }

        private class EditorInputDialog : EditorWindow
        {
            private string inputText = "";
            private string message;

            public static string Show(string title, string message, string defaultValue = "")
            {
                EditorInputDialog window = CreateInstance<EditorInputDialog>();
                window.titleContent = new GUIContent(title);
                window.message = message;
                window.inputText = defaultValue;
                window.minSize = new Vector2(300, 120);
                window.maxSize = new Vector2(300, 120);
                window.ShowModal();
                return window.inputText;
            }

            private void OnGUI()
            {
                GUILayout.Space(10);
                EditorGUILayout.LabelField(message);
                GUILayout.Space(10);

                inputText = EditorGUILayout.TextField(inputText);

                GUILayout.Space(20);
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("确定"))
                    {
                        Close();
                    }
                    if (GUILayout.Button("取消"))
                    {
                        inputText = "";
                        Close();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private FactionManager FindFactionManager()
        {
            if (FactionManager.Instance != null)
                return FactionManager.Instance;
            return FindObjectOfType<FactionManager>();
        }

        private FactionManager CreateFactionManager()
        {
            var managerObj = new GameObject("FactionManager");
            var manager = managerObj.AddComponent<FactionManager>();
            EditorUtility.SetDirty(managerObj);
            return manager;
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
                    var rel = manager.GetRelationship(factionMember.FactionName, faction.factionName);
                    Debug.Log($"{factionMember.FactionName} <-> {faction.factionName} : {rel}");
                }
            }
        }
    }
}
#endif