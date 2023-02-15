namespace Hypnos.Editor
{
    // NOTE: There will have gap when priority have difference of 11.
    public enum EditorId
    {
        Kernel,
        Build,
        ScriptableObjectFactory
    }

    public partial class EditorKernel
    {
        public const string UnityEditorAssemblyName = "Assembly-CSharp-Editor";
        public const string FrameworkEditorAssemblyName = "Hypnos.Editor";

        public const string FrameworkPath = "Hypnos/";
    }
}