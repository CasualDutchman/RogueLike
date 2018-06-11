using UnityEngine;

//used for everything that can be attacked
public interface IAttackable {

    bool Damage(int f);
    AudioClip GetHitClip();
}
