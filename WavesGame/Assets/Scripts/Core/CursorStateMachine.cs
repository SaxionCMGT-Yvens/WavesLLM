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
        private readonly CursorController _cursorController;
        private GridUnit _currentGridUnit;

        public CursorStateMachine(CursorController cursorController)
        {
            _cursorController = cursorController;
            CurrentState = CursorState.Roaming;
        }

        public void InteractOnUnit(GridUnit unit)
        {
            switch (CurrentState)
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
                    if (_cursorController.TargetSelectedGridUnit(unit))
                    {
                        ChangeStateTo(CursorState.Roaming);
                    }
                    break;
                case CursorState.Moving:
                    if (_cursorController.MoveSelectedActorTo(unit))
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
            DebugUtils.DebugLogMsg($"Change state from {CurrentState} to {newState}.", DebugUtils.DebugType.System);
            switch (CurrentState)
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

            CurrentState = newState;
            DebugUtils.DebugLogMsg($"Stated changed to {CurrentState}.", DebugUtils.DebugType.System);
            
            switch (CurrentState)
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
                    _cursorController.ToggleActive(true);
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

        public CursorState CurrentState { get; private set; }
    }
}