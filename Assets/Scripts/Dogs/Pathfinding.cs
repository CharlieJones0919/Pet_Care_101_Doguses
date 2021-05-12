/** \file Pathfinding.cs */
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/** \enum MoveSpeed
*  \brief A enum list of options for setting the dog's movement speed to. (Reduces typos by keeping them as an enumertated list).
*/
public enum MoveSpeed { Crawling, Walking, Running };

/** \class Pathfinding
*  \brief Contains functions which utilise the A* pathfinding classes/functionalities to move the dog towards its targets at specified speeds and detect when it has reached them.
*/
public class Pathfinding : MonoBehaviour
{
    public AStarSearch m_aStarSearch;                           //!< Reference to the A* script. (Retrieved from the ground plane).
    public GameObject m_randomPointStorage;                     //!< Where to store the random point in the hierarchy. Done for cleanliness of project.
    [SerializeField] private GameObject m_randomPoint = null;   //!< An empty game object which can be placed at a random position on the ground plane for random movement.
                                                                
    ////////// Dog Movement Speeds //////////                   
    [SerializeField] private float m_crawlSpeed = 0;            //!< Slowest movement speed definition.   
    [SerializeField] private float m_walkSpeed = 0;             //!< Regular movement speed definition. 
    [SerializeField] private float m_runSpeed = 0;              //!< Fastest movement speed definition. 
    [SerializeField] private float m_currentSpeed = 0;          //!< The dog's current set movement speed. (Is set to m_walkSpeed on initialisation).
                                                                
    [SerializeField] private Rigidbody m_RB = null;             //!< The dog's rigidbody component for setting velocity to for movement. 
    [SerializeField] private Collider m_collider = null;        //!< The dog's collider for checking if the next node in a found path has been reached yet: its position is within the collider's bounds.
                                                                
    private List<Vector3> m_foundPath = new List<Vector3>();    //!< Requested path to a destination as a list of Vector3 positions.
    [SerializeField] private bool m_randomNodeFound = false;    //!< Whether or not a traversable random node has been generated.
    [SerializeField] private bool m_pathFound = false;          //!< If a path has been found yet or not.

    [SerializeField] private GameObject m_currentTarget = null; //!< What the dog is currently trying to find a path to and move towards.
    [SerializeField] private bool m_reachedTarget = false;      //!< Whether or not the dog has reached its target yet: collided with it.

    /** \fn Start
    *  \brief Instantiate the m_randomPoint object, set the dog's speed to walking, and its current target to the random point.
    */
    private void Start()
    {
        m_randomPoint = Instantiate(m_randomPoint, Vector3.zero, Quaternion.identity); //Instantiate a new empty game object in the scene for the random point.
        m_randomPoint.name = ("RandomPoint_" + name);
        m_randomPoint.transform.parent = m_randomPointStorage.transform;
        m_randomPoint.transform.position = Vector3.zero;
        m_currentSpeed = m_walkSpeed;
        m_currentTarget = m_randomPoint;
    }

    /** \fn SetSpeed
    *  \brief A publically accessible
    */
    public void SetSpeed(MoveSpeed pace)
    {
        switch (pace)
        {
            case (MoveSpeed.Crawling):
                m_currentSpeed = m_crawlSpeed;
                break;
            case (MoveSpeed.Walking):
                m_currentSpeed = m_walkSpeed;
                break;
            case (MoveSpeed.Running):
                m_currentSpeed = m_runSpeed;
                break;
        }
    }

    /** \fn FindPathTo
    *  \brief Generates an A* path to a specified object in the world.
    *  \param point The object to find a path to.
    */
    private void FindPathTo(GameObject point)
    {
        // Where to store the generated path:
        List<ASNode> path = new List<ASNode>(); 

        //Set values back to default:
        m_pathFound = false;
        m_RB.velocity = Vector3.zero;

        // Find path if the parameter point is set to a valid GameObject:
        if (point != null)
        {
            // Where to temporarily store the new A* search instance:
            AStarSearch tempAStar = m_aStarSearch;
            //Request a path between this object (the dog) and the input object parameter:
            path = tempAStar.RequestPath(transform.position, point.transform.position);
        }

        //If a path has actually been found, add the node positions to m_foundPath and set m_pathFound to true so the path can be followed:
        if ((path != null) && (path.Count > 0))
        {
            m_foundPath.Clear();  //Clear old m_foundPath.

            //Populate m_foundPath with new path:
            foreach (ASNode item in path)
            {
                m_foundPath.Add(item.m_worldPos);
            }
            m_pathFound =  true;
        }
    }

    /** \fn IsSetToRandom
     *  \brief Returns whether the script's target is currently set to the random point.
     */
    public bool IsSetToRandom() {return (m_currentTarget == m_randomPoint);  }
    /** \fn IsSetToObject
     *  \brief Returns whether the script's target is currently set to the object specified as the parameter.
     */
    public bool IsSetToObject(GameObject obj) {  return (m_currentTarget == obj); }

    /** \fn SetTarget
     *  \brief Sets the script's current target to the object specified as the parameter.
     */
    public void SetTarget(GameObject targ) { m_currentTarget = targ; FindPathTo(m_currentTarget); }
    /** \fn SetTargetToRandom
     *  \brief Sets the script's current target to the random point object.
     */
    public void SetTargetToRandom() { m_currentTarget = m_randomPoint; FindPathTo(m_currentTarget); }

    /** \fn FollowPathTo
     *  \brief Moves this GameObject along a found A* path to a specified point and returns as true once the dog has reached that object.
     */
    public bool AttemptToReachTarget()
    {
        // If the target has been set to the random point but a random position for the point hasn't been found yet, generate a random position for it until one has been found.
        if (IsSetToObject(m_randomPoint) && !m_randomNodeFound) { if (!GenerateRandomPointInWorld(m_randomPoint)) { return false; } }
        // If a path hasn't been found to the target yet, attempt to find one first before proceeding.
        if (!m_pathFound) { FindPathTo(m_currentTarget); return false; }

        // If the dog has reached its target, reset all the pathfinding values, stop the dog's velocity, then return true.
        if (m_reachedTarget)
        {
            DogLookAt(m_currentTarget.transform.position, true);
            m_randomNodeFound = false;
            m_pathFound = false;
            m_reachedTarget = false;
            m_RB.velocity = Vector3.zero;
            return true;
        }
        // If there are no more positions left in the path and the target still hasn't been reached, set m_pathFound to false and return so on the next function call a new path should be found.
        else if (((m_foundPath.Count == 0) && !m_reachedTarget))  
        {
            m_pathFound = false;        
            return false;
        }
        // If the dog isn't at its target and hasn't reached the next node position in its path yet either, move towards the next path position.
        else if (!m_collider.bounds.Contains(m_foundPath[0]))
        {
            DogLookAt(m_foundPath[0], false); // Look at the first position in the path list.
            MoveDog();                        // Move forwards towards the position.
            //if (transform.InverseTransformDirection(m_RB.velocity).z == 0) { m_pathFound = false; }
            return false;
        }
        // If the next path position IS within the bounds of the dog's collider, remove it from the path list for the dog to move towards the next node.
        else
        {
            m_foundPath.Remove(m_foundPath[0]);
            return false;
        }
    }

    /** \fn OnTriggerEnter
      *  \brief Detects when the dog has collided with (reached) its current target. This will allow AttemptToReachTarget() to return as true.
      */
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject == m_currentTarget) { m_reachedTarget = true; }
    }

    /** \fn GenerateRandomPointInWorld
     *  \brief Locates a random traversable node on the ground plane grid and sets the random point GameObject's position to the node's position.
     */
    public bool GenerateRandomPointInWorld(GameObject recievingObject)
    {
        AStarSearch tempAStar = m_aStarSearch; //A new temporary ground plane grid A* search. 
        ASNode randomNode = tempAStar.NodePositionInGrid(new Vector3(Random.Range(-m_aStarSearch.gridSize.x, m_aStarSearch.gridSize.x), 0.0f, Random.Range(-m_aStarSearch.gridSize.y, m_aStarSearch.gridSize.y))); //Locate a random node on the grid.

        if (tempAStar.IsTraversable(randomNode.m_worldPos))
        {
            if (recievingObject = m_randomPoint) { m_randomNodeFound = true; } // A random traversable node has been found.
            recievingObject.transform.position = randomNode.m_worldPos;        // Set the random point to the position of the random traversable node.
            return true;
        }

        // Return false until a traversable random node position is found.
        m_randomNodeFound = false;
        return false;
    }

    /** \fn MoveDog
    *  \brief Set the dog's rigidbody velocity to its current movement speed in its local direction of forwards - to the direction it's rotated towwards.
    */
    private void MoveDog()
    {
        m_RB.velocity = (transform.forward * (m_currentSpeed * Time.timeScale));
    }

    /** \fn DogLookAt
    *  \brief Rotates the dog object towards a specified position on the Y-axis.
    *  \param target The position to rotate towards.
    *  \param instant Whether rotation should be done at movement speed or instantly.
    */
    private void DogLookAt(Vector3 target, bool instant)
    {
        //The looking direction/rotation of the target from the dog object.
        var targetPosition = Quaternion.LookRotation(target - transform.position);

        //Remove rotations on axies other than Y.
        targetPosition.x = 0.0f;
        targetPosition.z = 0.0f;

        //Rotate dog towards target position.
        if (!instant) transform.rotation = Quaternion.RotateTowards(transform.rotation, targetPosition, (m_currentSpeed * 2.0f) * Time.timeScale);
        else transform.rotation = targetPosition;
    }

    /** \fn OnDrawGizmosSelected
    *  \brief Allows found A* path nodes to be drawn in the editor while the dog object is selected.
    */
    private void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        foreach (Vector3 node in m_foundPath)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawCube(node, new Vector3(1.5f, 0.1f, 1.5f));
        }
#endif
    }
}
