namespace TechCosmos.FactionForge.Runtime
{
    [System.Serializable]
    public class Faction
    {
        public string factionName;

        // 序列化字典，用于在Inspector中显示
        [System.Serializable]
        public class RelationshipDictionary : SerializableDictionary<string, FactionRelationship> { }

        public RelationshipDictionary relationships = new RelationshipDictionary();
    }
}
