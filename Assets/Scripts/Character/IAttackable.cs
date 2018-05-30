using UnityEngine;

public interface IAttackable {

    void Damage(float f);
    AudioClip GetHitClip();
}
