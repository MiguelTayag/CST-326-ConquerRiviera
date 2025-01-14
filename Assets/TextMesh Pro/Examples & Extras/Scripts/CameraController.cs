using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;


namespace TMPro.Examples
{
    
    public class CameraController : MonoBehaviour
    {
        public enum CameraModes { Follow, Isometric, Free }

        private Transform cameraTransform;
        private Transform dummyTarget;

        [FormerlySerializedAs("CameraTarget")] public Transform cameraTarget;

        [FormerlySerializedAs("FollowDistance")] public float followDistance = 30.0f;
        [FormerlySerializedAs("MaxFollowDistance")] public float maxFollowDistance = 100.0f;
        [FormerlySerializedAs("MinFollowDistance")] public float minFollowDistance = 2.0f;

        [FormerlySerializedAs("ElevationAngle")] public float elevationAngle = 30.0f;
        [FormerlySerializedAs("MaxElevationAngle")] public float maxElevationAngle = 85.0f;
        [FormerlySerializedAs("MinElevationAngle")] public float minElevationAngle = 0f;

        [FormerlySerializedAs("OrbitalAngle")] public float orbitalAngle = 0f;

        [FormerlySerializedAs("CameraMode")] public CameraModes cameraMode = CameraModes.Follow;

        [FormerlySerializedAs("MovementSmoothing")] public bool movementSmoothing = true;
        [FormerlySerializedAs("RotationSmoothing")] public bool rotationSmoothing = false;
        private bool previousSmoothing;

        [FormerlySerializedAs("MovementSmoothingValue")] public float movementSmoothingValue = 25f;
        [FormerlySerializedAs("RotationSmoothingValue")] public float rotationSmoothingValue = 5.0f;

        [FormerlySerializedAs("MoveSensitivity")] public float moveSensitivity = 2.0f;

        private Vector3 currentVelocity = Vector3.zero;
        private Vector3 desiredPosition;
        private float mouseX;
        private float mouseY;
        private Vector3 moveVector;
        private float mouseWheel;

        // Controls for Touches on Mobile devices
        //private float prev_ZoomDelta;


        private const string EVENT_SMOOTHING_VALUE = "Slider - Smoothing Value";
        private const string EVENT_FOLLOW_DISTANCE = "Slider - Camera Zoom";


        void Awake()
        {
            if (QualitySettings.vSyncCount > 0)
                Application.targetFrameRate = 60;
            else
                Application.targetFrameRate = -1;

            if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
                Input.simulateMouseWithTouches = false;

            cameraTransform = transform;
            previousSmoothing = movementSmoothing;
        }


        // Use this for initialization
        void Start()
        {
            if (cameraTarget == null)
            {
                // If we don't have a target (assigned by the player, create a dummy in the center of the scene).
                dummyTarget = new GameObject("Camera Target").transform;
                cameraTarget = dummyTarget;
            }
        }

        // Update is called once per frame
        void LateUpdate()
        {
            GetPlayerInput();


            // Check if we still have a valid target
            if (cameraTarget != null)
            {
                if (cameraMode == CameraModes.Isometric)
                {
                    desiredPosition = cameraTarget.position + Quaternion.Euler(elevationAngle, orbitalAngle, 0f) * new Vector3(0, 0, -followDistance);
                }
                else if (cameraMode == CameraModes.Follow)
                {
                    desiredPosition = cameraTarget.position + cameraTarget.TransformDirection(Quaternion.Euler(elevationAngle, orbitalAngle, 0f) * (new Vector3(0, 0, -followDistance)));
                }
                else
                {
                    // Free Camera implementation
                }

                if (movementSmoothing == true)
                {
                    // Using Smoothing
                    cameraTransform.position = Vector3.SmoothDamp(cameraTransform.position, desiredPosition, ref currentVelocity, movementSmoothingValue * Time.fixedDeltaTime);
                    //cameraTransform.position = Vector3.Lerp(cameraTransform.position, desiredPosition, Time.deltaTime * 5.0f);
                }
                else
                {
                    // Not using Smoothing
                    cameraTransform.position = desiredPosition;
                }

                if (rotationSmoothing == true)
                    cameraTransform.rotation = Quaternion.Lerp(cameraTransform.rotation, Quaternion.LookRotation(cameraTarget.position - cameraTransform.position), rotationSmoothingValue * Time.deltaTime);
                else
                {
                    cameraTransform.LookAt(cameraTarget);
                }

            }

        }



        void GetPlayerInput()
        {
            moveVector = Vector3.zero;

            // Check Mouse Wheel Input prior to Shift Key so we can apply multiplier on Shift for Scrolling
            mouseWheel = Input.GetAxis("Mouse ScrollWheel");

            float touchCount = Input.touchCount;

            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || touchCount > 0)
            {
                mouseWheel *= 10;

                if (Input.GetKeyDown(KeyCode.I))
                    cameraMode = CameraModes.Isometric;

                if (Input.GetKeyDown(KeyCode.F))
                    cameraMode = CameraModes.Follow;

                if (Input.GetKeyDown(KeyCode.S))
                    movementSmoothing = !movementSmoothing;


                // Check for right mouse button to change camera follow and elevation angle
                if (Input.GetMouseButton(1))
                {
                    mouseY = Input.GetAxis("Mouse Y");
                    mouseX = Input.GetAxis("Mouse X");

                    if (mouseY > 0.01f || mouseY < -0.01f)
                    {
                        elevationAngle -= mouseY * moveSensitivity;
                        // Limit Elevation angle between min & max values.
                        elevationAngle = Mathf.Clamp(elevationAngle, minElevationAngle, maxElevationAngle);
                    }

                    if (mouseX > 0.01f || mouseX < -0.01f)
                    {
                        orbitalAngle += mouseX * moveSensitivity;
                        if (orbitalAngle > 360)
                            orbitalAngle -= 360;
                        if (orbitalAngle < 0)
                            orbitalAngle += 360;
                    }
                }

                // Get Input from Mobile Device
                if (touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
                {
                    Vector2 deltaPosition = Input.GetTouch(0).deltaPosition;

                    // Handle elevation changes
                    if (deltaPosition.y > 0.01f || deltaPosition.y < -0.01f)
                    {
                        elevationAngle -= deltaPosition.y * 0.1f;
                        // Limit Elevation angle between min & max values.
                        elevationAngle = Mathf.Clamp(elevationAngle, minElevationAngle, maxElevationAngle);
                    }


                    // Handle left & right 
                    if (deltaPosition.x > 0.01f || deltaPosition.x < -0.01f)
                    {
                        orbitalAngle += deltaPosition.x * 0.1f;
                        if (orbitalAngle > 360)
                            orbitalAngle -= 360;
                        if (orbitalAngle < 0)
                            orbitalAngle += 360;
                    }

                }

                // Check for left mouse button to select a new CameraTarget or to reset Follow position
                if (Input.GetMouseButton(0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit, 300, 1 << 10 | 1 << 11 | 1 << 12 | 1 << 14))
                    {
                        if (hit.transform == cameraTarget)
                        {
                            // Reset Follow Position
                            orbitalAngle = 0;
                        }
                        else
                        {
                            cameraTarget = hit.transform;
                            orbitalAngle = 0;
                            movementSmoothing = previousSmoothing;
                        }

                    }
                }


                if (Input.GetMouseButton(2))
                {
                    if (dummyTarget == null)
                    {
                        // We need a Dummy Target to anchor the Camera
                        dummyTarget = new GameObject("Camera Target").transform;
                        dummyTarget.position = cameraTarget.position;
                        dummyTarget.rotation = cameraTarget.rotation;
                        cameraTarget = dummyTarget;
                        previousSmoothing = movementSmoothing;
                        movementSmoothing = false;
                    }
                    else if (dummyTarget != cameraTarget)
                    {
                        // Move DummyTarget to CameraTarget
                        dummyTarget.position = cameraTarget.position;
                        dummyTarget.rotation = cameraTarget.rotation;
                        cameraTarget = dummyTarget;
                        previousSmoothing = movementSmoothing;
                        movementSmoothing = false;
                    }


                    mouseY = Input.GetAxis("Mouse Y");
                    mouseX = Input.GetAxis("Mouse X");

                    moveVector = cameraTransform.TransformDirection(mouseX, mouseY, 0);

                    dummyTarget.Translate(-moveVector, Space.World);

                }

            }

            // Check Pinching to Zoom in - out on Mobile device
            if (touchCount == 2)
            {
                Touch touch0 = Input.GetTouch(0);
                Touch touch1 = Input.GetTouch(1);

                Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
                Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;

                float prevTouchDelta = (touch0PrevPos - touch1PrevPos).magnitude;
                float touchDelta = (touch0.position - touch1.position).magnitude;

                float zoomDelta = prevTouchDelta - touchDelta;

                if (zoomDelta > 0.01f || zoomDelta < -0.01f)
                {
                    followDistance += zoomDelta * 0.25f;
                    // Limit FollowDistance between min & max values.
                    followDistance = Mathf.Clamp(followDistance, minFollowDistance, maxFollowDistance);
                }


            }

            // Check MouseWheel to Zoom in-out
            if (mouseWheel < -0.01f || mouseWheel > 0.01f)
            {

                followDistance -= mouseWheel * 5.0f;
                // Limit FollowDistance between min & max values.
                followDistance = Mathf.Clamp(followDistance, minFollowDistance, maxFollowDistance);
            }


        }
    }
}