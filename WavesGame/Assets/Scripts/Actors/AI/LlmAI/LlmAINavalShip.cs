using System.Collections;
using FALLA;
using UnityEngine;
using UUtils;

namespace Actors.AI.LlmAI
{
    public class LlmAINavalShip : AIBaseShip
    {
        [Header("LLM")]
        [SerializeField] 
        private LlmCallerObject llmCaller;
        [SerializeField]
        private LlmPromptSo basePrompt;
        
        protected override void Awake()
        {
            base.Awake();
            AssessUtils.CheckRequirement(ref llmCaller, this);
        }

        protected override IEnumerator TurnAI()
        {
            yield return new WaitForSeconds(0.05f);

            var prompt = LlmAiPromptGenerator.GeneratePrompt(this, basePrompt);
            llmCaller.CallLlm(prompt);
            yield return new WaitUntil(() => llmCaller.IsReady());
            var result = llmCaller.GetResponse();
            
        }
    }
}