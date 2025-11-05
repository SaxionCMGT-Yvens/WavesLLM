using Core;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UUtils;

namespace UI
{
    public class PlayerNavalShipOptionsPanel : MonoBehaviour
    {
        [SerializeField] private CursorController cursorController;
        [SerializeField] private RectTransform selfRectTransform;
        [SerializeField] private Button initialButton;

        private TweenerCore<Vector3, Vector3, VectorOptions> _showUpTween;
        private bool _introAnimation;
        
        private void Awake()
        {
            AssessUtils.CheckRequirement(ref cursorController, this);
            AssessUtils.CheckRequirement(ref selfRectTransform, this);
        }

        private void OnEnable()
        {
            selfRectTransform.localScale = Vector3.zero;
            EventSystem.current.SetSelectedGameObject(initialButton.gameObject);
            _introAnimation = true;
            _showUpTween = selfRectTransform.DOScale(Vector3.one, 0.3f).OnComplete(() =>
            {
                PlayerController.GetSingleton().onCancel += Cancel;
                _introAnimation = false;
            });
        }

        private void OnDisable()
        {
            _showUpTween?.Kill();
            PlayerController.GetSingleton().onCancel -= Cancel;
            // TODO - check if necessary again in the future 
            // EventSystem.current.SetSelectedGameObject(null); 
        }

        public void Move()
        {
            if (!CheckValidState()) return;
            cursorController.CommandToMoveSelectedActor();
        }

        public void Attack()
        {
            if (!CheckValidState()) return;
            cursorController.CommandToDisplayAttackArea();
        }

        public void Info()
        {
        }

        public void Cancel()
        {
            if (!CheckValidState()) return;
            cursorController.CancelSelectedActor();
        }

        private bool CheckValidState()
        {
            if (!gameObject.activeInHierarchy) return false;
            return !_introAnimation;
        }
    }
}