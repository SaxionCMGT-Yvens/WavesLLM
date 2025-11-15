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
        [SerializeField] private bool destructible = true;
        [SerializeField] private bool blockGridUnit;
        [SerializeField] private bool hasStepEffect;

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
            if (!destructible) return;
            
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

        public virtual void MoveTo(GridUnit unit, Action<GridUnit> onFinishMoving, bool animate = false, float time = 0.5f)
        {
            UpdateGridUnitOnMovement(unit);

            if (animate)
            {
                transform.DOMove(unit.transform.position, time).OnComplete(() => { onFinishMoving?.Invoke(unit); });
            }
            else
            {
                transform.position = unit.transform.position;
                onFinishMoving?.Invoke(unit);
            }
        }

        protected void UpdateGridUnitOnMovement(GridUnit unit)
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

        /// <summary>
        /// Applies any sort of effect to the GridActor that steps on this while moving.
        /// </summary>
        /// <returns>Returns the effects of stepping on this actor.</returns>
        public virtual GridStepEffectResult StepEffect(GridActor stepper)
        {
            return new GridStepEffectResult(true, null, false, 0);
        }

        public bool BlockGridUnit => blockGridUnit;
        public bool HasStepEffect => hasStepEffect;
        public GridUnit GetUnit() => currentUnit;
        public void SetUnit(GridUnit unit) => currentUnit = unit;
        public int GetMaxHealth() => maxHealth;
        public int GetCurrentHealth() => currentHealth;
        public float GetHealthRatio() => (float) currentHealth / maxHealth;
    }
}