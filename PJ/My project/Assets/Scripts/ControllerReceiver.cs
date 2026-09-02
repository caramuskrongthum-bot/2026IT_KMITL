using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ControllerReceiver : MonoBehaviour
{
    [Header("A Button Events")]
    public UnityEvent OnClickA;
    public UnityEvent OnHold2SecA;

    [Header("B Button Events")]
    public UnityEvent OnClickB;
    public UnityEvent OnHold2SecB;

    [Header("X Button Events")]
    public UnityEvent OnClickX;
    public UnityEvent OnHold2SecX;

    [Header("Y Button Events")]
    public UnityEvent OnClickY;
    public UnityEvent OnHold2SecY;

    [Header("Motion Sensor Data")]
    public Vector3 gyroData;

    // ตัวแปรสำหรับเก็บ Coroutine การกดค้างของแต่ละปุ่ม
    private Dictionary<string, Coroutine> holdCoroutines = new Dictionary<string, Coroutine>();
    private const float HOLD_DURATION = 2.0f; // เวลาที่ต้องกดค้าง (วินาที)

    public void OnDataReceived(string message)
    {
        // 1. จัดการข้อมูลการกดปุ่ม (เช่น ACTION_A_DOWN, ACTION_A_UP, ACTION_A)
        if (message.StartsWith("ACTION_"))
        {
            string action = message.Replace("ACTION_", "");
            HandleButtonInput(action);
            return;
        }

        // 2. จัดการข้อมูล Motion Sensor
        string[] data = message.Split(',');
        if (data.Length >= 3)
        {
            if (float.TryParse(data[0], out float x) &&
                float.TryParse(data[1], out float y) &&
                float.TryParse(data[2], out float z))
            {
                gyroData = new Vector3(x, y, z);
            }
        }
    }

    private void HandleButtonInput(string action)
    {
        // รองรับแบบกดแล้วปล่อยทันที (ถ้า Controller ส่งมาแค่ ACTION_A)
        if (action == "A" || action == "B" || action == "X" || action == "Y")
        {
            TriggerClick(action);
            return;
        }

        // กรณีส่งมาเป็น DOWN และ UP (สำหรับเช็กกดค้าง 2 วินาที)
        if (action.EndsWith("_DOWN"))
        {
            string button = action.Replace("_DOWN", "");
            OnButtonDown(button);
        }
        else if (action.EndsWith("_UP"))
        {
            string button = action.Replace("_UP", "");
            OnButtonUp(button);
        }
    }

    private void OnButtonDown(string button)
    {
        // ถ้ามีการกดค้างปุ่มเดิมอยู่ให้ยกเลิกก่อน
        if (holdCoroutines.ContainsKey(button) && holdCoroutines[button] != null)
        {
            StopCoroutine(holdCoroutines[button]);
        }

        // เริ่มนับเวลา 2 วินาที
        holdCoroutines[button] = StartCoroutine(HoldRoutine(button));
    }

    private void OnButtonUp(string button)
    {
        // ถ้าปล่อยปุ่มก่อนครบ 2 วินาที (Coroutine ยังทำงานอยู่) แสดงว่าเป็นการกดครั้งเดียว (Click)
        if (holdCoroutines.ContainsKey(button) && holdCoroutines[button] != null)
        {
            StopCoroutine(holdCoroutines[button]);
            holdCoroutines[button] = null;

            TriggerClick(button);
        }
    }

    private IEnumerator HoldRoutine(string button)
    {
        yield return new WaitForSeconds(HOLD_DURATION);

        // เมื่อกดค้างครบ 2 วินาที จะเรียก Hold Event และเคลียร์ตัวนับเวลา
        TriggerHold(button);
        holdCoroutines[button] = null;
    }

    private void TriggerClick(string button)
    {
        switch (button)
        {
            case "A": OnClickA?.Invoke(); break;
            case "B": OnClickB?.Invoke(); break;
            case "X": OnClickX?.Invoke(); break;
            case "Y": OnClickY?.Invoke(); break;
        }
    }

    private void TriggerHold(string button)
    {
        switch (button)
        {
            case "A": OnHold2SecA?.Invoke(); break;
            case "B": OnHold2SecB?.Invoke(); break;
            case "X": OnHold2SecX?.Invoke(); break;
            case "Y": OnHold2SecY?.Invoke(); break;
        }
    }
}