using UnityEngine;
using UnityEngine.UI;

namespace Views
{
    public class PlayerView : MonoBehaviour
    {
        public Text TextHealth;

        public Image ImageSelectedWeapon;

        public Sprite[] SpritesSelectedWeapon;

        public void UpdateHealth(int health)
        {
            TextHealth.text = "HP: " + health;
        }

        public void UpdateSelectedWeaponIndex(int selectedWeaponIndex)
        {
            ImageSelectedWeapon.sprite = SpritesSelectedWeapon[selectedWeaponIndex];
        }
    }
}