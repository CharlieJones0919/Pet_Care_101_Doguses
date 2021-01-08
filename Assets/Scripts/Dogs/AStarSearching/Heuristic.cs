/** \file Heuristic.cs
*  \brief Functions return heuristic travel costs for A* searches.
*/
using System.Collections;
using UnityEngine;

/** \class Heuristic
*  \brief Contains the functions which return the heuristic calculations of costs to travel between 2 nodes in an A* search.
*/
static class Heuristic 
{
    /** \fn GetDistanceEuclidean
    *  \brief Returns the Euclidean distance cost to travel between 2 nodes. (𝐻 = √(〖δ𝑥^2〗+〖δ𝑦^2〗)).
    *  \param nodeA The first node.
    *  \param nodeB The second node.
    */
    public static float GetDistanceEuclidean(ASNode nodeA, ASNode nodeB)
    {
        // The difference in grid positions (a.k.a. the distance) between the 2 nodes on the X-axis to the power of 2. (〖δ𝑥^2〗).
        float distanceX = Mathf.Pow((nodeB.m_gridX - nodeA.m_gridX), 2);
        // The difference in grid positions (a.k.a. the distance) between the 2 nodes on the Y-axis to the power of 2. (〖δy^2〗).
        float distanceY = Mathf.Pow((nodeB.m_gridY - nodeA.m_gridY), 2);

        // Returns the final heuristic calculation after adding and squaring the distances. (𝐻 = √(〖δ𝑥^2〗+〖δ𝑦^2〗)).
        return Mathf.Sqrt(distanceX + distanceY);
    }

    /** \fn GetDistanceEuclideanNoSqr
    *  \brief Returns the Euclidean distance cost to travel between 2 nodes /without/ squaring the distance. (𝐻 =〖δ𝑥^2〗+〖δ𝑦^2〗).
    *  \param nodeA The first node.
    *  \param nodeB The second node.
    */
    public static float GetDistanceEuclideanNoSqr(ASNode nodeA, ASNode nodeB)
    {
        // The difference in grid positions (a.k.a. the distance) between the 2 nodes on the X-axis to the power of 2. (〖δ𝑥^2〗).
        float distanceX = Mathf.Pow((nodeB.m_gridX - nodeA.m_gridX), 2);
        // The difference in grid positions (a.k.a. the distance) between the 2 nodes on the Y-axis to the power of 2. (〖δy^2〗).
        float distanceY = Mathf.Pow((nodeB.m_gridY - nodeA.m_gridY), 2);

        // Returns the final heuristic calculation after adding the distances. (𝐻 =〖δ𝑥^2〗+〖δ𝑦^2〗).
        return (distanceX + distanceY);
    }

    /** \fn GetDistanceManhattan
    *  \brief Returns the Manhattan distance cost to travel between 2 nodes. (𝐻 = 𝐷 ∙ (𝑎𝑏𝑠(𝛿𝑥) + 𝑎𝑏𝑠(𝛿𝑦))).
    *  \param nodeA The first node.
    *  \param nodeB The second node.
    */
    public static float GetDistanceManhattan(ASNode nodeA, ASNode nodeB)
    {
        // The difference in grid positions (a.k.a. the distance) between the 2 nodes on the X-axis as an absolute/positive number. (𝑎𝑏𝑠(𝛿𝑥)).
        int distanceX = Mathf.Abs(nodeB.m_gridX - nodeA.m_gridX);
        // The difference in grid positions (a.k.a. the distance) between the 2 nodes on the Y-axis as an absolute/positive number. (𝑎𝑏𝑠(𝛿y)).
        int distanceY = Mathf.Abs(nodeB.m_gridY - nodeA.m_gridY);

        // The minimum cost of travel. (D).
        int cost = 1;

        // Returns the final heuristic calculation after adding the distances and multiplying them by the minimum distance. (𝐻 = 𝐷 ∙ (𝑎𝑏𝑠(𝛿𝑥) + 𝑎𝑏𝑠(𝛿𝑦))).
        return cost * (distanceX + distanceY);
    }

    /** \fn GetDistanceDiag
     *  \brief Returns the Diagonal distance cost to travel between 2 nodes. (𝐻 = 𝐷 ∙ 𝑚𝑎𝑥(𝑎𝑏𝑠(𝛿𝑥), 𝑎𝑏𝑠(𝛿𝑦))).
     *  \param nodeA The first node.
     *  \param nodeB The second node.
     */
    public static float GetDistanceDiag(ASNode nodeA, ASNode nodeB)
    {
        // The difference in grid positions (a.k.a. the distance) between the 2 nodes on the X-axis as an absolute/positive number. (𝑎𝑏𝑠(𝛿𝑥)).
        int distanceX = Mathf.Abs(nodeB.m_gridX - nodeA.m_gridX);
        // The difference in grid positions (a.k.a. the distance) between the 2 nodes on the Y-axis as an absolute/positive number. (𝑎𝑏𝑠(𝛿y)).
        int distanceY = Mathf.Abs(nodeB.m_gridY - nodeA.m_gridY);

        // The minimum cost of travel. (D).
        int cost = 1;

        // Returns the final heuristic calculation of the MAXIMUM distance multiplied by the minimum distance. (𝐻 = 𝐷 ∙ 𝑚𝑎𝑥(𝑎𝑏𝑠(𝛿𝑥), 𝑎𝑏𝑠(𝛿𝑦))).
        return cost * Mathf.Max(distanceX, distanceY);
    }

    /** \fn GetDistanceDiagShort
 *  \brief Returns the Diagonal Shortcut distance cost to travel between 2 nodes...
 *          𝐷2 ∙ 𝛿𝑥 + 𝛿𝑦 − 𝛿𝑥    𝒊𝒇 𝜹𝒙 ≤ 𝜹𝒚
 *  𝐻 = {
 *          𝐷2 ∙ 𝛿𝑦 + 𝛿𝑥 − 𝛿𝑦    𝒊𝒇 𝜹𝒙 > 𝜹𝒚
 *  \param nodeA The first node.
 *  \param nodeB The second node.
 */
    public static float GetDistanceDiagShort(ASNode nodeA, ASNode nodeB)
    {
        // The difference in grid positions (a.k.a. the distance) between the 2 nodes on the X-axis as an absolute/positive number. (𝛿𝑥).
        int distanceX = Mathf.Abs(nodeB.m_gridX - nodeA.m_gridX);
        // The difference in grid positions (a.k.a. the distance) between the 2 nodes on the Y-axis as an absolute/positive number. (𝛿y).
        int distanceY = Mathf.Abs(nodeB.m_gridY - nodeA.m_gridY);

        // Cost per travel. (D2 = √2 = 1.41).
        float cost = 1.41f;

        // The if statement dependant on returning the shortest diagonal distance. (𝐻 = {...).
        if (distanceX > distanceY) // If Y-axis distance is bigger or the same as X, return the X-axis distance. (𝒊𝒇 𝜹𝒙 > 𝜹𝒚).
        { 
            return (cost * distanceY) + (distanceX - distanceY); // (𝐻 = 𝐷2 ∙ 𝛿𝑦 + 𝛿𝑥 − 𝛿𝑦).
        }
        else // If X-axis distance is bigger, return the Y-axis distance. (𝒊𝒇 𝜹𝒙 ≤ 𝜹𝒚).
        {
            return (cost * distanceX) + (distanceY - distanceX); // (𝐻 = 𝐷2 ∙ 𝛿𝑥 + 𝛿𝑦 − 𝛿𝑥).
        }
    }
}
