using System;
using System.Collections.Generic;
using Actors;
using DG.Tweening;
using Grid;
using NaughtyAttributes;
using UI;
using UnityEngine;
using UUtils;

namespace Core
{
    public class CursorController : WeakSingleton<CursorController>
    {
        [Header("Data")] [SerializeField, ReadOnly]
        private Vector2Int index;

        [SerializeField] private Vector2Int initialIndex;

        [Header("References")] [SerializeField]
        private Animator cursorAnimator;

        [SerializeField] private PlayerNavalShipOptionsPanel navalShipOptionsPanel;

        private CursorStateMachine _stateMachine;
        private List<GridUnit> _walkableUnits;
        private NavalActor _selectedActor;
        private bool _movingAnimation;
        private bool _active = true;
        private bool _levelFinished;

        private static readonly int Select = Animator.StringToHash("Select");
        private static readonly int Finish = Animator.StringToHash("Finish");

        protected override void Awake()
        {
            base.Awake();
            AssessUtils.CheckRequirement(ref cursorAnimator, this);
        }

        #region Action-Related

        private void OnEnable()
        {
            PlayerController.GetSingleton().onMoveAction += Move;
            PlayerController.GetSingleton().onInteract += Interact;
        }

        private void OnDisable()
        {
            RemoveControllerCallbacks();
        }

        private void RemoveControllerCallbacks()
        {
            PlayerController.GetSingleton().onMoveAction -= Move;
            PlayerController.GetSingleton().onInteract -= Interact;
        }

        #endregion

        private void Start()
        {
            _stateMachine = new CursorStateMachine(this);
            MoveToIndex(initialIndex);
        }

        private void Interact()
        {
            if (!_active || _levelFinished) return;
            var validPosition = GridManager.GetSingleton().CheckGridPosition(index, out var gridUnit);
            if (!validPosition)
            {
                InvalidPosition();
                return;
            }

            _stateMachine.InteractOnUnit(gridUnit);
        }

        private void Move(Vector2 direction)
        {
            if (!_active || _levelFinished) return;
            var newIndex = new Vector2Int(index.x + (int)direction.x, index.y + (int)direction.y);
            MoveToIndex(newIndex);
        }

        public void MoveToIndex(Vector2Int newIndex, bool animate = true)
        {
            if (_movingAnimation) return;
            if (newIndex.x == index.x && newIndex.y == index.y) return;
            var validPosition = GridManager.GetSingleton().CheckGridPosition(newIndex, out var gridUnit);
            if (!validPosition) InvalidPosition();
            _movingAnimation = true;
            if (animate)
            {
                transform.DOMove(gridUnit.transform.position, 0.1f).OnComplete(() =>
                {
                    _movingAnimation = false;
                    index = gridUnit.Index();
                });
            }
            else
            {
                transform.position = gridUnit.transform.position;
                _movingAnimation = false;
                index = gridUnit.Index();
            }
        }

        private void InvalidPosition()
        {
            DebugUtils.DebugLogWarningMsg($"{name} tried to move to an invalid position.");
        }

        public void SetSelectedActor(NavalActor navalActor)
        {
            cursorAnimator.SetBool(Select, true);
            _selectedActor = navalActor;
        }

        public void ShowOptionsForSelectedActor()
        {
            //Show the options; for now, just show the valid positions
            //Grid Manager show the valid positions for this naval actor
            var type = _selectedActor.NavalType;
            switch (type)
            {
                case NavalActorType.Player:
                {
                    ShowWalkablePathForUnit();
                    //TODO
                    var isCurrentTurnPlayer = LevelController.GetSingleton().IsCurrentActor(_selectedActor);
                    navalShipOptionsPanel.ShowOptions(isCurrentTurnPlayer);
                }
                    break;
                case NavalActorType.Enemy:
                    ShowWalkablePathForUnit();
                    navalShipOptionsPanel.ShowOptions(false);
                    break;
                case NavalActorType.Collectable:
                case NavalActorType.Obstacle:
                case NavalActorType.Wave:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return;

            void ShowWalkablePathForUnit()
            {
                if (_selectedActor is not NavalShip navalShip) return;
                ResetWalkableUnits();
                _walkableUnits = GridManager.GetSingleton()
                    .GetGridUnitsInRadiusManhattan(index, navalShip.RemainingSteps);
                _walkableUnits.ForEach(unit => { unit.DisplayWalkingVisuals(); });
            }
        }

        public void CommandToMoveSelectedActor()
        {
            cursorAnimator.SetBool(Select, false);
            _stateMachine.ChangeStateTo(CursorState.Moving);
        }

        public bool SelectedActorCanAttack()
        {
            return _selectedActor is NavalShip navalShip && navalShip.CanAct();
        }

        public void CommandToDisplayAttackArea()
        {
            cursorAnimator.SetBool(Select, false);
            ResetWalkableUnits();

            if (_selectedActor is not NavalShip navalShip) return;
            var cannon = navalShip.NavalCannon;
            var cannonData = cannon.GetCannonSo;
            _walkableUnits = GridManager.GetSingleton().GetGridUnitsForMoveType(cannonData.targetAreaType, index,
                cannonData.area, cannonData.deadZone);
            _walkableUnits.ForEach(unit =>
            {
                unit.DisplayTargetingVisuals();
                var unitActor = unit.GetActor();
                if (unitActor == null) return;
                unitActor.ShowTarget();
            });
            _stateMachine.ChangeStateTo(CursorState.Targeting);
        }

        public void HideAttackArea()
        {
            cursorAnimator.SetBool(Select, false);
            _walkableUnits.ForEach(unit =>
            {
                unit.HideVisuals();
                var unitActor = unit.GetActor();
                if (unitActor == null) return;
                unitActor.HideTarget();
            });

            ResetWalkableUnits();
        }

        public bool TargetSelectedGridUnit(GridUnit gridUnit)
        {
            var enumerator = gridUnit.GetActorEnumerator();
            var attackHappened = false;
            while (enumerator.MoveNext())
            {
                var targetActor = enumerator.Current;
                if (targetActor == null)
                {
                    DebugUtils.DebugLogMsg($"Grid unit {gridUnit.Index()} has no valid target actor.",
                        DebugUtils.DebugType.Error);
                    continue;
                }

                if (_selectedActor == null)
                {
                    DebugUtils.DebugLogMsg($"Selected actor {index} is not valid (null).", DebugUtils.DebugType.Error);
                    continue;
                }

                if (_selectedActor is not NavalShip navalShip)
                {
                    DebugUtils.DebugLogMsg($"Targeting selected actor is not a Naval Ship {_selectedActor.name}.",
                        DebugUtils.DebugType.Error);
                    continue;
                }

                if (navalShip.CanAct())
                {
                    DebugUtils.DebugLogMsg($"Act upon {targetActor.name}!", DebugUtils.DebugType.Verbose);
                    var damage = navalShip.CalculateDamage();
                    targetActor.TakeDamage(damage);
                    attackHappened = true;
                }
                else
                {
                    DebugUtils.DebugLogMsg($"Selected actor {navalShip.name} has no actions left.",
                        DebugUtils.DebugType.Error);
                }
            }

            enumerator.Dispose();
            HideAttackArea();
            if (attackHappened && _selectedActor is NavalShip selectedNavalShip)
            {
                selectedNavalShip.TryToAct();
            }

            return attackHappened;
        }

        /// <summary>
        /// Move to the given GridUnit.
        /// Checks if the movement is valid by either checking if the movement is towards the GridUnit the Actor is
        /// already at, and also check if the given GridUnit is not Blocked and belongs to the walkable list.
        /// </summary>
        /// <param name="gridUnit"></param>
        /// <param name="onFinish"></param>
        /// <returns>True if the movement is done. False if there is no movement (moving to the current grid unit) or
        /// the movement is invalid (blocked or outside the walkable list).</returns>
        public bool MoveSelectedActorTo(GridUnit gridUnit, Action<GridUnit> onFinish)
        {
            if (gridUnit.Equals(_selectedActor.GetUnit()))
            {
                DebugUtils.DebugLogMsg($"{name} moving to its own position. No movement done.",
                    DebugUtils.DebugType.Verbose);
                onFinish?.Invoke(null);
                return false;
            }

            if (gridUnit.Type() != GridUnitType.Blocked && _walkableUnits.Contains(gridUnit))
            {
                var moveTo = _selectedActor.MoveTo(gridUnit, finalGridUnit =>
                    {
                        if (finalGridUnit != null)
                        {
                            onFinish?.Invoke(finalGridUnit);
                            //Only change the state after completing the move and proceeding with the finalState calculation.
                            _stateMachine.ChangeStateTo(CursorState.ShowingOptions);
                        }
                        else
                        {
                            onFinish?.Invoke(null);
                        }
                    },
                    true, 0.15f);
                return moveTo;
            }

            DebugUtils.DebugLogMsg($"{name} cannot move to {gridUnit.name}. Not in the Walkable List or it is Blocked.",
                DebugUtils.DebugType.Error);
            onFinish?.Invoke(null);
            return false;
        }

        public void CancelSelectedActor()
        {
            if (_selectedActor == null) return;
            _selectedActor = null;
            cursorAnimator.SetBool(Select, false);
            _stateMachine.ChangeStateTo(CursorState.Roaming);
        }

        public void HideOptionsPanel()
        {
            navalShipOptionsPanel.gameObject.SetActive(false);
        }

        public void ResetWalkableUnits()
        {
            if (_walkableUnits == null) return;
            _walkableUnits.ForEach(unit => { unit.HideVisuals(); });
            _walkableUnits = null;
        }

        public void ToggleActive(bool toggle)
        {
            _active = toggle;
        }

        public void FinishLevel()
        {
            _levelFinished = true;
            cursorAnimator.SetTrigger(Finish);
            _stateMachine.FinishStateMachine();
            RemoveControllerCallbacks();
        }

        public Vector2Int GetIndex() => index;
        public NavalActor GetSelectedActor() => _selectedActor;
        public bool IsActive() => _active;
        public CursorState GetState() => _stateMachine?.CurrentState ?? CursorState.Roaming;
    }
}