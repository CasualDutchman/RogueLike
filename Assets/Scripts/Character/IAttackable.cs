using UnityEngine;

public interface IAttackable {

    bool Damage(int f);
    AudioClip GetHitClip();
}
