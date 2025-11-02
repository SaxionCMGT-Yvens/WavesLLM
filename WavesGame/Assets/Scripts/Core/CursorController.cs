using Actors;
using DG.Tweening;
using Grid;
using NaughtyAttributes;
using UnityEngine;
using UUtils;

namespace Core
{
    public class CursorController : MonoBehaviour
    {
        [SerializeField, ReadOnly] private Vector2Int index;
        [SerializeField] private Vector2Int initialIndex;

        private CursorStateMachine _stateMachine;
        private NavalActor _selectedActor;
        private bool _moving;
        private bool _active = true;

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
            _stateMachine.UpdateState(gridUnit);
        }
        
        private void Move(Vector2 direction)
        {
            if (!_active) return;
            var newIndex = new Vector2Int(index.x + (int)direction.x, index.y + (int)direction.y);
            MoveToIndex(newIndex);
        }

        private void MoveToIndex(Vector2Int newIndex)
        {
            if (_moving) return;
            if (newIndex.x == index.x && newIndex.y == index.y) return;
            var validPosition = GridManager.GetSingleton().CheckGridPosition(newIndex, out var gridUnit);
            if(!validPosition) InvalidPosition();
            _moving = true;
            transform.DOMove(gridUnit.transform.position, 0.2f).OnComplete(() =>
            {
                _moving = false;
                index = gridUnit.Index();
            });
        }

        private void InvalidPosition()
        {
            DebugUtils.DebugLogWarningMsg($"{name} tried to move to an invalid position.");
        }

        public void SetSelectedActor(NavalActor navalActor)
        {
            _selectedActor = navalActor;
        }
        
        public void ToggleActive(bool toggle)
        {
            _active = toggle;
        }

        public void ToggleMoving(bool toggle)
        {
            _moving = toggle;
        }
        
        public CursorState GetState() => _stateMachine?.CurrentState ?? CursorState.Roaming;
    }
}