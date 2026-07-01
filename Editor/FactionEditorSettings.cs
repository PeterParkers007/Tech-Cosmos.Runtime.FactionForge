#if UNITY_EDITOR
namespace TechCosmos.FactionForge.Editor
{
    internal static class FactionEditorSettings
    {
        private const string BidirectionalEditModeKey = "FactionForge.BidirectionalEditMode";

        public static bool BidirectionalEditMode
        {
            get => UnityEditor.EditorPrefs.GetBool(BidirectionalEditModeKey, false);
            set => UnityEditor.EditorPrefs.SetBool(BidirectionalEditModeKey, value);
        }
    }
}
#endif
