using UnityEngine;

namespace BelgianAI
{
    public interface IGridIntity : IWeighted
    {
        int GridSlotIndex { get; set; }
        Vector3 DesiredPosition { get; }
        bool IsLocked { get; set; }
    }
}