using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UUtils;

namespace Core
{
    public class PlayerController : WeakSingleton<PlayerController>
    {
        [SerializeField] private PlayerInput input;

        public Action<Vector2> onMoveAction;
        public Action onInteract;

        public void OnMove(InputAction.CallbackContext context)
        {
            var readValue = context.ReadValue<Vector2>();
            onMoveAction?.Invoke(readValue);
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            onInteract?.Invoke();
        }
        
        public void OnNavigate(InputAction.CallbackContext context)
        {
            // var readValue = context.ReadValue<Vector2>();
            // onMoveAction?.Invoke(readValue);
        }
    }
}