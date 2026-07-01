#if UNITY_EDITOR
using TechCosmos.FactionForge.Runtime;
using UnityEditor;
using UnityEngine;

namespace TechCosmos.FactionForge.Editor
{
    internal static class FactionEditorUtility
    {
        public static bool TrySetRelationship(
            FactionManager manager,
            string factionA,
            string factionB,
            FactionRelationship relationship)
        {
            if (manager == null || !manager.CanConfigureRelationship(factionA, factionB))
                return false;

            Undo.RecordObject(manager, "Set Faction Relationship");
            manager.SetRelationship(
                factionA,
                factionB,
                relationship,
                FactionEditorSettings.BidirectionalEditMode);
            EditorUtility.SetDirty(manager);
            return true;
        }

        public static string GetUniqueFactionName(FactionManager manager, string baseName)
        {
            if (manager == null)
                return baseName;

            if (!FactionNameExists(manager, baseName))
                return baseName;

            for (int i = 1; i < 1000; i++)
            {
                string candidate = $"{baseName}_{i}";
                if (!FactionNameExists(manager, candidate))
                    return candidate;
            }

            return $"{baseName}_{System.Guid.NewGuid().ToString("N").Substring(0, 6)}";
        }

        public static bool FactionNameExists(FactionManager manager, string factionName)
        {
            if (manager == null || !FactionManager.IsValidFactionName(factionName))
                return false;

            foreach (var faction in manager.factions)
            {
                if (faction.factionName == factionName)
                    return true;
            }

            return false;
        }

        public static bool FactionNameExistsExcept(FactionManager manager, string factionName, int exceptIndex)
        {
            if (manager == null || !FactionManager.IsValidFactionName(factionName))
                return false;

            for (int i = 0; i < manager.factions.Count; i++)
            {
                if (i == exceptIndex)
                    continue;

                if (manager.factions[i].factionName == factionName)
                    return true;
            }

            return false;
        }
    }
}
#endif
