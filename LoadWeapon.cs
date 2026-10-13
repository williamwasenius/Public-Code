using TMPro;
using UnityEngine;

public class LoadWeapon : MonoBehaviour
{
    public Transform spawnPointWPNR;
    public Transform spawnPointWPNL;

    void Start()
    {
    int selectedWeapon1 = SelectionManager.Instance.selectedWeaponR;
    int selectedWeapon2 = SelectionManager.Instance.selectedWeaponL;

    if (selectedWeapon1 < 0 || selectedWeapon1 >= SelectionManager.Instance.weaponPrefabsRight.Length)
        {
            selectedWeapon1 = 0;
            SelectionManager.Instance.selectedWeaponR = 0;
        }

    if (selectedWeapon2 < 0 || selectedWeapon2 >= SelectionManager.Instance.weaponPrefabsLeft.Length)
        {
            selectedWeapon2 = 0;
            SelectionManager.Instance.selectedWeaponL = 0;
        }

    GameObject prefab1 = SelectionManager.Instance.weaponPrefabsRight[selectedWeapon1];
    GameObject prefab2 = SelectionManager.Instance.weaponPrefabsLeft[selectedWeapon2];

    GameObject clone1 = Instantiate(prefab1, spawnPointWPNR.position, Quaternion.identity, spawnPointWPNR);
    GameObject clone2 = Instantiate(prefab2, spawnPointWPNL.position, Quaternion.identity, spawnPointWPNL);

    clone1.transform.localPosition = Vector3.zero;
    clone1.transform.localRotation = Quaternion.identity;
    if (SelectionManager.Instance.altWeaponR)
        {
            clone1.GetComponent<WeaponHandler>().altWeapon = true;
        }

    clone2.transform.localPosition = Vector3.zero;
    clone2.transform.localRotation = Quaternion.identity;
    if (SelectionManager.Instance.altWeaponL)
        {
            clone2.GetComponent<WeaponHandler>().altWeapon = true;
        }
    }

}
