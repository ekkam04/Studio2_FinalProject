using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Ekkam {
    public class PreviewTime
    {
        public static float time
        {
            get
            {
                if (Application.isPlaying) return UnityEngine.Time.timeSinceLevelLoad;
                else return EditorPrefs.GetFloat("PreviewTime", 0f);
            }
            set
            {
                EditorPrefs.SetFloat("PreviewTime", value);
            }
        }
    }
}
