using UnityEngine;
using TMPro;

public class PlayerAmmo : MonoBehaviour
{
    [Header("Ammo Settings")]
    public int currentAmmo = 20;
    public int maxAmmo = 67;

    [Header("UI")]
    public TextMeshProUGUI ammoText;

    private void Start()
    {
        UpdateAmmoUI();
    }

    public bool CanShoot()
    {
        return currentAmmo > 0;
    }

    public void ConsumeAmmo()
    {
        if (currentAmmo > 0)
        {
            currentAmmo--;
            UpdateAmmoUI();
        }
    }

    public void AddAmmo(int amount)
    {
        currentAmmo = Mathf.Clamp(currentAmmo + amount, 0, maxAmmo);
        UpdateAmmoUI();
    }

    private void UpdateAmmoUI()
    {
        if (ammoText != null)
        {
            ammoText.text = $"Ammo: {currentAmmo} / {maxAmmo}";
        }
    }
}