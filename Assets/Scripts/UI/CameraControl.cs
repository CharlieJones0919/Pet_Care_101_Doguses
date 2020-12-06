/** \file CameraControl.cs
 *  \brief Contains any classes relevant to controlling the world camera/view.
 */
using UnityEngine;
using UnityEngine.UI;

/** \class CameraControl 
 *  \brief Allows touchscreen pinch and scroll controls to move the camera.
 */
public class CameraControl : MonoBehaviour
{
#if UNITY_IOS || UNITY_ANDROID
    [SerializeField] private Camera m_camera;           //!< The camera object to be moved on input.
    [SerializeField] private Collider m_cameraBounds;   //!< Boundary box the camera can move witin. (Prevents the player from moving the camera out of a range they'll be able to navigate back from).
    [SerializeField] private float m_minZoomLimit;      //!< Minimum the camera's field of view can be zoomed into.
    [SerializeField] private float m_maxZoomLimit;      //!< Maximum the camera's field of view can be zoomed out to.
    private Plane m_plane;                              //!< A plane to use as a direction reference for movement.

    [SerializeField] private Vector3 m_initialPos;          //!< The camera's initial position before any touch input. (For resetting the camera.)
    [SerializeField] private Quaternion m_initialRot;       //!< The camera's initial rotation before any touch input. (For resetting the camera.)
    [SerializeField] private float m_initialZoom;      //!< The camera's initial field of view before any touch input. (For resetting the camera.)
    [SerializeField] private bool m_cameraRotationEnabled;  //!< Set by the rotation toggle button to enable/disable camera rotation.


    /** \fn Awake 
     *  \brief Sets the camera to the scene's main camera on script instantiation if it hasn't been allocated a camera object before runtime.
     */
    private void Awake()
    {
        if (m_camera == null) m_camera = Camera.main;
        m_initialPos = m_camera.transform.position;
        m_initialRot = m_camera.transform.rotation;
        m_initialZoom = m_camera.fieldOfView;
    }

    /** \fn Update 
    *   \brief Checks for touch input on a loop. Includes zooming and rotation with 2 fingers, and position movement with 1 finger.
    **/
    private void Update()
    {
        int touchCount = Input.touchCount; // Number of screen touches this update.   

        if (touchCount > 0)    // If there has been touchscreen input...
        {
            m_plane.SetNormalAndPosition(transform.up, transform.position); // Update the relative position of "upwards."
            Touch touch1 = Input.GetTouch(0); // Get the first finger's touch input data.

            if (touchCount >= 2)
            {
                if (m_cameraRotationEnabled)
                {
                    float angle = Vector3.SignedAngle(PlanePosition(touch1.position), PlanePositionDelta(touch1), m_plane.normal) * Time.deltaTime;
                    if (angle != 0)
                    {
                        Vector3 rotation = new Vector3(0.0f, angle, 0.0f);
                        m_camera.transform.Rotate(rotation, Space.World);
                    }
                }

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
            else if (touchCount == 1)
            {
                // If the player used 1 finger to input, translate the camera's position.
                m_camera.transform.Translate(PlanePositionDelta(touch1), Space.World);
            }

            // Reset the camera's position back within bounds if it has exited during touch movement input.
            if (!m_cameraBounds.bounds.Contains(m_camera.transform.position))
            {
                m_camera.transform.position = m_cameraBounds.bounds.ClosestPoint(m_camera.transform.position);
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
        if (touch.phase != TouchPhase.Moved) //If the player's finger position hasn't moved, do nothing.
        {
            return Vector3.zero;
        }
        else // If the player's finger moves, return the difference in position from before and after the movement.
        {
            var rayBefore = m_camera.ScreenPointToRay(touch.position - touch.deltaPosition);
            var rayNow = m_camera.ScreenPointToRay(touch.position);

            if (m_plane.Raycast(rayBefore, out var enterBefore) && m_plane.Raycast(rayNow, out var enterNow))
            {
                return rayBefore.GetPoint(enterBefore) - rayNow.GetPoint(enterNow);
            }
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
#endif
}
