using System.Collections.Generic;
using Grid;
using Unity.VisualScripting;

namespace Actors.AI.LlmAI
{
    public static class LlmAiPromptGenerator
    {
        public static string GeneratePrompt(LlmAINavalShip llmAINavalShip, LlmPromptSo templatePrompt,
            List<AIFaction> enemyFactions)
        {
            var template = templatePrompt.prompt;
            var selfFaction = llmAINavalShip.GetFaction();
            var faction = selfFaction.ToString();
            template = ReplaceTagWithText(template, "faction", faction);
            template = ReplaceTagWithText(template, "enemy_factions", ListEnemyFactions(enemyFactions));
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
            var dimensions = GridManager.GetSingleton().GetDimensions();
            template = ReplaceTagWithText(template, "grid_size", dimensions.ToString());

            var walkableUnits = GridManager.GetSingleton()
                .GetGridUnitsInRadiusManhattan(index, llmAINavalShip.RemainingSteps);
            var movementPositions = ListGridUnitIndicesToString(walkableUnits);
            template = ReplaceTagWithText(template, "movement_positions", movementPositions);

            var safePositions = walkableUnits.FindAll(position => position.IsEmpty());
            template = ReplaceTagWithText(template, "safe_movement_positions",
                ListGridUnitIndicesToString(safePositions, true, "\r\n"));

            var nearbyWavePositions = walkableUnits.FindAll(position => position.HasActorOfType<WaveActor>());
            template = ReplaceTagWithText(template, "wave_movement_positions",
                ListGridUnitIndicesToString(nearbyWavePositions, false, "\r\n"));

            var nearbyShipsPositions = new List<GridActor>();
            walkableUnits.ForEach(position =>
            {
                if (position.Index().Equals(index) || !position.GetFirstActorOfType<GridActor>(out var actor)) return;
                if (actor is not WaveActor)
                {
                    nearbyShipsPositions.Add(actor);    
                }
            });
            template = ReplaceTagWithText(template, "blocked_movement_positions",
                ListGridActorsIndicesToString(nearbyShipsPositions, selfFaction, true, "\r\n"));

            var attackableUnits = GridManager.GetSingleton()
                .GetAttackableUnitsInRadiusManhattan(index, cannonData.GetCannonSo, llmAINavalShip.RemainingSteps);
            attackableUnits = attackableUnits.FindAll(unit => !unit.IsEmpty());

            template = ReplaceTagWithText(template, "possible_attack_positions",
                ListGridUnitsToString(attackableUnits, templatePrompt));

            var currentAttackableUnits = GridManager.GetSingleton()
                .GetGridUnitsForMoveType(cannonData.GetCannonSo.targetAreaType, index, cannonData.GetCannonSo.area,
                    cannonData.GetCannonSo.deadZone);
            var currentAttackableActors = new List<GridActor>();
            currentAttackableUnits.ForEach(position =>
            {
                if (position.Index().Equals(index) || !position.GetFirstActorOfType<GridActor>(out var actor)) return;
                if (actor is AINavalShip navalShip)
                {
                    if (navalShip.GetFaction() != selfFaction)
                    {
                        currentAttackableActors.Add(actor);        
                    }
                }
                else
                {
                    currentAttackableActors.Add(actor);
                }
            });

            template = ReplaceTagWithText(template, "current_attack_positions",
                ListGridActorsIndicesToString(currentAttackableActors, selfFaction, false, "\r\n"));

            var grid = GridManager.GetSingleton().Grid();

            template = ReplaceTagWithText(template, "grid_overview",
                ListGridToString(llmAINavalShip, grid, templatePrompt.includeEmptySpaces));

            template = ReplaceTagWithText(template, "grid_overview_symbolic",
                ListSymbolicGridToString(llmAINavalShip, grid));

            return template;
        }

        private static string ReplaceTagWithText(string template, string tag, string text)
        {
            return template.Replace($"@{tag}@", text);
        }

        private static string ListSymbolicGridToString(LlmAINavalShip selfShip, List<GridUnit> gridUnits)
        {
            if (gridUnits == null || gridUnits.Count == 0)
            {
                return "Nothing";
            }

            var text = "\r\n";
            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var gridUnit in gridUnits)
            {
                var index = gridUnit.Index();
                if (!gridUnit.IsEmpty())
                {
                    var topActor = gridUnit.GetActor();
                    text += $"{index} = ";
                    switch (topActor)
                    {
                        case LlmAINavalShip llmAINavalShip:
                        {
                            if (!llmAINavalShip.Equals(selfShip))
                            {
                                
                                var opposingFaction = !selfShip.GetFaction().Equals(llmAINavalShip.GetFaction());
                                var factionText = opposingFaction ? $"**Enemy**" : "**Ally**";
                                var health = llmAINavalShip.GetCurrentHealth();
                                var ratio = llmAINavalShip.GetHealthRatio();
                                text += $"🚢 {factionText} health:{health} ratio: {ratio}\r\n";
                            }
                            else
                            {
                                text += "🚢 self\r\n";
                            }

                            break;
                        }
                        case WaveActor wave:
                            text +=
                                $"{GridMoveTypeExtensions.GridMovementSymbol(wave.GetWaveDirection)}\r\n";
                            break;
                        case NavalTarget:
                            text += "= 🎯\r\n";
                            break;
                    }
                }
            }

            return text;
        }

        private static string ListGridToString(LlmAINavalShip selfShip, List<GridUnit> gridUnits,
            bool includeEmpty = true)
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
                    text += $"{index} = EMPTY;";
                }
                else
                {
                    var topActor = gridUnit.GetActor();
                    switch (topActor)
                    {
                        case LlmAINavalShip llmAINavalShip:
                        {
                            text += $"{index} = ";
                            if (llmAINavalShip.Equals(selfShip))
                            {
                                text += "SELF;";
                            }
                            else
                            {
                                var opposingFaction = !selfShip.GetFaction().Equals(llmAINavalShip.GetFaction());
                                var factionText = opposingFaction ? $"Enemy {llmAINavalShip.GetFaction()}" : "Ally";
                                var health = llmAINavalShip.GetCurrentHealth();
                                var ratio = llmAINavalShip.GetHealthRatio();
                                text += $"🚢 {factionText} health:{health} ratio: {ratio};";
                            }

                            break;
                        }
                        case WaveActor wave:
                            text +=
                                $"{index} = {GridMoveTypeExtensions.GridMovementSymbol(wave.GetWaveDirection)};";
                            break;
                        case NavalTarget target:
                            text += $"{index} = 🎯 health:{target.GetCurrentHealth()};";
                            break;
                    }
                }
            }

            return text[..^1] + "]";
        }

        private static string ListEnemyFactions(List<AIFaction> enemyFactions)
        {
            var text = "";
            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var aiFaction in enemyFactions)
            {
                text += $"{aiFaction},";
            }

            return text[..^1];
        }

        private static string ListGridUnitsToString(List<GridUnit> gridUnits, bool includeEmpty = true)
        {
            if (gridUnits == null || gridUnits.Count == 0)
            {
                return "[Nothing]";
            }

            var text = "[";
            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var gridUnit in gridUnits)
            {
                if (!gridUnit.IsEmpty() || (gridUnit.IsEmpty() && includeEmpty))
                {
                    text += $"{gridUnit.GetStringInfo()},";
                }
            }

            return text[..^1] + "]";
        }

        private static string ListGridUnitIndicesToString(List<GridUnit> gridUnits, bool includeEmpty = true,
            string separator = ",")
        {
            if (gridUnits == null || gridUnits.Count == 0)
            {
                return "Nothing";
            }

            var text = "\r\n";
            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var gridUnit in gridUnits)
            {
                if (!gridUnit.IsEmpty() || (gridUnit.IsEmpty() && includeEmpty))
                {
                    text += $"{gridUnit.Index()}{separator}";
                }
            }

            return separator.Equals(",") ? text[..^1] : text + "\r\n";
        }

        private static string ListGridActorsIndicesToString(List<GridActor> gridActors, AIFaction selfFaction, bool fullInfo = false,
            string separator = ",")
        {
            if (gridActors == null || gridActors.Count == 0)
            {
                return "[Nothing]";
            }

            var text = "\r\n";
            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var gridActor in gridActors)
            {
                if (gridActor == null || gridActor.GetUnit() == null) continue;
                var index = gridActor.GetUnit().Index();
                if (fullInfo)
                {
                    switch (gridActor)
                    {
                        case NavalTarget navalTarget:
                            text += $"{index} = 🎯 health:{navalTarget.GetCurrentHealth()}{separator}";
                            break;
                        case LlmAINavalShip llmAINavalShip:
                            var opposingFaction = !selfFaction.Equals(llmAINavalShip.GetFaction());
                            var factionText = opposingFaction ? $"Enemy {llmAINavalShip.GetFaction()}" : "Ally";
                            text += $"{index} = 🚢 {factionText} health:{llmAINavalShip.GetCurrentHealth()} ratio: {llmAINavalShip.GetHealthRatio()}{separator}";
                            break;
                        case WaveActor wave:
                            text += $"{index} = {GridMoveTypeExtensions.GridMovementSymbol(wave.GetWaveDirection)}{separator}";
                            break;
                    }

                    text += $"{separator}";
                }
                else
                {
                    text += $"{gridActor.GetUnit().Index()}{separator}";
                }
            }

            return separator.Equals(",") ? text[..^1] : text + "\r\n";
        }
    }
}