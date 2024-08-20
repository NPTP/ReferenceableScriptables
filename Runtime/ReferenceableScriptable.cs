using NPTP.ReferenceableScriptables.Attributes;
using UnityEngine;

namespace NPTP.ReferenceableScriptables
{
    public abstract class ReferenceableScriptable : ScriptableObject
    {
        [SerializeField][GUIDisabled] private string guid;
        public string Guid => guid;
    }
}