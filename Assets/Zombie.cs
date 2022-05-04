using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : MonoBehaviour
{
    public int hp = 6;

    public AudioSource shot;

    List<GameObject> Players;
    void Start()
    {
        // find all players
        Players = GameObject.FindObjectsOfType<GameObject>().Where(c => c.GetComponent<Player>() != null).ToList();
    }

    Vector3 lastSeenPlayerLocation = new Vector3(999, 999);
    List<Vector3> path = new List<Vector3>();
    void Update()
    {
        // find closest player, that is visible
        GameObject closest_visible_player = null;
        float closest_visible_player_distance = 999999;
        foreach (GameObject c in Players.Where(c => Pathfinding.CheckSight(transform.position, c.transform.position)))
        {
            if (closest_visible_player == null)
                closest_visible_player = c;
            if (Vector3.Distance(transform.position, c.transform.position) < closest_visible_player_distance)
            {
                closest_visible_player = c;
                closest_visible_player_distance = Vector3.Distance(transform.position, c.transform.position);
            }
        }
        // find any closest player
        GameObject closest_any_player = null;
        float closest_any_player_distance = 999999;
        foreach (GameObject c in Players)
        {
            if (closest_any_player == null)
                closest_any_player = c;
            if (Vector3.Distance(transform.position, c.transform.position) < closest_any_player_distance)
            {
                closest_any_player = c;
                closest_any_player_distance = Vector3.Distance(transform.position, c.transform.position);
            }
        }



        // if found a player that is visible
        if (closest_visible_player == null == false)
        {
            lastSeenPlayerLocation = closest_visible_player.transform.position;
            // walk  towards closest player, if not close already
            if (closest_visible_player_distance > 1.25f)
            {
                float speed = 2;
                Vector3 dir = (closest_visible_player.transform.position - transform.position).normalized;
                transform.position += dir * speed * Time.deltaTime;

                Debug.DrawLine(transform.position, closest_visible_player.transform.position, Color.red);
            }
            // otherwise deal damage to the player we're infront of
            else
            {
                closest_visible_player.GetComponent<Player>().playerhp -= 2 * Time.deltaTime;
            }
        }
        // we've seen a player somewhere but lost them      the value is set to 999 999 as a "null"
        else if (lastSeenPlayerLocation == new Vector3(999, 999) == false)
        {
            float speed = 2;
            Vector3 dir = (lastSeenPlayerLocation - transform.position).normalized;
            transform.position += dir * speed * Time.deltaTime;
            Debug.DrawLine(transform.position, lastSeenPlayerLocation, Color.green);

            // if reached last seen location, reset it
            if (Vector3.Distance(transform.position, lastSeenPlayerLocation) < 1.25f)
            {
                lastSeenPlayerLocation = new Vector3(999, 999);
            }
        }
        // dont see anything, follow waypoints
        else if (closest_any_player == null == false)
        {
            // if path empty or first point not visible get new path
            if (path.Count == 0 || Pathfinding.CheckSight(transform.position, path[0]) == false)
            {
                path = Pathfinding.GetWayPoints(transform.position, closest_any_player.transform.position);
            }
            // go to next point in path
            else
            {
                float speed = 2;
                Vector3 dir = (path[0] - transform.position).normalized;
                transform.position += dir * speed * Time.deltaTime;
                Debug.DrawLine(transform.position, path[0], Color.blue);

                // reached the point in the path, remove it from the path
                if (Vector3.Distance(transform.position, path[0]) < 1.25f)
                {
                    path.RemoveAt(0);
                }
            }
        }

        if (hp <= 0)
        {
            closest_any_player.GetComponent<Player>().money += 100;
            Debug.Log("money gained");
            Destroy(gameObject);
        }
    }
}