/** \file DogAI.cs
*  \brief Implements instances and usage of any AI employing scripts/classes.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/** \class DogAI
*  \brief Contains functions which utilise the A* pathfinding, RBS, FSM, BT and FIS classes/functionalities.
*/
public class DogAI : MonoBehaviour
{
    public GameObject testTarget;

    [SerializeField] private float m_moveSpeed;
    [SerializeField] private float m_rotationSpeed;
    [SerializeField] private float m_foundDistance;

    [SerializeField] private GameObject groundPlane;

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////// A* PATHFINDING VARIABLES /////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    private AStarSearch m_aStarSearch;
    private List<Vector3> m_foundPath = new List<Vector3>();
    
    private bool m_randomNodeFound = true;
    private GameObject m_randomPoint;
    private bool reachedPoint = false;

   // private List<GameObject> m_beds = new List<GameObject>();

    /////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////// RULE BASED SYSTEM VARIABLES ////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////
    public Dictionary<string, bool> facts = new Dictionary<string, bool>(); //!< Stores the dog's currently identified "facts" which determine the outcome of the behaviour tree, then subsequently the FSM's current state.

    //public Rules rules = new Rules(); //!< Rules which are defined by specified conditions which then result in a true or false return value.
    private float healthcheck;      


    // Start is called before the first frame update
    void Start()
    {
        m_aStarSearch = groundPlane.GetComponent<AStarSearch>();
        m_randomPoint = new GameObject("RandomPoint");
    }

    //// Update is called once per frame
    void Update()
    {
        // FollowPathToRandomPoint();
        FollowPathTo(testTarget);
    }

    /** \fn FindPathTo
    *  \brief Generates an A* path to a specified point in the world.
    *  \param point The point to find a path to.
    */
    public void FindPathTo(GameObject point)
    {
        //Where to store the generated path.
        List<ASNode> path = new List<ASNode>();

        //Find path if the parameter point is set to a GameObject.
        if (point != null)
        { 
            //Where to temporarily store the new A* search.
            AStarSearch tempAStar = m_aStarSearch;

            //Request a path between this object (the dog) and the input object parameter
            path = tempAStar.RequestPath(this.gameObject, point);
        }

        //If path is not null and more than 1.
        if ((path != null) && (path.Count >= 1))
        {
            //Clear old m_foundPath.
            m_foundPath.Clear();

            //Populate m_foundPath with new path
            foreach (ASNode item in path)
            {
                m_foundPath.Add(item.m_worldPos);
            }
        }
    }

    /** \fn FollowPathTo
     *  \brief Moves this GameObject along a found A* path.
     *  \param point The point to follow the path to.
     */
    public void FollowPathTo(GameObject point)
    {
        //Find path if the parameter point is set to a GameObject.
        if (point != null)
        {
            //If a path has been found.
            if (m_foundPath.Count > 0)
            {
                foreach (Vector3 node in m_foundPath)
                {
                    //  Debug.Log(Vector3.Distance(transform.position, node));
                    if (Vector3.Distance(transform.position, node) > m_foundDistance)
                    {
                        DogLookAt(node);
                        MoveDog(node);
                        return;
                    }
                    else
                    {
                        m_foundPath.Remove(node);
                        return;
                    }
                }
            }
            else 
            {
                m_randomNodeFound = false;

                if (Vector3.Distance(transform.position, point.transform.position) > m_foundDistance)
                {
                    FindPathTo(point);
                }

                DogLookAt(point.transform.position);
                return;
            }
        }
    }

    public void FollowPathToRandomPoint()
    {
        if (m_randomNodeFound)
        {
            FollowPathTo(m_randomPoint);
        }
        else  //If a random node hasn't been found yet.
        {
            StartCoroutine(GenerateRandomPointInWorld());
        }
    }

    IEnumerator GenerateRandomPointInWorld()
    {
        AStarSearch tempAStar = m_aStarSearch;

        ASNode randomNode = tempAStar.NodePositionInGrid(new Vector3(Random.Range(-90, 90), 0, Random.Range(-90, 90)));
        Vector3 consPos = Vector3.zero;

        while (!randomNode.m_traversable)
        {
            randomNode = tempAStar.NodePositionInGrid(new Vector3(Random.Range(-90, 90), 0, Random.Range(-90, 90)));
            yield return new WaitForEndOfFrame();
        }

        m_randomNodeFound = true;
        m_randomPoint.transform.position = randomNode.m_worldPos;
    }

    private void MoveDog(Vector3 target)
    {
       transform.position += transform.forward * m_moveSpeed * Time.deltaTime;
    }

    private void DogLookAt(Vector3 target)
    {
        var q = Quaternion.LookRotation(target - transform.position);
        q.x = 0.0f;
        q.z = 0.0f;

        transform.rotation = Quaternion.RotateTowards(transform.rotation, q, m_rotationSpeed);
    }

    private void OnDrawGizmosSelected()
    {
    #if UNITY_EDITOR
        foreach (Vector3 node in m_foundPath)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawCube(node, new Vector3(3.0f * 0.25f, 0.1f, 3.0f * 0.25f));
        }
    #endif
    }

}
