using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BoardManager : MonoBehaviour
{
    public class CellData
    {
        public bool Passable;
        public CellObject ContainedObject;
    }    

    private CellData[,] m_BoardData;
    private Tilemap m_Tilemap;
    private Grid m_Grid;
  
    public int Width;
    public int Height;
    public Tile[] GroundTiles;
    public Tile[] BlockingTiles;
  
    // Easy: Make the amount of food random between two values
    // Medium: Random food sprite
    // Try to create multiple prefabs with the amount granted set to a different amount on each based on how big it appears it be.
    public FoodObject FoodPrefab;
    // public FoodObject[] FoodPrefabs;
    private List<Vector2Int> m_EmptyCellsList;

    // Easy: Multiple walls 
    // Medium: Change wall tiles based on how damaged they are
    public WallObject WallPrefab;
    public Enemy EnemyPrefab;
    public ExitCellObject ExitCellPrefab;

    public void Init()
    {
        m_Tilemap = GetComponentInChildren<Tilemap>();
        m_Grid = GetComponentInChildren<Grid>();
        //Initialize the list
        m_EmptyCellsList = new List<Vector2Int>();
        
        m_BoardData = new CellData[Width, Height];


        for (int y = 0; y < Height; ++y)
        {
            for(int x = 0; x < Width; ++x)
            {
                Tile tile;
                m_BoardData[x, y] = new CellData();
                
                if(x == 0 || y == 0 || x == Width - 1 || y == Height - 1)
                {
                    tile = BlockingTiles[Random.Range(0, BlockingTiles.Length)];
                    m_BoardData[x, y].Passable = false;
                }
                else
                {
                    tile = GroundTiles[Random.Range(0, GroundTiles.Length)];
                    m_BoardData[x, y].Passable = true;
                    
                    //this is a passable empty cell, add it to the list!
                    m_EmptyCellsList.Add(new Vector2Int(x, y));
                }
                
                m_Tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }
        
        //remove the starting point of the player! It's not empty, the player is there
        m_EmptyCellsList.Remove(new Vector2Int(1, 1));

        Vector2Int endCoord = new Vector2Int(Width - 2, Height - 2);
        AddObject(Instantiate(ExitCellPrefab), endCoord);
        m_EmptyCellsList.Remove(endCoord);

        GenerateFood();
        GenerateWall();
        GenerateEnemy();
    }  

   public Vector3 CellToWorld(Vector2Int cellIndex)
   {
       return m_Grid.GetCellCenterWorld((Vector3Int)cellIndex);
   }

   public CellData GetCellData(Vector2Int cellIndex)
   {
       if (cellIndex.x < 0 || cellIndex.x >= Width
           || cellIndex.y < 0 || cellIndex.y >= Height)
       {
           return null;
       }

       return m_BoardData[cellIndex.x, cellIndex.y];
   }

    public void SetCellTile(Vector2Int cellIndex, Tile tile)
    {
        m_Tilemap.SetTile(new Vector3Int(cellIndex.x, cellIndex.y, 0), tile);
    }

    public Tile GetCellTile(Vector2Int cellIndex)
    {
        return m_Tilemap.GetTile<Tile>(new Vector3Int(cellIndex.x,     cellIndex.y, 0));
    }

    void AddObject(CellObject obj, Vector2Int coord)
    {
        CellData data = m_BoardData[coord.x, coord.y];
        obj.transform.position = CellToWorld(coord);
        data.ContainedObject = obj;
        obj.Init(coord);
    }

    void GenerateWall()
    {
        int wallCount = Random.Range(6, 10);
        for (int i = 0; i < wallCount; ++i)
        {
            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];

            m_EmptyCellsList.RemoveAt(randomIndex);
            WallObject newWall = Instantiate(WallPrefab);
            AddObject(newWall, coord);
        }
    }

    void GenerateFood()
    {
        int foodCount = 5;
        for (int i = 0; i < foodCount; ++i)
        {
            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];

            m_EmptyCellsList.RemoveAt(randomIndex);
            FoodObject newFood = Instantiate(FoodPrefab);
            AddObject(newFood, coord);
        }
    } 

    void GenerateEnemy() // New: Generate enemies
    {
        int enemyCount = Random.Range(2, 5); // You can adjust the number of enemies
        for (int i = 0; i < enemyCount; ++i)
        {
            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];

            m_EmptyCellsList.RemoveAt(randomIndex);
            Enemy newEnemy = Instantiate(EnemyPrefab);
            AddObject(newEnemy, coord);
        }
    }

    public void Clean()
    {
        //no board data, so exit early, nothing to clean
        if(m_BoardData == null)
            return;


        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                var cellData = m_BoardData[x, y];

                if (cellData.ContainedObject != null)
                {
                    //CAREFUL! Destroy the GameObject NOT just cellData.ContainedObject
                    //Otherwise what you are destroying is the JUST CellObject COMPONENT
                    //and not the whole gameobject with sprite
                    Destroy(cellData.ContainedObject.gameObject);
                }

                SetCellTile(new Vector2Int(x,y), null);
            }
        }
    }

}
