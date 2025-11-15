using System;

namespace Grid
{
    [Serializable]
    public struct GridStepEffectResult
    {
        public bool canContinueMovement;
        public GridUnit moveTo;
        public bool causeDamage;
        public int damage;

        public GridStepEffectResult(bool canContinueMovement, GridUnit moveTo, bool causeDamage, int damage)
        {
            this.canContinueMovement = canContinueMovement;
            this.moveTo = moveTo;
            this.causeDamage = causeDamage;
            this.damage = damage;
        }
    }
}