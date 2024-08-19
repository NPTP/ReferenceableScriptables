using System;
using UnityEngine;

namespace NPTP.ReferenceableScriptables
{
    [Serializable]
    public class GuidToPath
    {
        [SerializeField] private string guid;
        public string Guid => guid;
        [SerializeField] private string path;
        public string Path => path;

        public GuidToPath(string guid, string path)
        {
            this.guid = guid;
            this.path = path;
        }
    }
}