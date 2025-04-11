using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Classe Node représentant un point dans le labyrinthe
public class Node
{
    public int x, z; // Coordonnées du noeud
    public int gCost, hCost; // Coût depuis le début (g) et estimation du coût jusqu'à la fin (h)
    public Node parent; // Noeud précédent dans le chemin

    // Constructeur : initialise les coordonnées et les coûts
    public Node(int x, int z)
    {
        this.x = x;
        this.z = z;
        gCost = int.MaxValue; // Initialise gCost à une valeur très grande
        hCost = 0; // Initialise hCost à 0
    }

    // Propriété pour calculer le coût total (fCost = gCost + hCost)
    public int fCost => gCost + hCost;

    // Méthode pour comparer deux noeuds (vérifie si les coordonnées sont identiques)
    public override bool Equals(object obj)
    {
        if (obj is Node other)
        {
            return x == other.x && z == other.z;
        }
        return false;
    }

    // Méthode pour obtenir le code de hachage du noeud : utilise les coordonnées
    public override int GetHashCode()
    {
        return (x, z).GetHashCode();
    }
}

// Classe Maze2 : génère un labyrinthe et trouve un chemin
public class Maze2 : MonoBehaviour
{
    public GameObject cube; 
    public int width = 30, depth = 30; 
    public byte[,] map; // Tableau représentant le labyrinthe (0 = chemin, 1 = mur)
    public int entranceX, entranceZ, exitX, exitZ; // Coordonnées de l'entrée et de la sortie

    // Méthode Start : exécutée au démarrage du jeu
    void Start()
    {
        InitializeMap(); // Initialise la carte avec des murs
        GenerateMaze(1, 1); // Génère le labyrinthe
        CreateEntranceAndExit(); // Crée l'entrée et la sortie
        DrawMap(); // Dessine le labyrinthe

        // Trouve un chemin de l'entrée à la sortie
        List<Node> path = FindPath(new Vector2(entranceX, entranceZ), new Vector2(exitX, exitZ));
        if (path != null)
        {
            DisplayPath(path); // Affiche le chemin trouvé
        }
        else
        {
            Debug.Log("Aucun chemin trouvé."); // Affiche un message si aucun chemin n'est trouvé
        }
    }

    // Initialise la carte avec des murs (tous les éléments sont à 1)
    void InitializeMap()
    {
        map = new byte[width, depth]; 
        for (int z = 0; z < depth; z++) 
            for (int x = 0; x < width; x++) 
                map[x, z] = 1; 
    }

    // Génère le labyrinthe en utilisant l'algorithme de parcours en profondeur aléatoire
    void GenerateMaze(int x, int z)
    {
        map[x, z] = 0; // Marque la cellule actuelle comme chemin

        // Directions possibles (haut, bas, gauche, droite)
        int[] dx = { 0, 0, -2, 2 }; 
        int[] dz = { -2, 2, 0, 0 }; 

        // Mélange les directions pour obtenir un labyrinthe aléatoire
        List<int> directions = new List<int> { 0, 1, 2, 3 }; 
        for (int i = 0; i < directions.Count; i++) 
        {
            int tmp = directions[i]; 
            int r = Random.Range(i, directions.Count); 
            directions[i] = directions[r]; 
            directions[r] = tmp; 
        }

        // Parcours les directions mélangées
        foreach (int i in directions) 
        {
            int newX = x + dx[i], newZ = z + dz[i];

            // Vérifie si la nouvelle cellule est valide et n'a pas encore été visitée
            if (newX > 0 && newX < width - 1 && newZ > 0 && newZ < depth - 1 && map[newX, newZ] == 1)
            {
                map[x + dx[i] / 2, z + dz[i] / 2] = 0; // Marque la cellule intermédiaire comme chemin
                GenerateMaze(newX, newZ); // Appel récursif pour explorer la nouvelle cellule
            }
        }
    }

    // Crée l'entrée et la sortie du labyrinthe
    void CreateEntranceAndExit()
    {
        entranceX = 0; 
        entranceZ = Random.Range(1, depth - 1);

        // Assure que l'entrée est un chemin
        while (map[entranceX + 1, entranceZ] == 1) 
        {
            entranceZ = Random.Range(1, depth - 1); 
        }
        map[entranceX, entranceZ] = 0; 

        exitX = width - 1;
        exitZ = Random.Range(1, depth - 1);

        // Assure que la sortie est un chemin
        while (map[exitX - 1, exitZ] == 1)
        {
            exitZ = Random.Range(1, depth - 1);
        }
        map[exitX, exitZ] = 0;
    }

    // Dessine le labyrinthe en instanciant des cubes pour les murs
    void DrawMap()
    {
        GameObject parent = new GameObject("Maze Walls"); 
        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++) 
            {
                if (map[x, z] == 1)
                {
                    GameObject wall = Instantiate(cube, new Vector3(x, 1, z), Quaternion.identity); 
                    wall.transform.parent = parent.transform;
                }
            }
        }
    }

    // Trouve un chemin en utilisant l'algorithme A*
    public List<Node> FindPath(Vector2 startPos, Vector2 targetPos)
    {
        Node startNode = new Node((int)startPos.x, (int)startPos.y) { gCost = 0 }; // Noeud de départ
        Node targetNode = new Node((int)targetPos.x, (int)targetPos.y); // Noeud d'arrivée

        List<Node> openSet = new List<Node> { startNode }; // Liste des noeuds à explorer
        HashSet<Node> closedSet = new HashSet<Node>(); // Liste des noeuds déjà explorés
        HashSet<Node> testedNodes = new HashSet<Node>(); // Ajout pour les noeuds testés

        // Boucle tant qu'il y a des noeuds à explorer
        while (openSet.Count > 0)
        {
            openSet.Sort((a, b) => a.fCost.CompareTo(b.fCost)); // Trie les noeuds par coût total (fCost)
            Node currentNode = openSet[0]; // Prend le noeud avec le coût total le plus faible
            openSet.RemoveAt(0); // Retire le noeud de la liste des noeuds à explorer
            closedSet.Add(currentNode); // Ajoute le noeud à la liste des noeuds explorés

            // Vérifie si le noeud actuel est le noeud d'arrivée
            if (currentNode.x == targetNode.x && currentNode.z == targetNode.z)
            {
                DisplayTestedNodes(testedNodes); // Affiche les noeuds testés
                return RetracePath(startNode, currentNode); // Reconstruit le chemin
            }

            // Parcours les voisins du noeud actuel
            foreach (Node neighbor in GetNeighbors(currentNode))
            {
                // Vérifie si le voisin est un mur ou a déjà été exploré
                if (map[neighbor.x, neighbor.z] == 1 || closedSet.Contains(neighbor))
                    continue;

                // Calcule le nouveau coût depuis le début
                int newCost = currentNode.gCost + 1;
                // Vérifie si le nouveau coût est inférieur au coût actuel du voisin
                if (newCost < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newCost; // Met à jour le coût depuis le début
                    neighbor.hCost = GetDistance(neighbor, targetNode); // Calcule l'estimation du coût jusqu'à la fin
                    neighbor.parent = currentNode; // Définit le parent du voisin
                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor); // Ajoute le voisin à la liste des noeuds à explorer
                        testedNodes.Add(neighbor); // Ajouter le noeud testé
                    }
                }
            }
        }
        return null; // Aucun chemin trouvé
    }

    // Reconstruit le chemin à partir du noeud d'arrivée en remontant les parents
    List<Node> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        // Remonte les parents jusqu'au noeud de départ
        while (currentNode != null)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse(); // Inverse la liste pour obtenir le chemin dans le bon ordre
        return path;
    }

    // Obtient les voisins d'un noeud
    List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();
        int[] dx = { 0, 0, -1, 1 }; // Décalages en x pour les voisins
        int[] dz = { -1, 1, 0, 0 }; // Décalages en z pour les voisins

        // Parcours les 4 directions possibles
        for (int i = 0; i < 4; i++)
        {
            int checkX = node.x + dx[i], checkZ = node.z + dz[i];
            // Vérifie si les coordonnées du voisin sont valides
            if (checkX >= 0 && checkX < width && checkZ >= 0 && checkZ < depth)
                neighbors.Add(new Node(checkX, checkZ)); // Ajoute le voisin à la liste
        }
        return neighbors;
    }

    // Calcule la distance de Manhattan entre deux noeuds
    int GetDistance(Node a, Node b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.z - b.z);
    }

    // Affiche le chemin trouvé en instanciant des cubes verts
    void DisplayPath(List<Node> path)
    {
        foreach (Node node in path)
        {
            GameObject pathBlock = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pathBlock.transform.position = new Vector3(node.x, 0.5f, node.z);
            pathBlock.GetComponent<Renderer>().material.color = new Color(0f, 0.5f, 0f); // Couleur verte
        }
    }

    // Affiche les noeuds testés en instanciant des cubes rouges
    void DisplayTestedNodes(HashSet<Node> testedNodes)
    {
        foreach (Node node in testedNodes)
        {
            GameObject testedBlock = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testedBlock.transform.position = new Vector3(node.x, 0.25f, node.z); // Place les cubes testés légèrement plus bas
            testedBlock.GetComponent<Renderer>().material.color = Color.red; // Couleur rouge
        }
    }
}