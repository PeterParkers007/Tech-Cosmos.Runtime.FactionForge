#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TechCosmos.FactionForge.Runtime;

namespace TechCosmos.FactionForge.Editor
{
    public class FactionRelationshipWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private bool autoRefresh = true;
        private float lastRefreshTime;
        private const float REFRESH_INTERVAL = 1.0f;

        [MenuItem("Tech-Cosmos/阵营关系可视化窗口")]
        public static void ShowWindow()
        {
            var window = GetWindow<FactionRelationshipWindow>("阵营关系");
            window.minSize = new Vector2(400, 500);
            window.Show();
        }

        private void OnEnable()
        {
            RefreshData();
        }

        private void OnGUI()
        {
            DrawToolbar();

            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            {
                DrawRelationshipMatrix();
                DrawStatistics();
            }
            EditorGUILayout.EndScrollView();

            if (autoRefresh && EditorApplication.timeSinceStartup - lastRefreshTime > REFRESH_INTERVAL)
            {
                RefreshData();
                Repaint();
            }
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                EditorGUILayout.LabelField("阵营关系总览", EditorStyles.boldLabel);

                GUILayout.FlexibleSpace();

                autoRefresh = GUILayout.Toggle(autoRefresh, "自动刷新", EditorStyles.toolbarButton);

                bool bidirectional = FactionEditorSettings.BidirectionalEditMode;
                bool newBidirectional = GUILayout.Toggle(bidirectional, "双向绑定", EditorStyles.toolbarButton);
                if (newBidirectional != bidirectional)
                    FactionEditorSettings.BidirectionalEditMode = newBidirectional;

                if (GUILayout.Button("刷新", EditorStyles.toolbarButton))
                    RefreshData();

                if (GUILayout.Button("打开管理器", EditorStyles.toolbarButton))
                    Selection.activeObject = FindFactionManager()?.gameObject;

                if (GUILayout.Button("创建管理器", EditorStyles.toolbarButton))
                {
                    CreateFactionManager();
                    RefreshData();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawRelationshipMatrix()
        {
            var manager = FindFactionManager();

            if (manager == null || manager.factions.Count == 0)
            {
                EditorGUILayout.HelpBox("未找到阵营数据\n请先创建 FactionManager 并添加阵营", MessageType.Info);
                return;
            }

            if (manager.HasDuplicateFactionNames())
                EditorGUILayout.HelpBox("存在重复的阵营名称，请先修正后再编辑关系。", MessageType.Error);

            EditorGUILayout.LabelField("关系矩阵", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("阵营", EditorStyles.boldLabel, GUILayout.Width(120));
                foreach (var faction in manager.factions)
                    EditorGUILayout.LabelField(faction.factionName, EditorStyles.boldLabel, GUILayout.Width(80));
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            for (int i = 0; i < manager.factions.Count; i++)
            {
                var currentFaction = manager.factions[i];

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField(currentFaction.factionName, GUILayout.Width(120));

                    for (int j = 0; j < manager.factions.Count; j++)
                    {
                        if (i == j)
                        {
                            EditorGUILayout.LabelField("自身", GetCellStyle(FactionRelationship.Friendly), GUILayout.Width(80));
                            continue;
                        }

                        var otherFaction = manager.factions[j];
                        if (!manager.CanConfigureRelationship(currentFaction.factionName, otherFaction.factionName))
                        {
                            EditorGUILayout.LabelField("-", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(80));
                            continue;
                        }

                        var relationship = manager.GetRelationship(currentFaction.factionName, otherFaction.factionName);
                        var reverseRelationship = manager.GetRelationship(otherFaction.factionName, currentFaction.factionName);
                        bool isAsymmetric = relationship != reverseRelationship;

                        var cellStyle = GetCellStyle(relationship);
                        if (isAsymmetric && !FactionEditorSettings.BidirectionalEditMode)
                            cellStyle.fontStyle = FontStyle.Bold;

                        string label = isAsymmetric && !FactionEditorSettings.BidirectionalEditMode
                            ? $"{relationship}≠"
                            : relationship.ToString();

                        if (GUILayout.Button(label, cellStyle, GUILayout.Width(80)))
                        {
                            ShowRelationshipQuickMenu(
                                currentFaction.factionName,
                                otherFaction.factionName,
                                relationship);
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (i < manager.factions.Count - 1)
                    EditorGUILayout.Space(2);
            }
        }

        private void DrawStatistics()
        {
            var manager = FindFactionManager();
            if (manager == null || manager.factions.Count == 0)
                return;

            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("统计信息", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.LabelField($"总阵营数量: {manager.factions.Count}");

                int hostileCount = 0, friendlyCount = 0, alliedCount = 0, neutralCount = 0;

                for (int i = 0; i < manager.factions.Count; i++)
                {
                    for (int j = i + 1; j < manager.factions.Count; j++)
                    {
                        var nameA = manager.factions[i].factionName;
                        var nameB = manager.factions[j].factionName;
                        if (!manager.CanConfigureRelationship(nameA, nameB))
                            continue;

                        var rel = manager.GetRelationship(nameA, nameB);

                        switch (rel)
                        {
                            case FactionRelationship.Hostile: hostileCount++; break;
                            case FactionRelationship.Friendly: friendlyCount++; break;
                            case FactionRelationship.Allied: alliedCount++; break;
                            case FactionRelationship.Neutral: neutralCount++; break;
                        }
                    }
                }

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField($"敌对关系: {hostileCount}", GetMiniLabelStyle(Color.red));
                    EditorGUILayout.LabelField($"友好关系: {friendlyCount}", GetMiniLabelStyle(Color.green));
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField($"同盟关系: {alliedCount}", GetMiniLabelStyle(Color.cyan));
                    EditorGUILayout.LabelField($"中立关系: {neutralCount}", GetMiniLabelStyle(Color.gray));
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private GUIStyle GetCellStyle(FactionRelationship relationship)
        {
            var style = new GUIStyle(EditorStyles.miniButton);

            switch (relationship)
            {
                case FactionRelationship.Hostile:
                    style.normal.textColor = Color.red;
                    break;
                case FactionRelationship.Friendly:
                    style.normal.textColor = Color.green;
                    break;
                case FactionRelationship.Allied:
                    style.normal.textColor = Color.cyan;
                    break;
                case FactionRelationship.Neutral:
                    style.normal.textColor = Color.gray;
                    break;
            }

            return style;
        }

        private GUIStyle GetMiniLabelStyle(Color color)
        {
            var style = new GUIStyle(EditorStyles.miniLabel);
            style.normal.textColor = color;
            return style;
        }

        private void ShowRelationshipQuickMenu(string factionA, string factionB, FactionRelationship currentRelationship)
        {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("设为敌对"), currentRelationship == FactionRelationship.Hostile,
                () => SetRelationship(factionA, factionB, FactionRelationship.Hostile));
            menu.AddItem(new GUIContent("设为中立"), currentRelationship == FactionRelationship.Neutral,
                () => SetRelationship(factionA, factionB, FactionRelationship.Neutral));
            menu.AddItem(new GUIContent("设为友好"), currentRelationship == FactionRelationship.Friendly,
                () => SetRelationship(factionA, factionB, FactionRelationship.Friendly));
            menu.AddItem(new GUIContent("设为同盟"), currentRelationship == FactionRelationship.Allied,
                () => SetRelationship(factionA, factionB, FactionRelationship.Allied));

            menu.ShowAsContext();
        }

        private void SetRelationship(string factionA, string factionB, FactionRelationship relationship)
        {
            var manager = FindFactionManager();
            if (manager == null)
                return;

            if (!FactionEditorUtility.TrySetRelationship(manager, factionA, factionB, relationship))
            {
                Debug.LogWarning($"无法设置 {factionA} 与 {factionB} 的关系，请检查阵营名称是否有效。");
                return;
            }

            RefreshData();
            Repaint();

            var syncHint = FactionEditorSettings.BidirectionalEditMode ? "（已同步双向）" : string.Empty;
            Debug.Log($"已设置 {factionA} 与 {factionB} 的关系为 {relationship}{syncHint}");
        }

        private void RefreshData()
        {
            lastRefreshTime = (float)EditorApplication.timeSinceStartup;
        }

        private FactionManager FindFactionManager()
        {
            if (FactionManager.Instance != null)
                return FactionManager.Instance;

            return FindObjectOfType<FactionManager>();
        }

        private void CreateFactionManager()
        {
            var managerObj = new GameObject("FactionManager");
            managerObj.AddComponent<FactionManager>();
            EditorUtility.SetDirty(managerObj);
            Debug.Log("已创建 FactionManager");
        }

        [MenuItem("GameObject/Tools/FactionForge/创建阵营管理器", false, 0)]
        static void CreateFactionManagerMenu()
        {
            var managerObj = new GameObject("FactionManager");
            managerObj.AddComponent<FactionManager>();
            Selection.activeObject = managerObj;
        }
    }
}
#endif