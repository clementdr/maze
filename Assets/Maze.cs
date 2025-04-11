using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Maze : MonoBehaviour
{
    // On récupère un object qui servira de mur 
    public GameObject cube;


    // La taille de notre labyrinthe
    public int width = 30; //x length
    public int depth = 30; //z length
    // Au démarrage, on crée une double boucle qui va instancier un bloc sur chaque case 
    void Start()
    {
        for (int z = 0; z < depth; z++)
            for (int x = 0; x < width; x++)
            {
                // Les emplacements (cases) sont définis par une position
                Vector3 pos = new Vector3(x, 1, z);
                // On génère un object à l'endroit de la pos avec l'orientation standard
                Instantiate(cube, pos, Quaternion. identity);
            }
    }

    // Update is called once per frame
    void Update ()
    {
        
    }
}