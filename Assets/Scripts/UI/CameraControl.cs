/** \file CameraControl.cs
 *  \brief Contains any classes relevant to controlling the world camera/view.
 */
using UnityEngine;

/** \class CameraControl 
 *  \brief Allows touchscreen pinch and scroll controls to move the camera.
 */
public class CameraControl : MonoBehaviour
{
#if UNITY_IOS || UNITY_ANDROID
    [SerializeField] private Camera m_camera; //!< The camera object to be moved on input.
    [SerializeField] private Collider m_cameraBounds;
    private Plane m_plane;   //!< A plane to use as a direction reference for movement.


    [SerializeField] private Vector3 m_initialPos;
    //[SerializeField] private Quaternion m_initialRot;
    [SerializeField] private float m_zoomLimit = 5.0f;
    [SerializeField] private bool m_cameraRotationEnabled = false;

    /** \fn Awake 
     *  \brief Sets the camera to the scene's main camera on script instantiation if it hasn't been allocated a camera object before runtime.
     */
    private void Awake()
    {
        if (m_camera == null) m_camera = Camera.main;
        m_initialPos = m_camera.transform.position;
      //  m_initialRot = m_camera.transform.rotation;
    }

    /** \fn Update 
    *   \brief ...
    **/
    private void Update()
    {
        int touchCount = Input.touchCount; // Number of screen touches this update.   

        // If there has been touchscreen input...
        if (touchCount >= 1)
        {
            Touch touch1 = Input.GetTouch(0);
            m_plane.SetNormalAndPosition(transform.up, transform.position);

            //If the player used 1 finger to input, move the camera to that position.
            m_camera.transform.Translate(PlanePositionDelta(touch1), Space.World);


            if (touchCount >= 2)
            {
                Touch touch2 = Input.GetTouch(1);

                Vector3 beforePos1 = PlanePosition(touch1.position);   // Player's first touch input of the update before moving.
                Vector3 beforePos2 = PlanePosition(touch2.position);   // Player's second touch input of the update before moving.
                Vector3 afterPos1 = PlanePosition(touch1.position - touch1.deltaPosition);   // Player's first touch input of the update before moving.
                Vector3 afterPos2 = PlanePosition(touch2.position - touch2.deltaPosition);   // Player's second touch input of the update before moving.

                float zoom = Vector3.Distance(beforePos1, beforePos2) / Vector3.Distance(afterPos1, afterPos2);
                zoom = Mathf.Clamp(zoom, -m_zoomLimit, m_zoomLimit);

                if (zoom == 0)
                {
                    return;
                }

                if (afterPos2 != beforePos2)
                {
                    m_camera.transform.position = Vector3.LerpUnclamped(beforePos1, m_camera.transform.position, 1/zoom);

                    if (m_cameraRotationEnabled)
                    {
                        m_camera.transform.RotateAround(beforePos1, m_plane.normal, Vector3.SignedAngle(beforePos2 - beforePos1, afterPos2 - afterPos1, m_plane.normal));
                    }
                }
            }

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
#endif
}
