using NPTP.ReferenceableScriptables.Attributes;
using UnityEngine;

namespace NPTP.ReferenceableScriptables
{
    public abstract class ReferenceableScriptable : ScriptableObject
    {
#if UNITY_EDITOR
        public void EDITOR_SetGuid(string newGuid) => guid = newGuid;
#endif
        
        [SerializeField][HideInInspector] private bool referenceable;
        [SerializeField][HideInInspector][GUIDisabled] private string guid;
        public string Guid => guid;
        
        private void OnDestroy()
        {
            Referenceables.Unload(this);
        }
    }
}