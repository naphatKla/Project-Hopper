using UnityEngine;

public interface ISpawnOption
{
    int Weight { get; }
    int Chance { get; }

    bool TryPassChance();
}
