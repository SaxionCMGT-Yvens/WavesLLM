using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
            GridManager.GetSingleton().AddGridUnit(this);
        }

        public void DisplayWalkingVisuals()
        {
            spriteRenderer.gameObject.SetActive(true);
            spriteRenderer.sprite = GridManager.GetSingleton().GetSpriteForType(currentType);
        }

        public void HideWalkingVisuals()
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
#if UNITY_EDITOR
            var sprite = GridManager.GetSingleton().GetSpriteForType(currentType);
            EditorGUIUtility.SetIconForObject(gameObject, sprite.texture);
#endif
        }

        public GridActor GetActor()
        {
            var enumerator = _actors.GetEnumerator();
            enumerator.MoveNext();
            return enumerator.Current;
        }

        public GridUnitType Type() => currentType;
        public void SetIndex(Vector2Int newIndex) => index = newIndex;
        public Vector2Int Index() => index;
        public bool HasValidActors() => _actors != null;
        public int ActorsCount() => _actors.Count;
        public HashSet<GridActor>.Enumerator GetActorEnumerator() => _actors.GetEnumerator();
    }
}