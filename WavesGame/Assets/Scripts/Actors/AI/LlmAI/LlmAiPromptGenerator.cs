using System.Collections.Generic;
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
            template = ReplaceTagWithText(template, "self_status", llmAINavalShip.ToLlmString());
            template = ReplaceTagWithText(template, "health", llmAINavalShip.GetCurrentHealth().ToString());
            var index = llmAINavalShip.GetUnit().Index();            
            template = ReplaceTagWithText(template, "self_x", index.x.ToString());
            template = ReplaceTagWithText(template, "self_y", index.y.ToString());
            template = ReplaceTagWithText(template, "movement_range", shipData.stats.speed.Two.ToString());
            
            var walkableUnits = GridManager.GetSingleton().GetGridUnitsInRadiusManhattan(index, llmAINavalShip.RemainingSteps);
            var movementPositions = ListGridPositionsToString(walkableUnits);
            template = ReplaceTagWithText(template, "movement_positions", movementPositions);
            
            var attackableUnits = GridManager.GetSingleton()
                .GetAttackableUnitsInRadiusManhattan(index, cannonData.GetCannonSo, llmAINavalShip.RemainingSteps);
            attackableUnits = attackableUnits.FindAll(unit => !unit.IsEmpty());
            template = ReplaceTagWithText(template, "possible_attack_positions", ListGridUnitsToString(attackableUnits, templatePrompt));
            
            var grid = GridManager.GetSingleton().Grid();
            
            template = ReplaceTagWithText(template, "grid_overview", ListGridToString(llmAINavalShip, grid, false));
            
            return template;
        }

        private static string ReplaceTagWithText(string template, string tag, string text)
        {
            return template.Replace($"@{tag}@", text);
        }
        
        private static string ListGridToString(LlmAINavalShip selfShip, List<GridUnit> gridUnits, bool includeEmpty = true)
        {
            if (gridUnits == null || gridUnits.Count == 0)
            {
                return "Nothing";
            }
            
            var text = "[";
            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var gridUnit in gridUnits)
            {
                //(Format: [x,y] = content, where content can be: SHIP_faction_health_ratio, TARGET_health, WAVE_direction, or EMPTY)
                var index = gridUnit.Index();
                if (gridUnit.IsEmpty() && includeEmpty)
                {
                    text += $"[{index.x}, {index.y}] = EMPTY;";
                }
                else
                {
                    var topActor = gridUnit.GetActor();
                    switch (topActor)
                    {
                        case LlmAINavalShip llmAINavalShip:
                        {
                            text += $"[{index.x}, {index.y}] = ";
                            if (llmAINavalShip.Equals(selfShip))
                            {
                                text += "SELF;";
                            }
                            else
                            {
                                var opposingFaction = !selfShip.GetFaction().Equals(llmAINavalShip.GetFaction());
                                var factionText = opposingFaction ? $"Enemy Faction-{llmAINavalShip.GetFaction()}" : "Ally";
                                var health = llmAINavalShip.GetCurrentHealth();
                                var ratio = llmAINavalShip.GetHealthRatio();
                                text += $"SHIP_{factionText}_health[{health}]_ratio[{ratio}];";
                            }

                            break;
                        }
                        case WaveActor wave:
                            text += $"[{index.x}, {index.y}] = WAVE_direction[{wave.GetWaveDirection}];";
                            break;
                        case NavalTarget target:
                            text += $"[{index.x}, {index.y}] = TARGET_health[{target.GetCurrentHealth()}];";
                            break;
                    }
                }
            }
            return text[..^1] + "]";
        }

        private static string ListGridUnitsToString(List<GridUnit> gridUnits, bool includeEmpty = true)
        {
            if (gridUnits == null || gridUnits.Count == 0)
            {
                return "Nothing";
            }
            
            var text = "[";
            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var gridUnit in gridUnits)
            {
                if (!gridUnit.IsEmpty() || (gridUnit.IsEmpty() && includeEmpty))
                {
                    text += $"[{gridUnit.GetStringInfo()}],";    
                }
            }
            return text[..^1] + "]";
        }
        
        private static string ListGridPositionsToString(List<GridUnit> gridUnits)
        {
            if (gridUnits == null || gridUnits.Count == 0)
            {
                return "[Nothing]";
            }
            
            var text = "[";
            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var gridUnit in gridUnits)
            {
                if (!gridUnit.IsEmpty()) continue;
                var index = gridUnit.Index();
                text += $"({index.x}, {index.y}),";
            }
            return text[..^1] + "]";
        }
    }
}