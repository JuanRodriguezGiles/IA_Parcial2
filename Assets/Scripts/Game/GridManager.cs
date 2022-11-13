using System.Collections.Generic;

using Unity.Mathematics;

using UnityEngine;

using Random = UnityEngine.Random;

public class GridManager : MonoBehaviour
{
    public GameObject tile;
    public GameObject food;
    public Cell[,] cells;
    public List<GameObject> foods = new List<GameObject>();

    private int maxFood;
    private int rows, columns;
    
    public void CreateGrid(int rows, int columns)
    {
        this.rows = rows;
        this.columns = columns;
        cells = new Cell[rows, columns];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                cells[i, j] = new Cell();
                cells[i, j].position = new Vector2Int(j, i);
                Instantiate(tile, new Vector3(j, i, 0), quaternion.identity, transform);
            }
        }
    }

    public void CreateFood(int amount, int rows, int columns)
    {
        maxFood = amount;
        for (int i = 0; i < amount; i++)
        {
            int row = Random.Range(5, rows - 5);
            int column = Random.Range(5, columns - 5);

            if (cells[row, column].hasFood)
            {
                i--;
            }
            else
            {
                Vector3 pos = new Vector3(cells[row, column].position.x, cells[row, column].position.y, 0);
                GameObject go = Instantiate(food, pos, quaternion.identity, transform);
                foods.Add(go);
                cells[row, column].hasFood = true;
            }
        }
    }

    public void ReArrangeFood()
    {
        foreach (var f in foods)
        {
            Destroy(f);
        }
        foods.Clear();

        CreateFood(maxFood, rows, columns);
    }
}