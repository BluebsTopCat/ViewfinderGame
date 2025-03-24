using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


[System.Serializable]
public class PlantWeightPair
{
    public FolliageType Plant;
    [Range(0,10)]
    public float Weight;

    public PlantWeightPair(FolliageType plant, int weight)
    {
        this.Plant = plant;
        this.Weight = weight;
    }

    public bool ignoreChunking => Plant.ignoreChunking;
    public float GetCrowdDist => Plant.crowding;
    public GameObject GetGameObject => Plant.meshVariants[Random.Range(0, Plant.meshVariants.Length)];
    public float GetScale => Random.Range(Plant.scaleVariation.x, Plant.scaleVariation.y);
    public float GetRotation => Random.Range(0, 360);

}


[CreateAssetMenu(fileName = "Data", menuName = "Foliage/Biome", order = 2)]
public class Biome : ScriptableObject
{
    public PlantWeightPair[] Plants;
}

