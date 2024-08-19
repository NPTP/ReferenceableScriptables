using NPTP.ReferenceableScriptables.Attributes;
using UnityEngine;

namespace NPTP.ReferenceableScriptables
{
    public abstract class ReferenceableScriptable : ScriptableObject
    {
#if UNITY_EDITOR
        [SerializeField][GUIDisabled] private ScriptableReferenceContainer referenceContainer;
        public ScriptableReferenceContainer EDITOR_ReferenceContainer
        {
            get => referenceContainer;
            set => referenceContainer = value;
        }

        public void EDITOR_SetGuid(string newGuid) => guid = newGuid;
#endif
        
        [SerializeField][HideInInspector] private bool referenceable;
        [SerializeField][GUIDisabled] private string guid;
        public string Guid => guid;
        
        private void OnDestroy()
        {
            Referenceables.Unload(this);
        }
    }
}