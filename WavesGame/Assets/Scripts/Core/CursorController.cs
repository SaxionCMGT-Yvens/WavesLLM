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

        private static readonly int Select = Animator.StringToHash("Select");

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
            if (!_active) return;
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
            if (!_active) return;
            var newIndex = new Vector2Int(index.x + (int)direction.x, index.y + (int)direction.y);
            MoveToIndex(newIndex);
        }

        public void MoveToIndex(Vector2Int newIndex)
        {
            if (_movingAnimation) return;
            if (newIndex.x == index.x && newIndex.y == index.y) return;
            var validPosition = GridManager.GetSingleton().CheckGridPosition(newIndex, out var gridUnit);
            if (!validPosition) InvalidPosition();
            _movingAnimation = true;
            transform.DOMove(gridUnit.transform.position, 0.1f).OnComplete(() =>
            {
                _movingAnimation = false;
                index = gridUnit.Index();
            });
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
                    break;
                case NavalActorType.Obstacle:
                    break;
                case NavalActorType.Wave:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return;

            void ShowWalkablePathForUnit()
            {
                if (_selectedActor is not NavalShip navalShip) return;
                var data = navalShip.ShipData;
                ResetWalkableUnits();
                _walkableUnits = GridManager.GetSingleton()
                    .GetGridUnitsInRadiusManhattan(index, data.movementRadius);
                _walkableUnits.ForEach(unit => { unit.DisplayWalkingVisuals(); });
            }
        }

        public void CommandToMoveSelectedActor()
        {
            cursorAnimator.SetBool(Select, false);
            _stateMachine.ChangeStateTo(CursorState.Moving);
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

        public void HidAttackArea()
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
            var targetActor = gridUnit.GetActor();
            if (targetActor == null)
            {
                DebugUtils.DebugLogMsg($"Grid unit {gridUnit.Index()} has no valid target actor.", DebugUtils.DebugType.Error);
                return false;
            }
            if (_selectedActor == null)
            {
                DebugUtils.DebugLogMsg($"Selected actor {index} is not valid (null).", DebugUtils.DebugType.Error);
                return false;
            }
            if (_selectedActor is not NavalShip selectedNavalShip)
            {
                DebugUtils.DebugLogMsg($"Targeting selected actor is not a Naval Ship {_selectedActor.name}.", DebugUtils.DebugType.Error);
                return false;
            }

            var damage = selectedNavalShip.NavalCannon.GetCannonSo.damage;
            targetActor.TakeDamage(damage);
            // Camera shake and other effects
            HidAttackArea();
            return true;
        }
        
        public bool MoveSelectedActorTo(GridUnit gridUnit)
        {
            if (gridUnit.Type() != GridUnitType.Blocked && _walkableUnits.Contains(gridUnit))
            {
                _selectedActor.MoveTo(gridUnit, () => { _stateMachine.ChangeStateTo(CursorState.ShowingOptions); },
                    true, 0.15f);
                return true;
            }

            DebugUtils.DebugLogMsg($"{name} cannot move to {gridUnit.name}. Not in the Walkable List or it is Blocked.",
                DebugUtils.DebugType.Error);
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

        public bool IsActive() => _active;
        public CursorState GetState() => _stateMachine?.CurrentState ?? CursorState.Roaming;
    }
}