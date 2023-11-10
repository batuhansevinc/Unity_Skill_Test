using System;
using System.Collections.Generic;
using Assignment01.Enums;
using UnityEngine;

namespace Assignment01.Controller
{
    public class LevelController : MonoBehaviour
    {
        [Header("Level Endpoints")]
        //public GameObject startCell;
        //public GameObject endCell;

        [Header("Grid Settings")]
        [Range(1, 10)]
        public int gridSize;

        private GameObject[,] gridMatrix;
        private List<GridCell> connectedToSource = new List<GridCell>();
        private List<GameObject> validGroundObjects = new List<GameObject>();

        const float X_INTERVAL = 0.71f;
        const float Z_INTERVAL = 0.71f;

        public class GridCell
        {
            public GameObject cellObject;
            public Vector2Int gridPosition;
            public GridCell leftNeighbor, rightNeighbor, upNeighbor, downNeighbor;
            public List<GameObject> connectedPipes = new List<GameObject>();
            

        }

        private GridCell[,] gridCells;
        private GridCell selectedCell;

        private void Start()
        {
            InitializeGrid();
        }

        public void InitializeGrid()
        {
            gridCells = new GridCell[gridSize, gridSize];
            validGroundObjects.Clear(); // Önceki objeler varsa temizle

            for (int x = 0; x < gridSize; x++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    GameObject currentObject = transform.GetChild(x * gridSize + z).gameObject;
            
                    // GroundObject kontrolü
                    GroundObject groundObject = currentObject.GetComponent<GroundObject>();
                    if (groundObject != null && groundObject.SelectedObjectType != ObjectType.None)
                    {
                        validGroundObjects.Add(currentObject); // Valid listesine ekle

                        GridCell newCell = new GridCell();
                        newCell.cellObject = currentObject;
                        newCell.gridPosition = new Vector2Int(x, z);
                        gridCells[x, z] = newCell;
                    }
                }
            }

            // Komşuların atanması
            for (int x = 0; x < gridSize; x++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    GridCell currentCell = gridCells[x, z];
                    if(currentCell == null) continue; // Eğer bu hücre null ise komşularını atlamak için

                    if (x > 0 && gridCells[x - 1, z] != null)
                        currentCell.leftNeighbor = gridCells[x - 1, z];
                    if (x < gridSize - 1 && gridCells[x + 1, z] != null)
                        currentCell.rightNeighbor = gridCells[x + 1, z];
                    if (z < gridSize - 1 && gridCells[x, z + 1] != null)
                        currentCell.upNeighbor = gridCells[x, z + 1];
                    if (z > 0 && gridCells[x, z - 1] != null)
                        currentCell.downNeighbor = gridCells[x, z - 1];
                }
            }
        }

        
        /*private bool CheckIfGameIsCompleted()
        {
            List<GridCell> connectedCells = PerformBFS(GetCellFromObject(startCell));
            foreach(GridCell cell in connectedCells)
            {
                //Debug.Log("Connected cell position: " + cell.gridPosition);
            }
            GridCell endGridCell = GetCellFromObject(endCell);
    
            bool isCompleted = connectedCells.Contains(endGridCell);
            if (isCompleted)
            {
                Debug.Log("Game Completed!");
            }
    
            return isCompleted;
        }*/

        
        public int GetGridSize()
        {
            return gridSize;
        }
        public float GetXInterval()
        {
            return X_INTERVAL;
        }

        public float GetZInterval()
        {
            return Z_INTERVAL;
        }


        /*private void Update()
        {
            if (gridCells == null) return;

            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (CheckIfGameIsCompleted())
                {
                    Debug.Log("Game Completed!");
                    // Oyunu tamamlama işlemlerini burada yapabilirsiniz.
                }

                if (Physics.Raycast(ray, out hit))
                {
                    GridCell cell = GetCellFromObject(hit.transform.gameObject);
                    if (cell != null)
                    {
                        selectedCell = cell;

                        if (cell == gridCells[0, 0])
                        {
                            List<GridCell> newConnectedToSource = PerformBFS(cell);
                            connectedToSource.Clear();
                            connectedToSource.AddRange(newConnectedToSource);
                            foreach (GridCell connectedCell in connectedToSource)
                            {
                                // Eğer borunuz için animasyon kodu varsa buraya ekleyebilirsiniz.
                            }
                        }
                    }
                }
            }
        }*/

        /*private List<GridCell> PerformBFS(GridCell startCell)
        {
            List<GridCell> connectedCells = new List<GridCell>();
            Queue<GridCell> queue = new Queue<GridCell>();
            HashSet<GridCell> visited = new HashSet<GridCell>();

            queue.Enqueue(startCell);
            visited.Add(startCell);

            while (queue.Count > 0)
            {
                GridCell currentCell = queue.Dequeue();
                connectedCells.Add(currentCell);

                foreach (GridCell neighbor in GetConnectedNeighbors(currentCell))
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return connectedCells;
        }*/

        /*private IEnumerable<GridCell> GetConnectedNeighbors(GridCell cell)
        {
            List<GridCell> connectedNeighbors = new List<GridCell>();

            if (IsConnected(cell, cell.leftNeighbor))
                connectedNeighbors.Add(cell.leftNeighbor);
            if (IsConnected(cell, cell.rightNeighbor))
                connectedNeighbors.Add(cell.rightNeighbor);
            if (IsConnected(cell, cell.upNeighbor))
                connectedNeighbors.Add(cell.upNeighbor);
            if (IsConnected(cell, cell.downNeighbor))
                connectedNeighbors.Add(cell.downNeighbor);

            return connectedNeighbors;
        }*/

        private bool IsConnected(GridCell cellA, GridCell cellB)
        {
            if (cellA == null || cellB == null)
                return false;

            GameObject objectA = cellA.cellObject;
            GameObject objectB = cellB.cellObject;

            GroundObject groundObjectA = objectA.GetComponent<GroundObject>();
            GroundObject groundObjectB = objectB.GetComponent<GroundObject>();

            // Kaynak objesi için kontrol
            if (groundObjectA.SelectedObjectType == ObjectType.A)
            {
                if (groundObjectA.GetFacingDirection() == GroundObject.Direction.Down && groundObjectB.GetFacingDirection() == GroundObject.Direction.Up)
                    return true;
            }
    
            // Boru objesi için kontrol
            if (groundObjectA.SelectedObjectType == ObjectType.Pipe)
            {
                if ((groundObjectA.GetFacingDirection() == GroundObject.Direction.Up && groundObjectB.GetFacingDirection() == GroundObject.Direction.Down) ||
                    (groundObjectA.GetFacingDirection() == GroundObject.Direction.Down && groundObjectB.GetFacingDirection() == GroundObject.Direction.Up))
                    return true;
            }

            // A objesi için kontrol (Sadece aşağı yönlü bağlantı kabul edilir)
            if (groundObjectA.SelectedObjectType == ObjectType.A && groundObjectB.GetFacingDirection() == GroundObject.Direction.Down)
            {
                return true;
            }

            return false;
        }



        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Pipe"))
            {
                GridCell thisCell = GetCellFromObject(this.transform.GetChild(0).transform.GetChild(0).gameObject);
                if (thisCell != null)
                {
                    thisCell.connectedPipes.Add(other.gameObject);
                }
            }
        }

        private GridCell GetCellFromObject(GameObject obj)
        {
            for (int x = 0; x < gridSize; x++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    if (gridCells[x, z] != null && gridCells[x, z].cellObject == obj)
                    {
                        return gridCells[x, z];
                    }
                }
            }
            return null;
        }


        private void OnDrawGizmos()
        {
            if (connectedToSource != null)
            {
                Gizmos.color = Color.green;

                foreach (var connectedCell in connectedToSource)
                {
                    Gizmos.DrawSphere(connectedCell.cellObject.transform.position, 0.25f);
                }
            }
        }
    }
}
