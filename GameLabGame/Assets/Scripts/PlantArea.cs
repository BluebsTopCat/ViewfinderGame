using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlantArea : MonoBehaviour
{
    public Biome b;
    public float maxsteepness = .75f;
    public float density;
    public LayerMask validplacement;

    [InspectorButton("Delete")] public bool delete;
    // Start is called before the first frame update
    

    private void Delete()
    {
        delete = false;
        while(transform.childCount > 1)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }
    
    [InspectorButton("Generate")] public bool generate;
    private void Generate()
    {
        //Clear old
        generate = false;
        while(transform.childCount > 1) DestroyImmediate(transform.GetChild(0).gameObject);

        List<(Vector3, float)> objects = new List<(Vector3, float)>();

        int numberOfPlants = Mathf.RoundToInt((density / 10)  * transform.localScale.magnitude);
        
        for (int i = 0; i < numberOfPlants; i++)
        {
            PlantWeightPair p = b.Plants[getRandomWeighted(b)];
            
            Vector3 initialpos = Random.insideUnitSphere;

            Vector3 posadjusted = Vector3.zero;

            Vector3 spherePoint = transform.TransformPoint(initialpos + Vector3.up);
        
            RaycastHit h;
            Ray r = new Ray(spherePoint, Vector3.down);
            if (Physics.Raycast(r, out h, transform.localScale.magnitude * 2, validplacement)
                && !foundCloseTuple((h.point, p.GetCrowdDist), objects) && Vector3.Dot(h.normal, Vector3.up) > maxsteepness)
            {

                posadjusted = h.point;
            }
            else
            {
                continue;
            }
            
            GameObject output = PrefabUtility.InstantiatePrefab(p.GetGameObject, this.transform) as GameObject;

            output.transform.position = posadjusted;
            output.transform.rotation = p.GetRotation;
            output.transform.localScale = transform.InverseTransformVector(Vector3.one * p.GetScale); 
            output.transform.parent = this.transform;
            objects.Add((output.transform.position, p.GetCrowdDist));
        }
        
    }

  
    private int getRandomWeighted(Biome biome)
    {
        float accumulatedWeight = 0;
        List<(float, float, int)> weights = new List<(float, float, int)>();
        for(int i = 0; i< biome.Plants.Length; i++)
        {
            weights.Add((accumulatedWeight, accumulatedWeight + biome.Plants[i].Weight,i));
            accumulatedWeight += biome.Plants[i].Weight;
        }

        float random = Random.Range(0, accumulatedWeight);
        foreach ((float, float, int) p in weights)
        {
            if (random >= p.Item1 && random < p.Item2) return p.Item3;
        }

        return -1;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.matrix = Gizmos.matrix * transform.localToWorldMatrix;
        Gizmos.DrawWireSphere(Vector3.zero, 1);
    }

    bool foundCloseTuple((Vector3, float) a, List<(Vector3, float)> elements)
    {
        for (int i = 0; i < elements.Count; i++)
        {
            if (Vector3.Distance(a.Item1, elements[i].Item1) < Mathf.Max(a.Item2, elements[i].Item2))
            {
                return true;
            }
        }

        return false;
    }

}
