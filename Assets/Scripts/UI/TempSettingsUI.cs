using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TempSettingsUI : MonoBehaviour
{
    [Header("Engine References")]
    [SerializeField]
    protected MenuManager menuManager;

    [Header("Settings Menu Buttons")]
    [SerializeField]
    protected Button applyButton;
    [SerializeField]
    protected Button cancelButton;


    void Start()
    {
        applyButton.onClick.AddListener(() =>
        {
            menuManager.SetWindowState(MenuManager.WindowState.Title);
        });

        cancelButton.onClick.AddListener(() =>
        {
            menuManager.SetWindowState(MenuManager.WindowState.Title);
        });
    }
}
