using UnityEngine;

namespace BelgianAI
{
    public interface IStageManager
    {
        float OuterRadius { get; }
        float InnerRadius { get; }
        Vector3 PlayerPosition { get; }

        bool RequestSlot(IAttacker attacker);
        void ReleaseSlot(IAttacker attacker);
        bool RequestAttack(IAttacker attacker, Attack attack);
        void ReleaseAttack(IAttacker attacker);
        Vector3 GetWaitPosition(IAttacker attacker);
    }
}