using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance { get; private set; }

    public MechStatsSO[] mechSelections;
    public GameObject[] weaponPrefabsRight;
    public GameObject[] weaponPrefabsLeft;
    public int selectedMech = 0;
    public int selectedWeaponR = 0;       
    public int selectedWeaponL = 0;
    public bool altWeaponR = false;
    public bool altWeaponL = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
}