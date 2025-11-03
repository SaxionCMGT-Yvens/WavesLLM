using System;
using Actors;
using Grid;
using UUtils;

namespace Core
{
    public enum CursorState
    {
        Roaming,
        SelectGridUnit,
        ShowingOptions,
        Moving,
        OnTheMove,
        Targeting
    }

    public class CursorStateMachine
    {
        private CursorController _cursorController;
        private CursorState _currentState;
        private GridUnit _currentGridUnit;

        public CursorStateMachine(CursorController cursorController)
        {
            _cursorController = cursorController;
            _currentState = CursorState.Roaming;
        }

        public void InteractOnUnit(GridUnit unit)
        {
            switch (_currentState)
            {
                case CursorState.Roaming:
                    _currentGridUnit = unit;
                    ChangeStateTo(CursorState.SelectGridUnit);
                    break;
                case CursorState.SelectGridUnit:
                    break;
                case CursorState.ShowingOptions:
                    break;
                case CursorState.Targeting:
                    break;
                case CursorState.Moving:
                    if (_cursorController.MoveTo(unit))
                    {
                        ChangeStateTo(CursorState.OnTheMove);    
                    }
                    break;
                case CursorState.OnTheMove:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void ChangeStateTo(CursorState newState)
        {
            DebugUtils.DebugLogMsg($"Change state from {_currentState} to {newState}.", DebugUtils.DebugType.System);
            switch (_currentState)
            {
                case CursorState.Roaming:
                    _cursorController.ToggleActive(false);
                    break;
                case CursorState.SelectGridUnit:
                    break;
                case CursorState.ShowingOptions:
                    _cursorController.HideOptionsPanel();
                    break;
                case CursorState.Targeting:
                    break;
                case CursorState.Moving:
                    _cursorController.ToggleActive(false);
                    break;
                case CursorState.OnTheMove:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _currentState = newState;
            DebugUtils.DebugLogMsg($"Stated changed to {_currentState}.", DebugUtils.DebugType.System);
            
            switch (_currentState)
            {
                case CursorState.Roaming:
                    _cursorController.ToggleActive(true);
                    _cursorController.ResetWalkableUnits();
                    break;
                case CursorState.SelectGridUnit:
                    DebugUtils.DebugLogMsg($"Selecting grid unit {_currentGridUnit} | Contains {_currentGridUnit.ActorsCount()} actors.", DebugUtils.DebugType.Verbose);
                    if (_currentGridUnit.ActorsCount() > 0)
                    {
                        var getTopActor = _currentGridUnit.GetActor();
                        if (getTopActor is NavalActor navalActor)
                        {
                            DebugUtils.DebugLogMsg($"Top actor is a Naval Actor {navalActor.name} {navalActor}.", DebugUtils.DebugType.Verbose);
                            _cursorController.SetSelectedActor(navalActor);
                            // ReSharper disable once TailRecursiveCall
                            ChangeStateTo(CursorState.ShowingOptions);
                        }
                        else
                        {
                            //TODO something wrong
                        }
                    }
                    else
                    {
                        DebugUtils.DebugLogMsg($"No actors found in grid unit {_currentGridUnit}.", DebugUtils.DebugType.Verbose);
                        // ReSharper disable once TailRecursiveCall
                        ChangeStateTo(CursorState.Roaming);
                        // If select a grid unit with no actor, then just return back to roaming
                    }

                    break;
                case CursorState.ShowingOptions:
                    _cursorController.ShowOptionsForSelectedActor();
                    break;
                case CursorState.Targeting:
                    break;
                case CursorState.Moving:
                    _cursorController.ToggleActive(true);
                    break;
                case CursorState.OnTheMove:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public CursorState CurrentState => _currentState;
    }
}