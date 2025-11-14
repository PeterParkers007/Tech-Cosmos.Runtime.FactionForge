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
            // 确保所有阵营的关系字典都是最新的
            foreach (var faction in factions)
            {
                UpdateFactionRelationships(faction);
            }
        }

        private void UpdateFactionRelationships(Faction faction)
        {
            // 为当前阵营更新与其他所有阵营的关系条目
            foreach (var otherFaction in factions)
            {
                if (otherFaction.factionName != faction.factionName)
                {
                    if (!faction.relationships.ContainsKey(otherFaction.factionName))
                    {
                        faction.relationships[otherFaction.factionName] = FactionRelationship.Neutral;
                    }
                }
            }

            // 移除已经不存在的阵营关系
            var keysToRemove = new List<string>();
            foreach (var relationship in faction.relationships)
            {
                if (!FactionExists(relationship.Key))
                {
                    keysToRemove.Add(relationship.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                faction.relationships.Remove(key);
            }
        }

        private bool FactionExists(string factionName)
        {
            return factions.Exists(f => f.factionName == factionName);
        }

        // 公开API
        public FactionRelationship GetRelationship(string factionA, string factionB)
        {
            var faction = factions.Find(f => f.factionName == factionA);
            if (faction != null && faction.relationships.ContainsKey(factionB))
            {
                return faction.relationships[factionB];
            }
            return FactionRelationship.Neutral;
        }

        public void SetRelationship(string factionA, string factionB, FactionRelationship relationship)
        {
            var faction = factions.Find(f => f.factionName == factionA);
            if (faction != null && FactionExists(factionB))
            {
                faction.relationships[factionB] = relationship;
            }
        }

        // Editor工具调用的方法
        public void RefreshAllRelationships()
        {
            InitializeRelationships();
        }
    }
}
