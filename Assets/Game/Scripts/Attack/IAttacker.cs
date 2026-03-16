using System.Collections.Generic;
using UnityEngine;

namespace BelgianAI
{
    public interface IAttacker
    {
        int GridWeight { get; }
        IReadOnlyList<Attack> AvailableAttacks { get; }

        Attack CurrentAttack { get; }
        void SetCurrentAttack(Attack attack);
        void OnSlotAssigned(GridSlot slot);
        void OnSlotReleased();
        Vector3 CurrentPosition { get; }
    }
}