using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class GamepadCursorUI : MonoBehaviour
{
    [Header("Cursor")]
    public RectTransform cursor;
    public Canvas canvas;
    public float moveSpeed = 1000f;

    [Header("Input")]
    public InputActionReference moveAction;
    public InputActionReference submitAction;

    [Header("UI")]
    public List<Button> buttons = new List<Button>();

    [Header("Optional Canvas Check")]
    public GameObject targetCanvas;

    private Button currentButton;

    void Start()
    {
        InputSystem.settings.updateMode =
            InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;

        cursor.gameObject.SetActive(false);
        cursor.anchoredPosition = Vector2.zero;
    }

    void OnEnable()
    {
        moveAction.action.Enable();
        submitAction.action.Enable();

        cursor.gameObject.SetActive(false);
        cursor.anchoredPosition = Vector2.zero;
    }

    void OnDisable()
    {
        moveAction.action.Disable();
        submitAction.action.Disable();
    }

    void Update()
    {
        // ตรวจสอบว่า Canvas เป้าหมายเปิดอยู่หรือไม่
        if (targetCanvas != null && !targetCanvas.activeInHierarchy)
        {
            if (cursor.gameObject.activeSelf) cursor.gameObject.SetActive(false);
            return;
        }

        CheckGamepadInput();

        if (!cursor.gameObject.activeSelf)
            return;

        MoveCursor();
        DetectButton();
        HandleSubmit();
    }

    void CheckGamepadInput()
    {
        Vector2 input = moveAction.action.ReadValue<Vector2>();

        if (input.magnitude > 0.1f)
        {
            if (!cursor.gameObject.activeSelf)
            {
                cursor.gameObject.SetActive(true);
                cursor.anchoredPosition = Vector2.zero;
            }
        }
    }

    void MoveCursor()
    {
        Vector2 input = moveAction.action.ReadValue<Vector2>();

        cursor.anchoredPosition +=
            input * moveSpeed * Time.unscaledDeltaTime;

        RectTransform canvasRect =
            canvas.GetComponent<RectTransform>();

        Vector2 pos = cursor.anchoredPosition;

        float halfW = canvasRect.rect.width * 0.5f;
        float halfH = canvasRect.rect.height * 0.5f;

        pos.x = Mathf.Clamp(pos.x, -halfW, halfW);
        pos.y = Mathf.Clamp(pos.y, -halfH, halfH);

        cursor.anchoredPosition = pos;
    }

    void DetectButton()
    {
        currentButton = null;

        foreach (Button btn in buttons)
        {
            if (btn == null || !btn.gameObject.activeInHierarchy || !btn.interactable)
                continue;

            RectTransform btnRect = btn.GetComponent<RectTransform>();

            if (RectTransformUtility.RectangleContainsScreenPoint(
                btnRect,
                RectTransformToScreenPoint(cursor),
                canvas.worldCamera))
            {
                currentButton = btn;

                if (EventSystem.current.currentSelectedGameObject != btn.gameObject)
                {
                    EventSystem.current.SetSelectedGameObject(btn.gameObject);
                }

                break;
            }
        }

        if (currentButton == null && EventSystem.current.currentSelectedGameObject != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    void HandleSubmit()
    {
        if (currentButton != null &&
            currentButton.gameObject.activeInHierarchy &&
            submitAction.action.WasPressedThisFrame())
        {
            currentButton.onClick.Invoke();
        }
    }

    Vector2 RectTransformToScreenPoint(RectTransform rect)
    {
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            return rect.position;
        }

        return RectTransformUtility.WorldToScreenPoint(
            canvas.worldCamera,
            rect.position
        );
    }
}