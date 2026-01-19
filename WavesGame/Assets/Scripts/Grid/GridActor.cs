/*
 * Copyright (c) 2025 Yvens R Serpa [https://github.com/YvensFaos/]
 * 
 * This work is licensed under the Creative Commons Attribution 4.0 International License.
 * To view a copy of this license, visit http://creativecommons.org/licenses/by/4.0/
 * or see the LICENSE file in the root directory of this repository.
 */

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

        protected bool markedForDeath;

        protected virtual void Start()
        {
            SetUnit(GridManager.GetSingleton().GetGridPosition(transform));
            //Adjust position to match the grid precisely
            var gridUnit = GetUnit();
            transform.position = gridUnit.transform.position;
            gridUnit.AddActor(this);
            currentHealth = maxHealth;
        }

        public virtual bool TakeDamage(int damage)
        {
            if (!destructible) return false;
            
            currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);
            if (currentHealth <= 0)
            {
                DestroyActor();
            }
            return currentHealth <= 0;
        }

        protected virtual void DestroyActor()
        {
            markedForDeath = true;
            DebugUtils.DebugLogMsg($"Destroying actor {name}.", DebugUtils.DebugType.System);
            currentUnit.RemoveActor(this);
            Destroy(gameObject);
        }

        public virtual bool MoveTo(GridUnit unit, Action<GridUnit> onFinishMoving, bool animate = false, float time = 0.5f)
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

            return true;
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
        public bool IsMarkedForDeath() => markedForDeath;
        
        public override string ToString()
        {
            return $"{name}; currentUnit=[{currentUnit.Index().x}, {currentUnit.Index().y}]; maxHealth={maxHealth}; currentHealth={currentHealth}; ratio={GetHealthRatio()}; blockGridUnit={blockGridUnit}; hasStepEffect={hasStepEffect}; markedForDeath={markedForDeath};";
        }
    }
}