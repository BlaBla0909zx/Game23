using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIHealthBar : MonoBehaviour
{
    //slider 
    [SerializeField] private Slider slider;

    public void SetHealth(float health, float maxHealth)
    {
        slider.value = health / maxHealth;
    }
    
}
