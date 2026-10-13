using UnityEngine;
using UnityEngine.Animations.Rigging;

public class FetchMechComponentsScript : MonoBehaviour
{

    public GameObject lowerTorso;
    public GameObject upperTorso;
    public GameObject aimPivot;
    public RigBuilder rig;
    public MultiAimConstraint multiAC;

    public GameObject weaponR;
    public GameObject weaponL;

    public Animator animator;

    void Start()
    {
        if (lowerTorso == null)
        {
            lowerTorso = GameObject.Find("LowerTorso");
        }
        if (upperTorso == null)
        {
            upperTorso = GameObject.Find("UpperTorso");
        }
        if (aimPivot == null)
        {
            aimPivot = GameObject.Find("AimPivot");
        }
        if (weaponR == null)
        {
            weaponR = GameObject.Find("mech_01_core_hardpoint_R");
        }
        if (weaponL == null)
        {
            weaponL = GameObject.Find("mech_01_core_hardpoint_L");
        }
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

}
