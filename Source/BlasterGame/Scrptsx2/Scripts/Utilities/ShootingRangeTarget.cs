using UnityEngine;
using System.Collections;

public class ShootingRangeTarget : MonoBehaviour {

    public Animator anim;

    public void HitTarget()
    {
        anim.SetBool("Down", true);
    }
}
