namespace BelgianAI
{
    public interface IStageManager
    {
        bool RequestSlot(IAttacker attacker);
        void ReleaseSlot(IAttacker attacker);
        bool RequestAttack(IAttacker attacker, Attack attack);
        void ReleaseAttack(IAttacker attacker);
        void UpdateAssignments();
    }
}