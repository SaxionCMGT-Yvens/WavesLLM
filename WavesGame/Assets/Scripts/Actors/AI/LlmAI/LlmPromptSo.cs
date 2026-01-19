using UnityEngine;

namespace Actors.AI.LlmAI
{
    [CreateAssetMenu(fileName = "LlmPrompt", menuName = "Waves/LLM/Prompt")]
    public class LlmPromptSo : ScriptableObject
    {
        [TextArea(10, 40)]
        public string prompt;

        public bool includeEmptySpaces;
    }
}