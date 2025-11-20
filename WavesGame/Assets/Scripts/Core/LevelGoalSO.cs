using UnityEngine;

namespace Core
{
    public enum LevelGoalType
    {
        DestroyAllTargets, DestroyAllEnemies, SurviveForTurns, DestroySpecificEnemy, Custom
    }
    
    [CreateAssetMenu(fileName = "New Level Goal", menuName = "Waves/Level Goal", order = 3)]
    public class LevelGoalSO : ScriptableObject
    {
        public LevelGoalType type;
        
        //TODO for the custom type, create a sort of prefab with script checker.
    }
}