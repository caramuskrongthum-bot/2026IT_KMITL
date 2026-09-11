using UnityEngine;

public class CustomSpringBone : MonoBehaviour
{
    // =========================================================
    // BONE SETUP
    // =========================================================

    [Header("Bone Setup")]
    [Tooltip("Bone ลูกของ bone นี้")]
    public Transform childBone;

    [Tooltip("แกนที่ชี้จากโคน bone ไปทางปลาย bone ใน Local Space")]
    public Vector3 boneAxis = new Vector3(0f, 1f, 0f);

    [Tooltip("ใช้ Rest Rotation ที่บันทึกตอนเริ่มเกม")]
    public bool useRestRotation = true;


    // =========================================================
    // SPRING PHYSICS
    // =========================================================

    [Header("Physics & Force")]

    [Range(0.001f, 1f)]
    [Tooltip("ความแข็งของ spring ยิ่งมากยิ่งกลับตำแหน่งเดิมเร็ว")]
    public float stiffnessForce = 0.08f;

    [Range(0.001f, 0.99f)]
    [Tooltip("แรงหน่วง ยิ่งมากยิ่งหยุดเร็ว")]
    public float dragForce = 0.35f;

    [Tooltip("แรงโน้มถ่วงใน World Space")]
    public Vector3 gravity = new Vector3(0f, -0.005f, 0f);

    [Tooltip("รัศมี collision ของปลาย bone")]
    public float boneRadius = 0.025f;


    // =========================================================
    // STABILITY
    // =========================================================

    [Header("Stability")]

    [Tooltip("FPS อ้างอิงสำหรับ tuning physics")]
    public float referenceFrameRate = 60f;

    [Range(1, 8)]
    [Tooltip("จำนวน physics substeps")]
    public int substeps = 2;

    [Range(1f, 180f)]
    [Tooltip("องศาสูงสุดที่ bone สามารถหมุนต่อเฟรม")]
    public float maxDegreesPerFrame = 35f;

    [Range(1, 8)]
    [Tooltip("จำนวนรอบแก้ collision")]
    public int collisionIterations = 2;


    // =========================================================
    // ROTATION LIMIT
    // =========================================================

    [Header("Rotation Limit")]

    [Tooltip("จำกัดมุมจากทิศ Rest")]
    public bool limitRotation = true;

    [Range(0f, 180f)]
    public float maxRotationAngle = 80f;


    // =========================================================
    // COLLISION
    // =========================================================

    [Header("Center Collider")]

    [Tooltip("กระดูก/Transform บริเวณ pelvis")]
    public Transform centerCollider;

    public float centerRadius = 0.15f;


    [Header("Leg Capsules")]

    public LegCapsule[] legCapsules;


    [System.Serializable]
    public struct LegCapsule
    {
        public Transform startBone;
        public Transform endBone;

        [Min(0f)]
        public float radius;
    }


    // =========================================================
    // INTERNAL STATE
    // =========================================================

    private Vector3 currentTipPos;
    private Vector3 previousTipPos;

    private float initialLength;

    // Rest local rotation ของ bone
    private Quaternion restLocalRotation;

    // Rest world rotation
    private Quaternion restWorldRotation;

    // ตำแหน่ง origin frame ก่อนหน้า
    private Vector3 previousOriginPosition;

    private bool initialized;


    // =========================================================
    // UNITY
    // =========================================================

    void Awake()
    {
        Initialize();
    }


    void Start()
    {
        if (!initialized)
        {
            Initialize();
        }
    }


    void LateUpdate()
    {
        if (!initialized)
            return;

        Simulate(Time.deltaTime);
    }


    // =========================================================
    // INITIALIZE
    // =========================================================

    private void Initialize()
    {
        if (childBone == null)
        {
            if (transform.childCount > 0)
            {
                childBone = transform.GetChild(0);
            }
            else
            {
                Debug.LogWarning(
                    $"[{name}] CustomSpringBone: ไม่มี childBone",
                    this
                );

                return;
            }
        }

        initialLength = Vector3.Distance(
            transform.position,
            childBone.position
        );

        if (initialLength <= 0.00001f)
        {
            Debug.LogWarning(
                $"[{name}] CustomSpringBone: bone length เป็น 0",
                this
            );

            return;
        }

        // Normalize axis
        if (boneAxis.sqrMagnitude < 0.000001f)
        {
            boneAxis = Vector3.up;
        }

        boneAxis.Normalize();

        // เก็บ Rest Rotation
        restLocalRotation = transform.localRotation;
        restWorldRotation = transform.rotation;

        // เริ่ม tip จากตำแหน่งจริง
        currentTipPos = childBone.position;
        previousTipPos = childBone.position;

        previousOriginPosition = transform.position;

        initialized = true;
    }


    // =========================================================
    // MAIN SIMULATION
    // =========================================================

    private void Simulate(float deltaTime)
    {
        if (deltaTime <= 0f)
            return;

        if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime))
            return;

        // จำกัด timestep กัน physics กระโดดตอน frame drop
        deltaTime = Mathf.Min(deltaTime, 1f / 20f);

        int safeSubsteps = Mathf.Clamp(substeps, 1, 8);

        float frameRatio =
            deltaTime * referenceFrameRate;

        float stepRatio =
            frameRatio / safeSubsteps;

        // -----------------------------------------------------
        // Detect origin movement
        // -----------------------------------------------------

        Vector3 originPosition = transform.position;

        Vector3 originDelta =
            originPosition - previousOriginPosition;

        // ย้าย simulated tip ตาม bone ต้นทาง
        // เพื่อไม่ให้ character เดินแล้วผมหลุดจากหัว
        if (originDelta.sqrMagnitude > 0.00000001f)
        {
            currentTipPos += originDelta;
            previousTipPos += originDelta;
        }

        previousOriginPosition = originPosition;


        // -----------------------------------------------------
        // Spring coefficients
        // -----------------------------------------------------

        float stiffnessStep =
            1f - Mathf.Pow(
                1f - Mathf.Clamp01(stiffnessForce),
                stepRatio
            );

        float dragStep =
            Mathf.Pow(
                1f - Mathf.Clamp01(dragForce),
                stepRatio
            );


        // -----------------------------------------------------
        // Simulation
        // -----------------------------------------------------

        for (int i = 0; i < safeSubsteps; i++)
        {
            SimulateStep(
                stiffnessStep,
                dragStep,
                stepRatio,
                deltaTime
            );
        }


        // -----------------------------------------------------
        // Apply Rotation
        // -----------------------------------------------------

        ApplyRotation(deltaTime, frameRatio);
    }


    // =========================================================
    // SINGLE PHYSICS STEP
    // =========================================================

    private void SimulateStep(
        float stiffnessStep,
        float dragStep,
        float stepRatio,
        float deltaTime
    )
    {
        Vector3 origin = transform.position;


        // =====================================================
        // REST TARGET
        // =====================================================

        Vector3 restDirection;

        if (useRestRotation)
        {
            // ใช้ Rest Rotation
            // ไม่ใช้ rotation ที่ simulation เพิ่งสร้าง
            restDirection =
                restWorldRotation * boneAxis;
        }
        else
        {
            restDirection =
                transform.rotation * boneAxis;
        }

        restDirection.Normalize();


        Vector3 targetPosition =
            origin +
            restDirection * initialLength;


        // =====================================================
        // VELOCITY
        // =====================================================

        Vector3 velocity =
            currentTipPos - previousTipPos;


        // =====================================================
        // SPRING
        // =====================================================

        Vector3 springForce =
            (targetPosition - currentTipPos)
            * stiffnessStep;


        // =====================================================
        // GRAVITY
        // =====================================================

        Vector3 gravityForce =
            gravity * stepRatio;


        // =====================================================
        // NEW POSITION
        // =====================================================

        Vector3 nextTipPos =
            currentTipPos
            + velocity * dragStep
            + springForce
            + gravityForce;


        // ป้องกัน NaN
        if (float.IsNaN(nextTipPos.x) ||
            float.IsNaN(nextTipPos.y) ||
            float.IsNaN(nextTipPos.z) ||
            float.IsInfinity(nextTipPos.x) ||
            float.IsInfinity(nextTipPos.y) ||
            float.IsInfinity(nextTipPos.z))
        {
            nextTipPos =
                origin +
                restDirection * initialLength;
        }


        // =====================================================
        // COLLISION + LENGTH SOLVER
        // =====================================================

        int iterations =
            Mathf.Clamp(collisionIterations, 1, 8);

        for (int i = 0; i < iterations; i++)
        {
            // รักษาความยาว bone
            nextTipPos =
                ConstrainLength(
                    nextTipPos,
                    origin
                );

            // Center / pelvis
            if (centerCollider != null)
            {
                nextTipPos =
                    ResolveSphereCollision(
                        nextTipPos,
                        centerCollider.position,
                        centerRadius
                    );
            }

            // Legs
            if (legCapsules != null)
            {
                for (int j = 0; j < legCapsules.Length; j++)
                {
                    LegCapsule leg =
                        legCapsules[j];

                    if (leg.startBone == null ||
                        leg.endBone == null)
                        continue;

                    nextTipPos =
                        ResolveCapsuleCollision(
                            nextTipPos,
                            leg.startBone.position,
                            leg.endBone.position,
                            Mathf.Max(0f, leg.radius)
                        );
                }
            }
        }


        // Final length correction
        nextTipPos =
            ConstrainLength(
                nextTipPos,
                origin
            );


        // =====================================================
        // UPDATE STATE
        // =====================================================

        previousTipPos = currentTipPos;

        currentTipPos = nextTipPos;
    }


    // =========================================================
    // KEEP BONE LENGTH
    // =========================================================

    private Vector3 ConstrainLength(
        Vector3 point,
        Vector3 origin
    )
    {
        Vector3 direction =
            point - origin;

        float sqrMagnitude =
            direction.sqrMagnitude;

        if (sqrMagnitude < 0.00000001f)
        {
            Vector3 fallback =
                useRestRotation
                    ? restWorldRotation * boneAxis
                    : transform.rotation * boneAxis;

            if (fallback.sqrMagnitude < 0.000001f)
                fallback = Vector3.up;

            fallback.Normalize();

            return origin +
                   fallback * initialLength;
        }

        return origin +
               direction.normalized *
               initialLength;
    }


    // =========================================================
    // APPLY ROTATION
    // =========================================================

    private void ApplyRotation(
        float deltaTime,
        float frameRatio
    )
    {
        Vector3 origin =
            transform.position;

        Vector3 toTip =
            currentTipPos - origin;

        if (toTip.sqrMagnitude < 0.00000001f)
            return;

        toTip.Normalize();


        // =====================================================
        // REST DIRECTION
        // =====================================================

        Vector3 restDirection =
            restWorldRotation * boneAxis;

        if (restDirection.sqrMagnitude < 0.000001f)
            return;

        restDirection.Normalize();


        // =====================================================
        // ROTATION LIMIT
        // =====================================================

        if (limitRotation)
        {
            float angle =
                Vector3.Angle(
                    restDirection,
                    toTip
                );

            if (angle > maxRotationAngle)
            {
                Vector3 axis =
                    Vector3.Cross(
                        restDirection,
                        toTip
                    );

                if (axis.sqrMagnitude > 0.000001f)
                {
                    axis.Normalize();

                    Quaternion limitRotationQuat =
                        Quaternion.AngleAxis(
                            maxRotationAngle,
                            axis
                        );

                    toTip =
                        limitRotationQuat *
                        restDirection;
                }
            }
        }


        // =====================================================
        // CURRENT AXIS
        // =====================================================

        Vector3 currentAxis =
            transform.TransformDirection(
                boneAxis
            );

        if (currentAxis.sqrMagnitude < 0.000001f)
            return;

        currentAxis.Normalize();


        // =====================================================
        // ROTATION DELTA
        // =====================================================

        Quaternion delta =
            Quaternion.FromToRotation(
                currentAxis,
                toTip
            );


        Quaternion targetRotation =
            delta *
            transform.rotation;


        // =====================================================
        // ROTATION SPEED LIMIT
        // =====================================================

        float safeFrameRatio =
            Mathf.Max(frameRatio, 1f);

        float maxDelta =
            maxDegreesPerFrame *
            safeFrameRatio;


        // =====================================================
        // APPLY
        // =====================================================

        transform.rotation =
            Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                maxDelta
            );
    }


    // =========================================================
    // SPHERE COLLISION
    // =========================================================

    private Vector3 ResolveSphereCollision(
        Vector3 point,
        Vector3 sphereCenter,
        float sphereRadius
    )
    {
        float radius =
            Mathf.Max(
                0f,
                boneRadius
            )
            +
            Mathf.Max(
                0f,
                sphereRadius
            );

        if (radius <= 0f)
            return point;

        Vector3 direction =
            point - sphereCenter;

        float distanceSqr =
            direction.sqrMagnitude;

        if (distanceSqr <
            radius * radius)
        {
            if (distanceSqr < 0.00000001f)
            {
                // ถ้า point อยู่ตรงกลาง collider
                // ใช้ทิศ rest เป็น fallback
                direction =
                    restWorldRotation *
                    boneAxis;

                if (direction.sqrMagnitude <
                    0.000001f)
                {
                    direction = Vector3.up;
                }

                direction.Normalize();
            }
            else
            {
                direction.Normalize();
            }

            return sphereCenter +
                   direction * radius;
        }

        return point;
    }


    // =========================================================
    // CAPSULE COLLISION
    // =========================================================

    private Vector3 ResolveCapsuleCollision(
        Vector3 point,
        Vector3 capsuleStart,
        Vector3 capsuleEnd,
        float capsuleRadius
    )
    {
        Vector3 line =
            capsuleEnd - capsuleStart;

        float length =
            line.magnitude;

        // ถ้า start/end อยู่จุดเดียวกัน
        // capsule จะกลายเป็น sphere
        if (length <= 0.000001f)
        {
            return ResolveSphereCollision(
                point,
                capsuleStart,
                capsuleRadius
            );
        }

        Vector3 lineDirection =
            line / length;


        float projection =
            Vector3.Dot(
                point - capsuleStart,
                lineDirection
            );

        projection =
            Mathf.Clamp(
                projection,
                0f,
                length
            );


        Vector3 closestPoint =
            capsuleStart +
            lineDirection *
            projection;


        return ResolveSphereCollision(
            point,
            closestPoint,
            capsuleRadius
        );
    }


    // =========================================================
    // RESET
    // =========================================================

    [ContextMenu("Reset Spring")]
    public void ResetSpring()
    {
        if (childBone == null)
            return;

        // คืน rotation เดิม
        transform.localRotation =
            restLocalRotation;

        transform.rotation =
            restWorldRotation;

        // reset tip
        currentTipPos =
            childBone.position;

        previousTipPos =
            childBone.position;

        previousOriginPosition =
            transform.position;
    }


    // =========================================================
    // RECAPTURE REST POSE
    // =========================================================

    [ContextMenu("Recapture Rest Pose")]
    public void RecaptureRestPose()
    {
        if (childBone == null)
            return;

        initialLength =
            Vector3.Distance(
                transform.position,
                childBone.position
            );

        if (initialLength <= 0.00001f)
            return;

        restLocalRotation =
            transform.localRotation;

        restWorldRotation =
            transform.rotation;

        currentTipPos =
            childBone.position;

        previousTipPos =
            childBone.position;

        previousOriginPosition =
            transform.position;
    }


    // =========================================================
    // VALIDATION
    // =========================================================

    private void OnValidate()
    {
        if (boneAxis.sqrMagnitude <
            0.000001f)
        {
            boneAxis =
                Vector3.up;
        }
        else
        {
            boneAxis.Normalize();
        }

        stiffnessForce =
            Mathf.Clamp(
                stiffnessForce,
                0.001f,
                1f
            );

        dragForce =
            Mathf.Clamp(
                dragForce,
                0.001f,
                0.99f
            );

        referenceFrameRate =
            Mathf.Max(
                1f,
                referenceFrameRate
            );

        substeps =
            Mathf.Clamp(
                substeps,
                1,
                8
            );

        collisionIterations =
            Mathf.Clamp(
                collisionIterations,
                1,
                8
            );

        boneRadius =
            Mathf.Max(
                0f,
                boneRadius
            );

        centerRadius =
            Mathf.Max(
                0f,
                centerRadius
            );

        maxRotationAngle =
            Mathf.Clamp(
                maxRotationAngle,
                0f,
                180f
            );

        maxDegreesPerFrame =
            Mathf.Clamp(
                maxDegreesPerFrame,
                1f,
                180f
            );
    }


    // =========================================================
    // GIZMOS
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        // -----------------------------------------------------
        // Bone
        // -----------------------------------------------------

        if (childBone != null)
        {
            Gizmos.color =
                Color.cyan;

            Gizmos.DrawLine(
                transform.position,
                childBone.position
            );

            Gizmos.DrawWireSphere(
                childBone.position,
                boneRadius
            );
        }


        // -----------------------------------------------------
        // Center Collider
        // -----------------------------------------------------

        if (centerCollider != null)
        {
            Gizmos.color =
                Color.magenta;

            Gizmos.DrawWireSphere(
                centerCollider.position,
                centerRadius
            );
        }


        // -----------------------------------------------------
        // Leg Capsules
        // -----------------------------------------------------

        if (legCapsules != null)
        {
            Gizmos.color =
                Color.yellow;

            foreach (var leg in legCapsules)
            {
                if (leg.startBone == null ||
                    leg.endBone == null)
                    continue;

                Vector3 start =
                    leg.startBone.position;

                Vector3 end =
                    leg.endBone.position;

                float radius =
                    Mathf.Max(
                        0f,
                        leg.radius
                    );

                Gizmos.DrawLine(
                    start,
                    end
                );

                Gizmos.DrawWireSphere(
                    start,
                    radius
                );

                Gizmos.DrawWireSphere(
                    end,
                    radius
                );
            }
        }


        // -----------------------------------------------------
        // Rest Direction
        // -----------------------------------------------------

        if (Application.isPlaying &&
            initialized)
        {
            Vector3 restDirection =
                restWorldRotation *
                boneAxis;

            Gizmos.color =
                Color.green;

            Gizmos.DrawLine(
                transform.position,
                transform.position +
                restDirection *
                initialLength
            );

            // Current simulated tip
            Gizmos.color =
                Color.red;

            Gizmos.DrawSphere(
                currentTipPos,
                boneRadius
            );
        }
    }
}