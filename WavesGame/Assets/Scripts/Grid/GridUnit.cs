/*
 * Copyright (c) 2025 Yvens R Serpa [https://github.com/YvensFaos/]
 * 
 * This work is licensed under the Creative Commons Attribution 4.0 International License.
 * To view a copy of this license, visit http://creativecommons.org/licenses/by/4.0/
 * or see the LICENSE file in the root directory of this repository.
 */

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UUtils;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Grid
{
    public class GridUnit : MonoBehaviour
    {
        [SerializeField] private GridUnitType originalType;
        [SerializeField] private GridUnitType currentType;
        [SerializeField] private Vector2Int index;
        [SerializeField] private SpriteRenderer spriteRenderer;
        private HashSet<GridActor> _actors;

        private void Awake()
        {
            _actors = new HashSet<GridActor>();
        }

        private void Start()
        {
            GridManager.GetSingleton()?.AddGridUnit(this);
        }

        public void DisplayWalkingVisuals()
        {
            spriteRenderer.gameObject.SetActive(true);
            ChangeSprite(GridManager.GetSingleton().GetSpriteForType(currentType));
        }

        public void DisplayTargetingVisuals()
        {
            spriteRenderer.gameObject.SetActive(true);
            ChangeSprite(GridManager.GetSingleton().GetSpriteForType(GridUnitType.Moveable));
        }

        public void HideVisuals()
        {
            spriteRenderer.gameObject.SetActive(false);
        }

        public void AddActor(GridActor actor)
        {
            _actors.Add(actor);
            actor.SetUnit(this);
            if (currentType == GridUnitType.Moveable && actor.BlockGridUnit)
            {
                UpdateType(GridUnitType.Blocked);
            }
        }

        public void RemoveActor(GridActor actor)
        {
            _actors.Remove(actor);
            actor.SetUnit(null);
            if (_actors.Count == 0)
            {
                UpdateType(originalType);
            }
            else
            {
                var stillBlocked = _actors.Any(currentActor => currentActor.BlockGridUnit);
                if (!stillBlocked)
                {
                    UpdateType(originalType);
                }
            }
        }

        private void UpdateType(GridUnitType type)
        {
            currentType = type;
            ChangeSprite(GridManager.GetSingleton().GetSpriteForType(currentType));
        }

        private void ChangeSprite(Sprite sprite)
        {
            spriteRenderer.sprite = sprite;
#if UNITY_EDITOR
            EditorGUIUtility.SetIconForObject(gameObject, sprite.texture);
#endif
        }

        public GridActor GetActor()
        {
            var enumerator = _actors.GetEnumerator();
            enumerator.MoveNext();
            var current = enumerator.Current;
            enumerator.Dispose();
            return current;
        }

        public List<GridActor> GetHasStepEffectActors()
        {
            var actorsWithStepEffect = new List<GridActor>();
            var enumerator = _actors.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var enumeratorCurrent = enumerator.Current;
                if (enumeratorCurrent != null && enumeratorCurrent.HasStepEffect)
                {
                    actorsWithStepEffect.Add(enumeratorCurrent);
                }
            }
            enumerator.Dispose();
            return actorsWithStepEffect;
        }

        /// <summary>
        /// Returns how many actors were destroyed by the given attack.
        /// </summary>
        /// <param name="damage"></param>
        /// <returns></returns>
        public int DamageActors(int damage)
        {
            return _actors.Count(gridActor => gridActor.TakeDamage(damage));
        }

        public override string ToString()
        {
            return $"{name} {Index()}";
        }

        /// <summary>
        /// Calculates the Manhattan distance between this and another grid unit.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public float DistanceTo(GridUnit other)
        {
            DebugUtils.DebugLogMsg($"Calculating {name}[{index}] distance to {other.name}[{other.Index()}].", DebugUtils.DebugType.Verbose);
            return Mathf.Abs(other.Index().x - index.x) + Mathf.Abs(other.Index().y - index.y);
        }

        public static float Distance(GridUnit a, GridUnit b)
        {
            return a.DistanceTo(b);
        }

        public GridUnitType Type() => currentType;
        public void SetIndex(Vector2Int newIndex) => index = newIndex;
        public Vector2Int Index() => index;
        public bool HasValidActors() => _actors != null;
        public int ActorsCount() => _actors.Count;
        [MustDisposeResource]
        public HashSet<GridActor>.Enumerator GetActorEnumerator() => _actors.GetEnumerator();
    }
}