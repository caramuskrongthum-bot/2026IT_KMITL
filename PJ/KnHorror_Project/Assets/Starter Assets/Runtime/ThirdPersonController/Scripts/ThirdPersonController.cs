using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using StarterAssets;
namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {

        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        [Header("💅✨ DBD LEAN & SLOPE SETTINGS ✨")]
        [Tooltip("มุมเอียงตัวสูงสุดเมื่อเลี้ยว (Z-Axis)")]
        public float MaxLeanAngle = 12.0f;
        [Tooltip("ความเร็วในการเอียงตัว")]
        public float LeanSpeed = 8.0f;

        [Space(5)]
        [Tooltip("เปิด/ปิดระบบเอียงลำตัวและขาตามความชันของพื้น")]
        public bool AlignToGroundNormal = true;
        [Tooltip("ความเร็วในการปรับองศาลำตัวให้ขนานพื้น")]
        public float SurfaceAlignSpeed = 10.0f;

        [Header("✨ NATIVE QUEEN IK SETTINGS ✨")]
        [Tooltip("เปิด/ปิดระบบ Head Look At")]
        public bool EnableLookAt = true;
        [Range(0, 1)] public float LookAtWeight = 0.85f;
        [Range(0, 1)] public float BodyWeight = 0.3f;
        [Range(0, 1)] public float HeadWeight = 0.9f;
        public float LookAtSpeed = 10.0f;
        public float LookDistance = 15.0f;

        [Space(5)]
        [Tooltip("เปิด/ปิด Foot IK")]
        public bool EnableFootIK = true;
        [Range(0, 1)] public float FootIKWeight = 1.0f;
        public float FootRaycastDistance = 1.2f;
        public float FootOffset = 0.14f;
        public float FootIKSpeed = 20.0f;

        [Header("✨ CATCHING & TELEPORT STATE ✨")]
        [Tooltip("หากเป็น true จะหยุดการเคลื่อนไหวและการควบคุมทั้งหมด เพื่อเตรียมรับการ Teleport")]
        public bool IsCatching = false;

        // Cinemachine internal
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // Player internal
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // DBD Mechanics Internal
        private float _currentLeanAngle;
        private Vector3 _groundNormal = Vector3.up;

        // Timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // Animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;
        private bool _hasAnimator;

        // IK Internal variables
        private Vector3 _currentLookPos;
        private Vector3 _leftFootIKPos, _rightFootIKPos;
        private Quaternion _leftFootIKRot, _rightFootIKRot;
        private float _currentLeftWeight, _currentRightWeight;
        public bool CanMove = true;
        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
                return false;
#endif
            }
        }

        private void Awake()
        {
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#else
            Debug.LogError("Starter Assets package is missing dependencies.");
#endif

            AssignAnimationIDs();

            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            // ถ้าโดนจับอยู่ (IsCatching = true) ให้ตัดการทำงานฟังก์ชันเคลื่อนไหวและแรงโน้มถ่วงทิ้งไปเลยแม่
            if (IsCatching)
            {
                if (_controller.enabled) _controller.enabled = false;
                return;
            }
            else
            {
                if (!_controller.enabled) _controller.enabled = true;
            }

            JumpAndGravity();
            GroundedCheck();
            if (CanMove)
            {
                Move();
            }
            else
            {
                _animator.SetFloat("Speed", 0);
            }
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        public void SetBoolCanMove(bool a)
        {
            CanMove = a;
        }
        public void SetBoolCanMoveT()
        {
            CanMove = true;
        }
        public void SetBoolCanMoveF()
        {
            CanMove = false;
        }

        // ==========================================
        // 💅✨ PUBLIC METHODS FOR CATCHING STATE
        // ==========================================
        public void SetIsCatching(bool catching)
        {
            IsCatching = catching;
        }

        public void SetIsCatchingTrue()
        {
            IsCatching = true;
        }

        public void SetIsCatchingFalse()
        {
            IsCatching = false;
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void GroundedCheck()
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

            if (Grounded && Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 1.5f, GroundLayers))
            {
                _groundNormal = hit.normal;
            }
            else
            {
                _groundNormal = Vector3.up;
            }

            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void CameraRotation()
        {
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f);
        }

        private void Move()
        {
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);

                float targetLean = -_input.move.x * MaxLeanAngle * (_speed / SprintSpeed);
                _currentLeanAngle = Mathf.Lerp(_currentLeanAngle, targetLean, Time.deltaTime * LeanSpeed);

                Quaternion targetRotation = Quaternion.Euler(0.0f, rotation, 0.0f);

                if (AlignToGroundNormal && Grounded)
                {
                    Quaternion surfaceRotation = Quaternion.FromToRotation(transform.up, _groundNormal) * transform.rotation;
                    targetRotation = Quaternion.Slerp(transform.rotation, surfaceRotation, Time.deltaTime * SurfaceAlignSpeed);
                }

                transform.rotation = targetRotation * Quaternion.Euler(0.0f, 0.0f, _currentLeanAngle);
            }
            else
            {
                _currentLeanAngle = Mathf.Lerp(_currentLeanAngle, 0.0f, Time.deltaTime * LeanSpeed);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0.0f, transform.eulerAngles.y, 0.0f), Time.deltaTime * LeanSpeed);
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                _fallTimeoutDelta = FallTimeout;

                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                _jumpTimeoutDelta = JumpTimeout;

                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    _animator.SetBool(_animIDFreeFall, true);
                }

                _input.jump = false;
            }

            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (!_hasAnimator || IsCatching) return; // ปิด IK ชั่วคราวตอนโดนจับด้วยเพื่อความปลอดภัย

            if (EnableLookAt && _mainCamera != null)
            {
                Vector3 targetPos = _mainCamera.transform.position + _mainCamera.transform.forward * LookDistance;
                _currentLookPos = Vector3.Lerp(_currentLookPos, targetPos, Time.deltaTime * LookAtSpeed);

                _animator.SetLookAtWeight(LookAtWeight, BodyWeight, HeadWeight, 1.0f, 0.5f);
                _animator.SetLookAtPosition(_currentLookPos);
            }
            else
            {
                _animator.SetLookAtWeight(0);
            }

            if (EnableFootIK && Grounded)
            {
                ProcessFootIK(AvatarIKGoal.LeftFoot, ref _leftFootIKPos, ref _leftFootIKRot, ref _currentLeftWeight);
                ProcessFootIK(AvatarIKGoal.RightFoot, ref _rightFootIKPos, ref _rightFootIKRot, ref _currentRightWeight);
            }
            else
            {
                _animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);
                _animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0);
                _animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
                _animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0);
            }
        }

        private void ProcessFootIK(AvatarIKGoal foot, ref Vector3 currentPos, ref Quaternion currentRot, ref float currentWeight)
        {
            Vector3 footPos = _animator.GetIKPosition(foot);
            Vector3 rayStart = footPos + Vector3.up * 0.5f;

            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, FootRaycastDistance, GroundLayers, QueryTriggerInteraction.Ignore))
            {
                Vector3 targetPos = hit.point + Vector3.up * FootOffset;
                Quaternion targetRot = Quaternion.FromToRotation(Vector3.up, hit.normal) * transform.rotation;

                currentPos = Vector3.Lerp(currentPos, targetPos, Time.deltaTime * FootIKSpeed);
                currentRot = Quaternion.Slerp(currentRot, targetRot, Time.deltaTime * FootIKSpeed);
                currentWeight = Mathf.Lerp(currentWeight, FootIKWeight, Time.deltaTime * 10f);

                _animator.SetIKPositionWeight(foot, currentWeight);
                _animator.SetIKRotationWeight(foot, currentWeight);
                _animator.SetIKPosition(foot, currentPos);
                _animator.SetIKRotation(foot, currentRot);
            }
            else
            {
                currentWeight = Mathf.Lerp(currentWeight, 0f, Time.deltaTime * 10f);
                _animator.SetIKPositionWeight(foot, currentWeight);
                _animator.SetIKRotationWeight(foot, currentWeight);
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            Gizmos.color = Grounded ? transparentGreen : transparentRed;
            Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f && FootstepAudioClips.Length > 0)
            {
                var index = Random.Range(0, FootstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }
    }
}