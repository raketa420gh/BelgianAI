namespace BelgianAI
{
    public interface IStageManager
    {
        public void Initialize();
        bool RequestSlot(IAttacker attacker);
        void ReleaseSlot(IAttacker attacker);
        bool RequestAttack(IAttacker attacker, Attack attack);
        void ReleaseAttack(IAttacker attacker);
        void UpdateAssignments();
    }
}