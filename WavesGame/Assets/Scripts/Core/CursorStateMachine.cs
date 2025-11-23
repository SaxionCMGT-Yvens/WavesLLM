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
                case CursorState.SelectGridUnit: //Nothing to do
                case CursorState.ShowingOptions: //Nothing to do
                    break;
                case CursorState.Targeting:
                    if (_cursorController.TargetSelectedGridUnit(unit))
                    {
                        var index = _currentGridUnit.Index();
                        DebugUtils.DebugLogMsg($"Going to position {index}.", DebugUtils.DebugType.Verbose);
                        _cursorController.MoveToIndex(index);
                        ChangeStateTo(CursorState.Roaming);
                    }
                    break;
                case CursorState.Moving:
                    //TODO change this because it is acting synchronously
                    var move = _cursorController.MoveSelectedActorTo(unit, (final) =>
                    {
                        DebugUtils.DebugLogMsg($"Going to final position {final} {(final != null ? final.Index() : "[Invalid]")}.", DebugUtils.DebugType.Verbose);
                        _currentGridUnit = final == null ? unit : final;
                        var index = _currentGridUnit.Index();
                        DebugUtils.DebugLogMsg($"Move to the index: {index}.", DebugUtils.DebugType.Verbose);
                        _cursorController.MoveToIndex(index, false);
                    });
                    if (move)
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
                    //TODO remove target from the enemies
                    PlayerController.GetSingleton().onCancel -= CancelTargetingCommand;
                    break;
                case CursorState.Moving:
                    _cursorController.ToggleActive(false);
                    PlayerController.GetSingleton().onCancel -= CancelMovementCommand;
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
                        var actorEnumerator = _currentGridUnit.GetActorEnumerator();
                        while(actorEnumerator.MoveNext())
                        {
                            var getTopActor = actorEnumerator.Current;
                            if (getTopActor is not NavalActor navalActor) continue;
                            DebugUtils.DebugLogMsg($"Top actor is a Naval Actor {navalActor.name} {navalActor}.",
                                DebugUtils.DebugType.Verbose);
                            _cursorController.SetSelectedActor(navalActor);
                            // ReSharper disable once TailRecursiveCall
                            actorEnumerator.Dispose();
                            ChangeStateTo(CursorState.ShowingOptions);
                            return;
                        }
                        actorEnumerator.Dispose();
                        NoValidActorFound();
                    }
                    else
                    {
                        NoValidActorFound();
                    }

                    break;
                case CursorState.ShowingOptions:
                    _cursorController.ShowOptionsForSelectedActor();
                    break;
                case CursorState.Targeting:
                    _cursorController.ToggleActive(true);
                    PlayerController.GetSingleton().onCancel += CancelTargetingCommand;
                    break;
                case CursorState.Moving:
                    _cursorController.ToggleActive(true);
                    PlayerController.GetSingleton().onCancel += CancelMovementCommand;
                    break;
                case CursorState.OnTheMove:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return;

            void NoValidActorFound()
            {
                DebugUtils.DebugLogMsg($"No valid actors found in grid unit {_currentGridUnit}.", DebugUtils.DebugType.Verbose);
                ChangeStateTo(CursorState.Roaming);
                // If select a grid unit with no actor, then just return back to roaming
            }
        }

        private void CancelMovementCommand()
        {
            ChangeStateTo(CursorState.Roaming);
        }

        private void CancelTargetingCommand()
        {
            _cursorController.HideAttackArea();
            ChangeStateTo(CursorState.Roaming);
        }

        public CursorState CurrentState { get; private set; }
    }
}