using NPTP.ReferenceableScriptables.Attributes;
using UnityEngine;

namespace NPTP.ReferenceableScriptables.AssetTypes
{
    public abstract class ReferenceableScriptable : ScriptableObject
    {
        [SerializeField][HideInInspector][GUIDisabled] private string guid;
        public string Guid => guid;
    }
}