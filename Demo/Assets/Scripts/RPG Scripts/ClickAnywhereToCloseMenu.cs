using UnityEngine;
using UnityEngine.SceneManagement;

public class ClickAnywhereToCloseMenu : MonoBehaviour
{
    [Tooltip("The GameObject that contains the main menu UI. If left empty, the script uses the object it is attached to.")]
    [SerializeField] private GameObject menuPanel;

    [Tooltip("Only hide the menu when the First Level scene is active.")]
    [SerializeField] private bool onlyOnFirstLevel = true;

    [Tooltip("Scene name to check when onlyOnFirstLevel is enabled.")]
    [SerializeField] private string firstLevelSceneName = "First Level";

    private void Reset()
    {
        if (menuPanel == null)
        {
            menuPanel = gameObject;
        }
    }

    private void Update()
    {
        if (menuPanel == null || !menuPanel.activeInHierarchy)
            return;

        if (onlyOnFirstLevel && SceneManager.GetActiveScene().name != firstLevelSceneName)
            return;

        if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
        {
            menuPanel.SetActive(false);
        }
    }
}
