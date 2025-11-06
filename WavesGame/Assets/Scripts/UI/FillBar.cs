using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;
using UUtils;

namespace UI
{
    public class FillBar : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer fillBar;
        [SerializeField, ReadOnly] private Material fillBarMaterial;
        [SerializeField] private string materialProperty = "_FillFactor";
        private Tweener _animateMaterial;

        private void Awake()
        {
            AssessUtils.CheckRequirement(ref fillBar, this);
            fillBarMaterial = fillBar.material;
        }

        public void SetFillFactor(float factor, float speedFactor)
        {
            _animateMaterial?.Kill();
            _animateMaterial = AnimateMaterialProperty.AnimateProperty(fillBarMaterial, materialProperty, factor, 1.0f * speedFactor);
        }
    }
}