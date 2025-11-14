// FactionMember.cs
using UnityEngine;

namespace TechCosmos.FactionForge.Runtime
{
    public class FactionMember : MonoBehaviour
    {
        [SerializeField] private string factionName = "Neutral";
        [SerializeField] private bool canChangeFaction = false;

        public string FactionName => factionName;
        public event System.Action<string> OnFactionChanged;

        public void SetFaction(string newFaction)
        {
            if (!canChangeFaction)
            {
                Debug.LogWarning($"角色 {gameObject.name} 的阵营被设置为不可更改", this);
                return;
            }

            string oldFaction = factionName;
            factionName = newFaction;
            OnFactionChanged?.Invoke(oldFaction);

            Debug.Log($"角色 {gameObject.name} 的阵营从 {oldFaction} 变更为 {newFaction}");
        }

        public FactionRelationship GetRelationshipWith(FactionMember other)
        {
            if (other == null) return FactionRelationship.Neutral;
            return FactionManager.Instance.GetRelationship(factionName, other.factionName);
        }

        public FactionRelationship GetRelationshipWith(string otherFaction)
        {
            return FactionManager.Instance.GetRelationship(factionName, otherFaction);
        }

        // 便捷判断方法
        public bool IsHostileTo(FactionMember other)
            => GetRelationshipWith(other) == FactionRelationship.Hostile;

        public bool IsFriendlyTo(FactionMember other)
            => GetRelationshipWith(other) == FactionRelationship.Friendly;

        public bool IsAlliedTo(FactionMember other)
            => GetRelationshipWith(other) == FactionRelationship.Allied;

        public bool IsNeutralTo(FactionMember other)
            => GetRelationshipWith(other) == FactionRelationship.Neutral;
    }
}