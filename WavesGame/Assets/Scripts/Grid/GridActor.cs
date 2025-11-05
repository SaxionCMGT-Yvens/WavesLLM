using System;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;
using UUtils;

namespace Grid
{
    public class GridActor : MonoBehaviour
    {
        [Header("Data")] [SerializeField] private int maxHealth;
        [SerializeField, ReadOnly] private int currentHealth;

        [Header("References")] [SerializeField, ReadOnly]
        protected GridUnit currentUnit;

        [SerializeField] private SpriteRenderer targetRenderer;
        [SerializeField] private bool blockGridUnit;

        protected virtual void Start()
        {
            SetUnit(GridManager.GetSingleton().GetGridPosition(transform));
            //Adjust position to match the grid precisely
            var gridUnit = GetUnit();
            transform.position = gridUnit.transform.position;
            gridUnit.AddActor(this);
            currentHealth = maxHealth;
        }

        public virtual void TakeDamage(int damage)
        {
            currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);
            if (currentHealth <= 0)
            {
                DestroyActor();
            }
        }

        protected virtual void DestroyActor()
        {
            DebugUtils.DebugLogMsg($"Destroying actor {name}.", DebugUtils.DebugType.System);
            currentUnit.RemoveActor(this);
            Destroy(gameObject);
        }

        public virtual void MoveTo(GridUnit unit, Action onFinishMoving, bool animate = false, float time = 0.5f)
        {
            UpdateUnitOnMovement(unit);

            if (animate)
            {
                transform.DOMove(unit.transform.position, time).OnComplete(() => { onFinishMoving?.Invoke(); });
            }
            else
            {
                transform.position = unit.transform.position;
                onFinishMoving?.Invoke();
            }
        }

        protected void UpdateUnitOnMovement(GridUnit unit)
        {
            if (currentUnit != null)
            {
                currentUnit.RemoveActor(this);
            }

            //Adding the actor to the unit also updates the actor's current unit
            unit.AddActor(this);
        }

        public void ShowTarget()
        {
            targetRenderer.gameObject.SetActive(true);
        }

        public void HideTarget()
        {
            targetRenderer.gameObject.SetActive(false);
        }

        public bool BlockGridUnit => blockGridUnit;
        public GridUnit GetUnit() => currentUnit;
        public void SetUnit(GridUnit unit) => currentUnit = unit;
        public int GetMaxHealth() => maxHealth;
        public int GetCurrentHealth() => currentHealth;
    }
}