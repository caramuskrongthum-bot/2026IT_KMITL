using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class EscapeMenuPause : MonoBehaviour
{
    public InputAction inputAction;
    public GameObject pauseMenu;

    [SerializeField] private bool isPaused;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        UpdatePauseState();
    }

    private void OnEnable()
    {
        inputAction.Enable();
    }

    private void OnDisable()
    {
        inputAction.Disable();
    }

    private void Update()
    {
        if (inputAction.WasPressedThisFrame())
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        UpdatePauseState();
    }

    public void SetPause(bool state)
    {
        isPaused = state;
        UpdatePauseState();
    }

    public void BackToMenu()
    {
        SetPause(false); // Make sure time Scale resets to 1f before changing scene
        SceneManager.LoadScene("Menu");
    }

    private void UpdatePauseState()
    {
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(isPaused);
        }

        Time.timeScale = isPaused ? 0f : 1f;
    }
}