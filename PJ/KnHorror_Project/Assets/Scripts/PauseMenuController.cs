using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject pauseMenuUI; // ลาก Pause Menu ของแม่มาใส่ตรงนี้จ่ะ

    [Header("Input Action")]
    public InputActionReference escAction; // ลาก Action ปุ่ม ESC จาก Input Action Asset มาใส่

    private bool isPaused = false;

    private void OnEnable()
    {
        if (escAction != null)
        {
            escAction.action.Enable();
            escAction.action.performed += OnPausePerformed;
        }
    }

    private void OnDisable()
    {
        if (escAction != null)
        {
            escAction.action.performed -= OnPausePerformed;
            escAction.action.Disable();
        }
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;

        // เปิดหน้าต่าง Pause Menu
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);

        // หยุดเวลาในเกม (ถ้าอยากให้หยุดนะแม่)
        Time.timeScale = 0f;

        // ✨ ปลดล็อกเมาส์และโชว์เคอร์เซอร์
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        isPaused = false;

        // ปิดหน้าต่าง Pause Menu
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        // กลับมาเดินเวลาปกติ
        Time.timeScale = 1f;

        // ✨ ล็อกเมาส์กลับไปซ่อนตามเดิม (เหมาะกับแนวเดินยิง/บุคคลที่ 3)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}