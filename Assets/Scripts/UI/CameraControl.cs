/** \file CameraControl.cs
 *  \brief Contains any classes relevant to controlling the world camera/view.
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/** \class CameraControl 
 *  \brief Allows touchscreen pinch and scroll controls to move the camera.
 */
public class CameraControl : MonoBehaviour
{
    private float m_movementSpeed = 1.75f;
    private float m_rotationSpeed = 2.75f;

    [SerializeField] private Controller controller;

    private Camera m_camera;    //!< The camera object to set the camera specific values of.
    [SerializeField] private Collider m_cameraBounds;   //!< Boundary box the camera can move witin. (Prevents the player from moving the camera out of a range they'll be able to navigate back from).
    [SerializeField] private float m_minZoomLimit;      //!< Minimum the camera's field of view can be zoomed into.
    [SerializeField] private float m_maxZoomLimit;      //!< Maximum the camera's field of view can be zoomed out to.
    private Plane m_plane;                              //!< A plane to use as a direction reference for movement.

    [SerializeField] private Vector3 m_initialPos;          //!< The camera's initial position before any touch input. (For resetting the camera.)
    [SerializeField] private Quaternion m_initialRot;       //!< The camera's initial rotation before any touch input. (For resetting the camera.)
    [SerializeField] private float m_initialZoom;           //!< The camera's initial field of view before any touch input. (For resetting the camera.)
    [SerializeField] private bool m_cameraRotationEnabled;  //!< Set by the rotation toggle button to enable/disable camera rotation.

    /** \fn Awake 
     *  \brief Sets the camera to the scene's main camera on script instantiation if it hasn't been allocated a camera object before runtime and sets the camera's initial orientation values.
     */
    private void Start()
    {
        m_camera = Camera.main;
        m_initialPos = m_camera.transform.position;
        m_initialRot = m_camera.transform.rotation;
        m_initialZoom = m_camera.fieldOfView;
        m_plane.distance = 10.0f;
    }

    /** \fn FixedUpdate 
    *   \brief Checks for touch input on a loop. Includes zooming and rotation with 2 fingers, and position movement with 1 finger.
    **/
    private void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            //If in the editor, check for mouse input.
#if UNITY_EDITOR
            CheckDogTap_Editor();
            //If not in the editor check for touch input. 
#elif UNITY_IOS || UNITY_ANDROID
            CheckDogTap_Mobile();
            CheckCameraMovement_Mobile();
#endif
        }
    }

    private void CheckDogTap_Editor()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("AAA");

            Ray raycast = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit raycastHit;

            if (Physics.Raycast(raycast, out raycastHit, Mathf.Infinity))
            {
                Debug.Log("Selected: " + raycastHit.collider.gameObject.name);

                foreach (KeyValuePair<GameObject, Dog> dog in controller.GetAllDogs())
                {
                    if (raycastHit.transform.gameObject == dog.Key)
                    {
                        dog.Value.m_facts["IS_FOCUS"] = true;

                        if (controller.UIOutput.GetFocusDog() != dog.Key)
                        {
                            controller.UIOutput.SetFocusDog(dog.Value);
                        }
                        return;
                    }
                }
            }
        }
    }

    private void CheckDogTap_Mobile()
    {
        if ((Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Began)) //Gets first touch input.
        {
            Ray raycast = Camera.main.ScreenPointToRay(Input.GetTouch(0).position); //A raycast between the camera and touch position to get the world position of the touch.
            RaycastHit raycastHit;

            if (Physics.Raycast(raycast, out raycastHit, Mathf.Infinity)) //If the raycast hits anything...
            {
                foreach (KeyValuePair<GameObject, Dog> dog in controller.GetAllDogs())
                {
                    if (raycastHit.transform.gameObject == dog.Key)
                    {
                        dog.Value.m_facts["IS_FOCUS"] = true;

                        if (controller.UIOutput.GetFocusDog() != dog.Key)
                        {
                            controller.UIOutput.SetFocusDog(dog.Value);
                        }
                        return;
                    }
                }
            }
        }
    }

    private void CheckCameraMovement_Mobile()
    {
        int touchCount = Input.touchCount; // Number of screen touches this update.   

        if (touchCount > 0)    // If there has been touchscreen input...
        {
            m_plane.SetNormalAndPosition(transform.up, transform.position); // Update the relative position of "upwards."
            Touch touch1 = Input.GetTouch(0); // Get the first finger's touch input data.

            if (touch1.phase == TouchPhase.Moved)
            {
                if (touchCount == 1) //Single Finger Input
                {
                    switch (m_cameraRotationEnabled)
                    {
                        case (true):
                            var pos1 = PlanePosition(Input.GetTouch(0).position);
                            var pos1b = PlanePosition(Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition);
                            m_camera.transform.RotateAround(pos1, m_plane.normal, -Vector3.SignedAngle(pos1, pos1b, m_plane.normal) * m_rotationSpeed);
                            break;
                        case (false):
                            m_camera.transform.Translate(PlanePositionDelta(touch1) * m_movementSpeed, Space.World);
                            break;
                    }
                }
                else if (touchCount >= 2) //Two Finger Input
                {
                    Touch touch2 = Input.GetTouch(1); // Get the second finger's touch input data.

                    Vector3 beforePos1 = PlanePosition(touch1.position);   // Player's first touch input of the update before moving.
                    Vector3 beforePos2 = PlanePosition(touch2.position);   // Player's second touch input of the update before moving.
                    Vector3 afterPos1 = PlanePosition(touch1.position - touch1.deltaPosition);   // Player's first touch input of the update before moving.
                    Vector3 afterPos2 = PlanePosition(touch2.position - touch2.deltaPosition);   // Player's second touch input of the update before moving.

                    float zoom = Vector3.Distance(afterPos1, afterPos2) / Vector3.Distance(beforePos1, beforePos2); // How much to zoom in or out: the average position between the changed distance of the players 2 fingers after movement.

                    if ((zoom != 0) && ((m_camera.fieldOfView * zoom) >= m_minZoomLimit) && ((m_camera.fieldOfView * zoom) < m_maxZoomLimit))
                    {
                        m_camera.fieldOfView *= zoom;
                    }
                }

                //Reset the camera's position back within bounds if it has exited during touch movement input.
                if (!m_cameraBounds.bounds.Contains(m_camera.transform.position))
                {
                    m_camera.transform.position = m_cameraBounds.bounds.ClosestPoint(m_camera.transform.position);
                }
            }
        }
    }

    /** \fn PlanePosition 
     *  \brief Returns the camera/plane position before movement.
     *  \param screenPos The position of the player's fingers on the screen.
     **/
    protected Vector3 PlanePosition(Vector2 screenPos)
    {
        var currentRay = m_camera.ScreenPointToRay(screenPos); // A ray between the camera's position and the position of the player's fingers.

        if (m_plane.Raycast(currentRay, out var enterNow))
        {
            return currentRay.GetPoint(enterNow);
        }
        else
        {
            return Vector3.zero;
        }
    }

    /** \fn PlanePositionDelta
     *  \brief Returns the camera/plane position after movement.
     *  \param touch A touch event; when the player moves their finger position across the screen for a period of time.
     **/
    protected Vector3 PlanePositionDelta(Touch touch)
    {
        var rayBefore = m_camera.ScreenPointToRay(touch.position - touch.deltaPosition);
        var rayNow = m_camera.ScreenPointToRay(touch.position);

        if (m_plane.Raycast(rayBefore, out var enterBefore) && m_plane.Raycast(rayNow, out var enterNow))
        {
            return rayBefore.GetPoint(enterBefore) - rayNow.GetPoint(enterNow);
        }

        return Vector3.zero;
    }

    /** \fn ToggleRotation
    *  \brief Allows the rotation toggle to set whether or not rotation is enabled.
    **/
    public void ToggleRotation()
    {
        m_cameraRotationEnabled = !m_cameraRotationEnabled;
    }

    /** \fn ResetCamera
    *  \brief Allows the reset button to reset all the camera's values to what they were before touch input.
    **/
    public void ResetCamera()
    {
        m_camera.transform.position = m_initialPos;
        m_camera.transform.rotation = m_initialRot;
        m_camera.fieldOfView = m_initialZoom;
    }
}
