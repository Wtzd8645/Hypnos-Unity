namespace Blanketmen.Hypnos.Editor
{
    // NOTE: There will have gap when priority have difference of 11.
    public enum EditorId
    {
        Kernel,
        Build,
        ScriptableObjectFactory
    }

    public partial class KernelEditor
    {
        public const string UnityEditorAssemblyName = "Assembly-CSharp-Editor";
        public const string FrameworkEditorAssemblyName = "Blanketmen.Hypnos.Editor";

        public const string ScriptableObjectExt = ".asset";

        public const string EditorBuiltInResourcePath = "Assets/Editor Default Resources/";
        public const string FrameworkPath = "Blanketmen/Hypnos/";
    }
}