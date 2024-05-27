// Project:     Monster University for Daggerfall Unity
// Author:      DunnyOfPenwick
// Origin Date: January 2024

using System.Collections.Generic;
using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;


namespace MonsterUniversity
{


    public static class Utility
    {

        /// <summary>
        /// Gets all entities (including the player) within range of the specified location.
        /// </summary>
        public static List<DaggerfallEntityBehaviour> GetEntitiesInArea(Vector3 location, float range)
        {
            List<DaggerfallEntityBehaviour> entities = new List<DaggerfallEntityBehaviour>();

            Vector3 playerLocation = GameManager.Instance.PlayerEntityBehaviour.transform.position;
            float playerDistance = (playerLocation - location).magnitude;

            //GetNearbyObjects() gets objects near the player.  Make the range big enough to include
            //our range as well.
            float checkRange = playerDistance + range;

            List<PlayerGPS.NearbyObject> nearby = GameManager.Instance.PlayerGPS.GetNearbyObjects(PlayerGPS.NearbyObjectFlags.Enemy, checkRange);

            foreach (PlayerGPS.NearbyObject no in nearby)
            {
                DaggerfallEntityBehaviour behaviour = no.gameObject.GetComponent<DaggerfallEntityBehaviour>();
                if (Vector3.Distance(location, behaviour.transform.position) <= range)
                    entities.Add(behaviour);
            }

            //...add the player as well
            if (Vector3.Distance(location, playerLocation) <= range)
                entities.Add(GameManager.Instance.PlayerEntityBehaviour);

            return entities;
        }


        /// <summary>
        /// Determines is path from origin to target is unobstructed by terrain.
        /// </summary>
        public static bool HasPath(Vector3 origin, Vector3 target)
        {
            Vector3 direction = (target - origin).normalized;
            float distance = Vector3.Distance(origin, target);

            return !Physics.Raycast(origin, direction, out RaycastHit hitInfo, distance, 1);
        }


    } //class Utility


} //namespace
