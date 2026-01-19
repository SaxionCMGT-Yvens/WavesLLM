using System.Collections.Generic;
using System.Linq;
using Grid;

namespace Actors.AI.LlmAI
{
    public static class LlmAiPromptGenerator
    {
        public static string GeneratePrompt(LlmAINavalShip llmAINavalShip, LlmPromptSo templatePrompt)
        {
            var template = templatePrompt.prompt;
            var faction = llmAINavalShip.GetFaction().ToString();
            template = ReplaceTagWithText(template, "faction", faction);
            var shipData = llmAINavalShip.ShipData;
            template = ReplaceTagWithText(template, "move_count", shipData.stats.speed.Two.ToString());
            var cannonData = llmAINavalShip.NavalCannon;
            template = ReplaceTagWithText(template, "attack_range", cannonData.GetCannonSo.area.ToString());
            template = ReplaceTagWithText(template, "attack_offset", cannonData.GetCannonSo.deadZone.ToString());
            template = ReplaceTagWithText(template, "cannon", cannonData.ToString());
            template = ReplaceTagWithText(template, "self_status", llmAINavalShip.ToString());
            template = ReplaceTagWithText(template, "health", llmAINavalShip.GetCurrentHealth().ToString());
            var index = llmAINavalShip.GetUnit().Index();            
            template = ReplaceTagWithText(template, "self_x", index.x.ToString());
            template = ReplaceTagWithText(template, "self_y", index.y.ToString());
            template = ReplaceTagWithText(template, "movement_range", shipData.stats.speed.Two.ToString());
            var walkableUnits = GridManager.GetSingleton().GetGridUnitsInRadiusManhattan(index, llmAINavalShip.RemainingSteps);
            template = ReplaceTagWithText(template, "movement_positions", ListGridUnitsToString(walkableUnits, templatePrompt));
            
            //TODO change the method used to return also units that HAVE actors on them. Currently it is only returning empty positions.
            var attackableUnits = GridManager.GetSingleton()
                .GetAttackableUnitsInRadiusManhattan(index, cannonData.GetCannonSo, llmAINavalShip.RemainingSteps);
            template = ReplaceTagWithText(template, "possible_attack_positions", ListGridUnitsToString(attackableUnits, templatePrompt));
            var grid = GridManager.GetSingleton().Grid();
            template = ReplaceTagWithText(template, "grid_overview", ListGridUnitsToString(grid, templatePrompt));
            
            return template;
        }

        private static string ReplaceTagWithText(string template, string tag, string text)
        {
            return template.Replace($"@{tag}@", text);
        }

        private static string ListGridUnitsToString(List<GridUnit> gridUnits, LlmPromptSo promptSo)
        {
            var text = gridUnits.Aggregate("[", (current, gridUnit) =>
            {
                if (!gridUnit.IsEmpty()) return current + $"[{gridUnit.GetStringInfo()}],";
                if (promptSo.includeEmptySpaces)
                {
                    return current + $"[{gridUnit.GetStringInfo()}],";    
                }
                return current;

            });
            return text[..^1] + "]";
        }
    }
}