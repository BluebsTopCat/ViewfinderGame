using System.Collections;
using System.Collections.Generic;
using GD.MinMaxSlider;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "Foliage/FoliageType", order = 1)]
public class FolliageType : ScriptableObject
{
   public GameObject[] meshVariants;
   public bool ignoreChunking = false;
   public float crowding = 1f;
   [MinMaxSlider(0,2)]
   public Vector2 scaleVariation = new Vector2(1f, 1f);
   
}
