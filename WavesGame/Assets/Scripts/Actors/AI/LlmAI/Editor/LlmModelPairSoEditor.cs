using UnityEditor;
using UnityEngine;
using UUtils.Editor;

namespace Actors.AI.LlmAI.Editor
{
    [CustomEditor(typeof(LlmModelPairSo))]
    public class LlmModelPairSoEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            var llmModelPairSo = (LlmModelPairSo) target;
            if (GUILayout.Button("Rename LlmModelPair"))
            {
                RenameScriptableObjectHelper.RenameAssetFile(llmModelPairSo, llmModelPairSo.modelPair.ToString());
            }
        }
    }
}