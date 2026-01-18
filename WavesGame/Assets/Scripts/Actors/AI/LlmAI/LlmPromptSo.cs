using UnityEngine;

namespace Actors.AI.LlmAI
{
    [CreateAssetMenu(fileName = "LlmPrompt", menuName = "Waves/LLM/Prompt")]
    public class LlmPromptSo : ScriptableObject
    {
        [TextArea(10, 20)]
        public string basePrompt;
    }
}