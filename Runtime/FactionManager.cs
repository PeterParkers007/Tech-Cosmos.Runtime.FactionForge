using System.Collections.Generic;
using UnityEngine;

namespace TechCosmos.FactionForge.Runtime
{
    public class FactionManager : MonoBehaviour
    {
        [SerializeField] public List<Faction> factions = new List<Faction>();

        private static FactionManager instance;
        public static FactionManager Instance => instance;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeRelationships();
        }

        private void InitializeRelationships()
        {
            foreach (var faction in factions)
                UpdateFactionRelationships(faction);
        }

        private void UpdateFactionRelationships(Faction faction)
        {
            if (!IsValidFactionName(faction.factionName))
                return;

            foreach (var otherFaction in factions)
            {
                if (otherFaction.factionName != faction.factionName &&
                    IsValidFactionName(otherFaction.factionName))
                {
                    if (!faction.relationships.ContainsKey(otherFaction.factionName))
                        faction.relationships[otherFaction.factionName] = FactionRelationship.Neutral;
                }
            }

            var keysToRemove = new List<string>();
            foreach (var relationship in faction.relationships)
            {
                if (!FactionExists(relationship.Key))
                    keysToRemove.Add(relationship.Key);
            }

            foreach (var key in keysToRemove)
                faction.relationships.Remove(key);
        }

        private bool FactionExists(string factionName)
        {
            return IsValidFactionName(factionName) &&
                   factions.Exists(f => f.factionName == factionName);
        }

        public static bool IsValidFactionName(string factionName)
        {
            return !string.IsNullOrWhiteSpace(factionName);
        }

        public bool HasDuplicateFactionNames()
        {
            var seen = new HashSet<string>();
            foreach (var faction in factions)
            {
                if (!IsValidFactionName(faction.factionName))
                    continue;

                if (!seen.Add(faction.factionName))
                    return true;
            }

            return false;
        }

        public bool CanConfigureRelationship(string factionA, string factionB)
        {
            if (!IsValidFactionName(factionA) || !IsValidFactionName(factionB))
                return false;

            if (factionA == factionB)
                return false;

            if (!FactionExists(factionA) || !FactionExists(factionB))
                return false;

            return true;
        }

        public FactionRelationship GetRelationship(string factionA, string factionB)
        {
            if (factionA == factionB)
                return FactionRelationship.Friendly;

            var faction = factions.Find(f => f.factionName == factionA);
            if (faction != null && faction.relationships.ContainsKey(factionB))
                return faction.relationships[factionB];

            return FactionRelationship.Neutral;
        }

        public void SetRelationship(string factionA, string factionB, FactionRelationship relationship, bool bidirectional = false)
        {
            if (!CanConfigureRelationship(factionA, factionB))
                return;

            SetRelationshipOneWay(factionA, factionB, relationship);

            if (bidirectional)
                SetRelationshipOneWay(factionB, factionA, relationship);
        }

        private void SetRelationshipOneWay(string factionA, string factionB, FactionRelationship relationship)
        {
            var faction = factions.Find(f => f.factionName == factionA);
            if (faction != null && FactionExists(factionB))
                faction.relationships[factionB] = relationship;
        }

        public bool AreRelationshipsSymmetric(string factionA, string factionB)
        {
            if (factionA == factionB)
                return true;

            if (!CanConfigureRelationship(factionA, factionB))
                return true;

            return GetRelationship(factionA, factionB) == GetRelationship(factionB, factionA);
        }

        public bool RenameFaction(string oldName, string newName)
        {
            if (!IsValidFactionName(oldName) || !IsValidFactionName(newName))
                return false;

            if (oldName == newName)
                return true;

            var faction = factions.Find(f => f.factionName == oldName);
            if (faction == null)
                return false;

            if (factions.Exists(f => f != faction && f.factionName == newName))
                return false;

            faction.factionName = newName;

            foreach (var otherFaction in factions)
            {
                if (otherFaction.relationships.ContainsKey(oldName))
                {
                    var relationship = otherFaction.relationships[oldName];
                    otherFaction.relationships.Remove(oldName);
                    otherFaction.relationships[newName] = relationship;
                }
            }

            InitializeRelationships();
            return true;
        }

        public int SyncAllRelationshipsBidirectional()
        {
            int syncedCount = 0;

            for (int i = 0; i < factions.Count; i++)
            {
                for (int j = i + 1; j < factions.Count; j++)
                {
                    var nameA = factions[i].factionName;
                    var nameB = factions[j].factionName;

                    if (!CanConfigureRelationship(nameA, nameB))
                        continue;

                    var relAtoB = GetRelationship(nameA, nameB);
                    var relBtoA = GetRelationship(nameB, nameA);

                    if (relAtoB != relBtoA)
                    {
                        SetRelationship(nameA, nameB, relAtoB, bidirectional: true);
                        syncedCount++;
                    }
                }
            }

            return syncedCount;
        }

        public void RefreshAllRelationships()
        {
            InitializeRelationships();
        }
    }
}
