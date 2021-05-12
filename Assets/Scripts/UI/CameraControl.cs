/** \file CameraControl.cs
 *  \brief Contains any classes relevant to controlling the world camera/view or detecting touch input.
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/** \class CameraControl 
 *  \brief Allows touchscreen pinch and scroll controls to move the camera and detects if an object (like a dog) has been tapped.
 */
public class CameraControl : MonoBehaviour
{
    [SerializeField] private float m_movementSpeed = 0; //!< Camera movement speed.
    [SerializeField] private float m_rotationSpeed = 0; //!< Camera rotation speed.

    [SerializeField] private Controller m_controller = null;   //!< Reference to the game m_controller.

    private Camera m_camera;                                 //!< The camera object. (Just set to the main camera).
    [SerializeField] private Collider m_cameraBounds = null; //!< Boundary box the camera can move witin. (Prevents the player from moving the camera out of a range they'll be able to navigate back from).
    [SerializeField] private float m_minZoomLimit = 0;       //!< Minimum the camera's field of view can be zoomed into.
    [SerializeField] private float m_maxZoomLimit = 0;       //!< Maximum the camera's field of view can be zoomed out to.
    private Plane m_plane;                                   //!< A plane to use as a direction reference for movement.

    [SerializeField] private Vector3 m_initialPos = Vector3.zero;           //!< The camera's initial position before any touch input. (For resetting the camera.)
    [SerializeField] private Quaternion m_initialRot = Quaternion.identity; //!< The camera's initial rotation before any touch input. (For resetting the camera.)
    [SerializeField] private float m_initialZoom = 0;                       //!< The camera's initial field of view before any touch input. (For resetting the camera.)
    [SerializeField] private bool m_cameraRotationEnabled = true;           //!< Set by the rotation toggle button to enable/disable camera rotation.

    public GameObject m_followTarget = null;                                //!< A target for the camera to follow.
    [SerializeField] private Vector3 m_followOffset = Vector3.zero;         //!< Position offset at which to follow the target.

    /** \fn Start 
     *  \brief Sets the camera to the scene's main camera on script instantiation if it hasn't been allocated a camera object and sets the camera's initial orientation values.
     */
    private void Start()
    {
        m_camera = Camera.main;
        m_initialPos = m_camera.transform.position;
        m_initialRot = m_camera.transform.rotation;
        m_initialZoom = m_camera.fieldOfView;
        m_plane.distance = 10.0f;

      //  m_followTarget = m_controller.defaultNULL;
    }

    /** \fn Update 
    *   \brief Checks for touch input on a loop. Includes zooming and rotation with 2 fingers and position movement with 1 finger for mobile. Also checks if a dog has been tapped. 
    *   Contains PC control checks for the latter and is API agnostic for debugging purposes.
    */
    private void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {

#if UNITY_EDITOR //If in the editor, check for mouse input.
            CheckDogTap_Editor();
#else            //If not in the editor check for touch input. 
            CheckDogTap_Mobile();
            CheckCameraMovement_Mobile();
#endif
        }

        CheckFollowDog(); //See if there's a target to follow.
    }

    /** \fn CheckDogTap_Editor 
    *   \brief Checks if a raycast between the camera and the worldspace has hit a dog object from mouse click input. If it has, that dog is set to be the "focus dog" - this will display the info panel for it.
    */
    private void CheckDogTap_Editor()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray raycast = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit raycastHit;

            if (Physics.Raycast(raycast, out raycastHit, Mathf.Infinity))
            {
                Debug.Log("Selected: " + raycastHit.collider.gameObject.name);

                foreach (KeyValuePair<GameObject, Dog> dog in m_controller.GetAllDogs())
                {
                    if (raycastHit.transform.gameObject == dog.Key)
                    {
                        dog.Value.m_facts["IS_FOCUS"] = true;

                        if (m_controller.UIOutput.GetFocusDog() != dog.Key)
                        {
                            m_followTarget = dog.Value.gameObject;
                            m_controller.UIOutput.gameObject.SetActive(true);
                            m_controller.UIOutput.OnOpen(dog.Value);
                        }
                        return;
                    }
                }
            }
        }
    }

    /** \fn CheckDogTap_Mobile 
    *   \brief Checks if a raycast between the camera and the worldspace has hit a dog object from touch input. If it has, that dog is set to be the "focus dog" - this will display the info panel for it.
    */
    private void CheckDogTap_Mobile()
    {
        if ((Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Began)) //Gets first touch input.
        {
            Ray raycast = Camera.main.ScreenPointToRay(Input.GetTouch(0).position); //A raycast between the camera and touch position to get the world position of the touch.
            RaycastHit raycastHit;

            if (Physics.Raycast(raycast, out raycastHit, Mathf.Infinity)) //If the raycast hits anything...
            {
                foreach (KeyValuePair<GameObject, Dog> dog in m_controller.GetAllDogs())
                {
                    if (raycastHit.transform.gameObject == dog.Key)
                    {
                        dog.Value.m_facts["IS_FOCUS"] = true;

                        if (m_controller.UIOutput.GetFocusDog() != dog.Key)
                        {
                            m_followTarget = dog.Value.gameObject;
                            m_controller.UIOutput.gameObject.SetActive(true);
                            m_controller.UIOutput.OnOpen(dog.Value);

                        }
                        return;
                    }
                }
            }
        }
    }

    /** \fn CheckFollowDog
    *   \brief Follows the last selected dog at an offset.
    */
    private void CheckFollowDog()
    {
        if (m_followTarget != null)
        {
            m_camera.transform.position = m_followTarget.transform.position + m_followOffset;
            m_camera.transform.rotation = Quaternion.LookRotation(m_followTarget.transform.position - m_camera.transform.position);
        }
    }

    /** \fn CheckCameraMovement_Mobile 
    *   \brief Detects touch screen input for camera movement. One finger touch dragging results in panning if rotation is toggled as off, and rotation otherwise, and pinching/expanding results in zooming in and out.
    */
    private void CheckCameraMovement_Mobile()
    {
        // Number of screen touches this update.   
        int touchCount = Input.touchCount; 

        // If there has been touchscreen input...
        if (touchCount > 0)   
        {
            if (m_followTarget != null) { ResetCamera(); return; };

            m_plane.SetNormalAndPosition(transform.up, transform.position); // Update the relative position of upwards.
            Touch touch1 = Input.GetTouch(0); // Get the first finger's touch input data.

            if (touch1.phase == TouchPhase.Moved)
            {
                if (touchCount == 1) //Single Finger Input
                {
                    //////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    /////////////////////////////////////////////// CAMERA PANNING ///////////////////////////////////////////////
                    //////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    m_camera.transform.Translate(PlanePositionDelta(touch1) * m_movementSpeed, Space.World);
                }

                else if (touchCount >= 2) //Two Finger Input
                {
                    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    /////////////////////////////////////////////// CAMERA ROTATION ///////////////////////////////////////////////
                    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    if (m_cameraRotationEnabled)
                    {
                            var pos1 = PlanePosition(Input.GetTouch(0).position);
                            var pos1b = PlanePosition(Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition);
                            m_camera.transform.RotateAround(pos1, m_plane.normal, -Vector3.SignedAngle(pos1, pos1b, m_plane.normal) * m_rotationSpeed);
                    }

                    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    /////////////////////////////////////////////// 2 FINGER ZOOM MOVEMENT ///////////////////////////////////////////////
                    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
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
        m_followTarget = null;
        m_camera.transform.position = m_initialPos;
        m_camera.transform.rotation = m_initialRot;
        m_camera.fieldOfView = m_initialZoom;
    }
}
