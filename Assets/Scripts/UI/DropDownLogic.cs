using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DropDownLogic : MonoBehaviour
{
    // Ссылка на TMP_Dropdown
    public TMP_Dropdown tmpDropdown;

    // UI-элементы, видимость которых будем переключать
    public GameObject panelOption1;
    public GameObject panelOption2;
    public GameObject panelOption3;

    void Start()
    {
        if (tmpDropdown != null)
        {
            // Подписываемся на событие изменения значения
            tmpDropdown.onValueChanged.AddListener(OnTMPDropdownValueChanged);
            // Инициализируем видимость UI-элементов в соответствии с текущим выбором
            OnTMPDropdownValueChanged(tmpDropdown.value);
        }
        else
        {
            Debug.LogWarning("TMP_Dropdown не задан в инспекторе!");
        }
    }

    // Метод, вызываемый при изменении значения TMP_Dropdown
    void OnTMPDropdownValueChanged(int index)
    {
        // Деактивируем все панели
        if(panelOption1 != null) panelOption1.SetActive(false);
        if(panelOption2 != null) panelOption2.SetActive(false);
        if(panelOption3 != null) panelOption3.SetActive(false);

        // В зависимости от выбранного индекса активируем нужную панель
        switch (index)
        {
            case 0:
                if (panelOption1 != null)
                    panelOption1.SetActive(true);
                break;
            case 1:
                if (panelOption2 != null)
                    panelOption2.SetActive(true);
                break;
            case 2:
                if (panelOption3 != null)
                    panelOption3.SetActive(true);
                break;
            default:
                Debug.LogWarning("Необработанный индекс TMP_Dropdown: " + index);
                break;
        }
    }
}
