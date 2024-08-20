namespace NPTP.ReferenceableScriptables
{
    /// <summary>
    /// Top class for runtime usage of Referenceable Scriptables.
    /// </summary>
    public static class Referenceables
    {
        /// <summary>
        /// Try to load a Referenceable Scriptable at the given guid address.
        /// Note that there is no need to unload what is loaded - the scriptable reference stays on the stack.
        /// </summary>
        public static bool TryLoad<T>(string guid, out T scriptable) where T : ReferenceableScriptable
        {
            if (!ReferenceablesTable.TryLoad(guid, out var referenceableScriptable))
            {
                scriptable = null;
                return false;
            }

            scriptable = referenceableScriptable as T;
            return scriptable != null;
        }
    }
}
