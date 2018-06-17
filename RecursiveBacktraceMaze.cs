using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecursiveBacktraceMaze : MonoBehaviour
{

    //wall dimensions
    [Header("Wall Dimensions")]
    public float _WallWidth = 4;
    public float _WallHeight = 4;
    public float _wallDepth = 0.1f;

    [Header("Wall Settings")]
    public wallType _wallType = wallType.quad;

    //grid settings
    [Header("Grid Options")]
    public int _gridWidth = 25;
    public int _gridLength = 25;

    //doesn't need to be public, just easier to debug this way.
    [Header("Grid")]
    public Grid _grid; 
    public GameObject wallModel; // prefab used to instantiate walls
    GameObject wallHolder; // empty game object used to parent walls to for organisation. 

    [Header("Quad Wall settings")]
    public GameObject[,] _wallsHorizontal; //2d array of horizontal walls for quad maze
    public GameObject[,] _wallsVert; // 2d array of vertical walls for quad maze

    [Header("Hex Wall settings")]
    public GameObject[,] _wallsHexVert; // 2d array of vertical walls
    public GameObject[,] _wallsHexDown; // 2d array of down sloped walls
    public GameObject[,] _wallsHexUp; // 2d array of upwards sloped walls


    // Use this for initialization
    void Start()
    {
        //this should work but doesn't for some reason, for now add wall prefab in inspector. 
        //wallModel = (GameObject)Resources.Load("Assets/Prefabs/Walls/WallPrefab") as GameObject;

        _grid = new Grid(_gridWidth, _gridLength);
        _grid.setupGrid();

        if (_wallType == wallType.quad)
        {
            generateWallsQuad();
            Thread.Sleep(100);
            setNodeNeighoursQuad();
            StartCoroutine(backTraceMazeQuad());
        }
        else if (_wallType == wallType.hex)
        {
            generateWallsHex();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            StopAllCoroutines();
            StartCoroutine(NewMaze());
        }
    }

    /// <summary>
    /// This method will create two grids of walls represented as two 2d vectors, totalling 4 walls per node allowing for each to have neighbours set to a wall gameobject which may later be removed 
    /// during the backtrace when generating the maze.
    /// </summary>
    void generateWallsQuad()
    {
        //need to generate two 2d vectors of walls, one for no rotation, one for 90 rotation.
        _wallsHorizontal = new GameObject[_gridWidth, _gridLength + 1];
        _wallsVert = new GameObject[_gridWidth + 1, _gridLength];
        wallHolder = new GameObject();
        wallHolder.transform.position = new Vector3(0, 0, 0);
        wallHolder.name = "wallHolder";
        wallHolder.transform.rotation = wallModel.transform.rotation;
        //for walls: vertical = (grid length * width) + length, horizontal "" "" + width
        for (int y = 0; y < _gridLength + 1; y++)
        {
            for (int x = 0; x < _gridWidth + 1; x++)
            {
                // for unity, z = forward, x = right, y = up. so x,y is actually x,z.
                if (x == _gridWidth && y < _gridLength)
                {
                    //special case to close outer loop.
                    GameObject temp1 = Instantiate(wallModel, new Vector3(x * _WallWidth - _WallWidth, 0, y * _WallHeight - (_WallHeight / 2)), transform.rotation, wallHolder.transform);
                    temp1.transform.Rotate(-90, 90, 0);
                    temp1.name = "vert" + x + "," + y;
                    _wallsVert[x, y] = temp1;
                }
                else if (y == _gridLength && x < _gridWidth)
                {
                    //special case to close outer loop
                    GameObject temp2 = Instantiate(wallModel, new Vector3(x * _WallWidth - (_WallWidth / 2), 0, y * _WallHeight - _WallHeight), transform.rotation, wallHolder.transform);
                    _wallsHorizontal[x, y] = temp2;
                    temp2.name = "hor" + x + "," + y;
                    temp2.transform.Rotate(-90, 180, 0);
                }
                else if (y < _gridLength && x < _gridWidth)
                {
                    GameObject tempGO = Instantiate(wallModel, new Vector3(x * _WallWidth - (_WallWidth / 2), 0, y * _WallHeight - _WallHeight), transform.rotation, wallHolder.transform);
                    tempGO.name = "hor" + x + "," + y;
                    _wallsHorizontal[x, y] = tempGO;
                    tempGO.transform.Rotate(-90, 180, 0);

                    tempGO = Instantiate(wallModel, new Vector3(x * _WallWidth - _WallWidth, 0, y * _WallHeight - (_WallHeight / 2)), transform.rotation, wallHolder.transform);
                    tempGO.transform.Rotate(-90, 90, 0);
                    tempGO.name = "vert" + x + "," + y;
                    _wallsVert[x, y] = tempGO;
                }
            }
        }
        //expected outcome should be grid width * grid height + (grid width + height) / 2

    }

    /// <summary>
    /// Hex grid is organised as a honeycomb: lines will alternate between widths of gridwidth and gridwidth -1.Each node will require 6 neighbours and share at least 3 walls between 2 nodes allowing
    /// for more freedom in navigating random paths.
    /// Vertical hex lines alternate as: gridwidth + 1 and gridwidth + 2. For simplicity it is easier to force an even gridlength to have equal numbers of each possible width. 
    /// </summary>
    private void generateWallsHex()
    {
        if (_gridLength % 2 != 0)
            _gridLength += 1;
        //For ease of construction, the array marking coords of the wall should be the larger of the 2 possible values, 
        //leaving the extra as null. Later check if the index is null before applying to nodes

        //vert walls make left and right boundary so width + 1
        _wallsHexVert = new GameObject[_gridWidth + 1, _gridLength];
        //sloped walls make the top and bottom so length is +1
        _wallsHexDown = new GameObject[_gridWidth, _gridLength + 1];
        _wallsHexUp = new GameObject[_gridWidth, _gridLength + 1];

        wallHolder = new GameObject();
        wallHolder.transform.position = new Vector3(0, 0, 0);
        wallHolder.name = "wallHolder";
        wallHolder.transform.rotation = wallModel.transform.rotation;
        GameObject tempGO;
        Vector3 pos;
        Vector3 translate;
        float hexWidth = _WallWidth * (float)Mathf.Sqrt(3);
        float hexTriHeight = (hexWidth - _WallWidth) / 2;
        float difference = hexWidth - _WallWidth;
        float depthAdjust = 0.25f;

        bool alt = false;
        bool flip = false;
        int correction = 0;
        //Note: given that a hexagon can be broken down into 6 triangles, the radius and thus spacing of walls is found using the 90/60/30 rule giving radius = line width * root 3 (root 3 from sin60)
        for (int y = 0; y < _gridLength + 1; y++) // + 1 is a special case for boundaries only
        {
            for (int x = 0; x < _gridWidth + 1; x++)
            { 
                if (x == _gridWidth && y < _gridLength) // closing boundary - rightmost wall
                {
                    if (alt)
                    {
                        pos = new Vector3((x * hexWidth - ((hexWidth - _WallWidth) / 2) - depthAdjust) - (hexWidth / 2),
                           0, (y * hexWidth) - (hexWidth - _WallWidth) / 2 + 1 - correction);
                        tempGO = Instantiate(wallModel, pos, transform.rotation, wallHolder.transform);
                        tempGO.name = "vertalt" + x + "," + y;
                        tempGO.transform.Rotate(-90, 90, 0);
                        translate = new Vector3((_WallWidth * (float)Math.Sqrt(3) / 2), 0, 0);
                        tempGO.transform.Translate(translate);
                        tempGO.transform.parent = wallHolder.transform;
                    }                    
                }
                else if (y == _gridLength && x < _gridWidth) //top boundary case
                {
                    pos = new Vector3(x * hexWidth, 0, y * hexWidth - hexWidth - 1 - correction + _WallWidth / 2);
                    tempGO = Instantiate(wallModel, pos, transform.rotation, wallHolder.transform);
                    tempGO.name = "down" + x + "," + y;
                    tempGO.transform.parent = wallHolder.transform;
                    tempGO.transform.Rotate(-90, -150, 0);

                    pos = new Vector3(x * hexWidth - hexWidth / 2, 0, y * hexWidth - hexWidth -1 - correction + _WallWidth / 2);
                    tempGO = Instantiate(wallModel, pos, transform.rotation, wallHolder.transform);
                    tempGO.name = "up" + x + "," + y;
                    tempGO.transform.parent = wallHolder.transform;
                    tempGO.transform.Rotate(-90, -30, 0);
                }
                else if (y < _gridLength && x < _gridWidth)
                {
                    if (alt)
                    {
                        pos = new Vector3((x * hexWidth - ((hexWidth - _WallWidth) / 2) - depthAdjust) - (hexWidth / 2),
                            0,(y * hexWidth) - (hexWidth - _WallWidth) / 2 + 1 - correction);
                        tempGO = Instantiate(wallModel, pos, transform.rotation, wallHolder.transform);
                        tempGO.name = "vertalt" + x + "," + y;
                        tempGO.transform.Rotate(-90, 90, 0);
                        translate = new Vector3((_WallWidth * (float)Math.Sqrt(3) / 2), 0, 0);
                        tempGO.transform.Translate(translate);
                        tempGO.transform.parent = wallHolder.transform;

                        if (flip)
                        {
                            pos = new Vector3(x * hexWidth, 0, y * hexWidth - hexWidth - correction);
                            tempGO = Instantiate(wallModel, pos, transform.rotation, wallHolder.transform);
                            tempGO.name = "downaltflip" + x + "," + y;
                            tempGO.transform.parent = wallHolder.transform;
                            tempGO.transform.Rotate(-90, -30, 0);

                            pos = new Vector3(x * hexWidth - hexWidth / 2, 0, y * hexWidth - hexWidth - correction);
                            tempGO = Instantiate(wallModel, pos, transform.rotation, wallHolder.transform);
                            tempGO.name = "upaltflip" + x + "," + y;
                            tempGO.transform.parent = wallHolder.transform;
                            tempGO.transform.Rotate(-90, -150, 0);
                        }
                        else
                        {
                            pos = new Vector3(x * hexWidth - hexWidth / 2, 0, y * hexWidth - hexWidth - correction);
                            tempGO = Instantiate(wallModel, pos, transform.rotation, wallHolder.transform);
                            tempGO.name = "downalt" + x + "," + y;
                            tempGO.transform.parent = wallHolder.transform;
                            tempGO.transform.Rotate(-90, -150, 0);

                            pos = new Vector3(x * hexWidth, 0, y * hexWidth - hexWidth - correction);
                            tempGO = Instantiate(wallModel, pos, transform.rotation, wallHolder.transform);
                            tempGO.name = "upalt" + x + "," + y;
                            tempGO.transform.parent = wallHolder.transform;
                            tempGO.transform.Rotate(-90, -30, 0);
                        }
                    }
                    else
                    {
                        pos = new Vector3(x * hexWidth - ((hexWidth - _WallWidth) / 2) - depthAdjust,
                            0, (y * hexWidth) + (hexWidth - _WallWidth) / 2 - 1 - correction);
                        tempGO = Instantiate(wallModel, pos, transform.rotation, wallHolder.transform);
                        tempGO.name = "vert" + x + "," + y;
                        tempGO.transform.Rotate(-90, 90, 0);
                        translate = new Vector3((hexWidth / 2), 0, 0);
                        tempGO.transform.Translate(translate);
                        tempGO.transform.parent = wallHolder.transform;

                        if (y == 0 && x == 0)
                        {
                            if (flip)
                            {
                                pos = new Vector3(x * hexWidth - hexWidth / 2, 0, y * hexWidth - hexWidth + 1 - correction);
                                tempGO = Instantiate(wallModel, pos, transform.rotation, wallHolder.transform);
                                tempGO.name = "downflip" + x + "," + y;
                                tempGO.transform.parent = wallHolder.transform;
                                tempGO.transform.Rotate(-90, -30, 0);

                                pos = new Vector3(x * hexWidth, 0, y * hexWidth - hexWidth + 1 - correction);
                                tempGO = Instantiate(wallModel, pos, transform.rotation, wallHolder.transform);
                                tempGO.name = "upflip" + x + "," + y;
                                tempGO.transform.parent = wallHolder.transform;
                                tempGO.transform.Rotate(-90, -150, 0);
                            }
                            else
                            {
                                pos = new Vector3(x * hexWidth, 0, y * hexWidth - hexWidth + 1 - correction);
                                tempGO = Instantiate(wallModel, pos, transform.rotation, wallHolder.transform);
                                tempGO.name = "down" + x + "," + y;
                                tempGO.transform.parent = wallHolder.transform;
                                tempGO.transform.Rotate(-90, -150, 0);
                            }
                        }

                        else if (y == 0 && x == 24)
                        {
                            if (flip)
                            {
                                pos = new Vector3(x * hexWidth - hexWidth / 2, 0, y * hexWidth - hexWidth + 1 - correction);
                                tempGO = Instantiate(wallModel, pos, transform.rotation, wallHolder.transform);
                                tempGO.name = "downflip" + x + "," + y;
                                tempGO.transform.parent = wallHolder.transform;
                                tempGO.transform.Rotate(-90, -30, 0);

                                pos = new Vector3(x * hexWidth, 0, y * hexWidth - hexWidth + 1 - correction);
                                tempGO = Instantiate(wallModel, pos, transform.rotation, wallHolder.transform);
                                tempGO.name = "upflip" + x + "," + y;
                                tempGO.transform.parent = wallHolder.transform;
                                tempGO.transform.Rotate(-90, -150, 0);
                            }
                            else
                            {
                                pos = new Vector3(x * hexWidth - hexWidth / 2, 0, y * hexWidth - hexWidth + 1 - correction);
                                tempGO = Instantiate(wallModel, pos, transform.rotation, wallHolder.transform);
                                tempGO.name = "up" + x + "," + y;
                                tempGO.transform.parent = wallHolder.transform;
                                tempGO.transform.Rotate(-90, -30, 0);
                            }
                        }

                        else
                        {
                            if (flip)
                        {
                            pos = new Vector3(x * hexWidth - hexWidth / 2, 0, y * hexWidth - hexWidth + 1 - correction);
                            tempGO = Instantiate(wallModel, pos, transform.rotation, wallHolder.transform);
                            tempGO.name = "downflip" + x + "," + y;
                            tempGO.transform.parent = wallHolder.transform;
                            tempGO.transform.Rotate(-90, -30, 0);

                            pos = new Vector3(x * hexWidth, 0, y * hexWidth - hexWidth + 1 - correction);
                            tempGO = Instantiate(wallModel, pos, transform.rotation, wallHolder.transform);
                            tempGO.name = "upflip" + x + "," + y;
                            tempGO.transform.parent = wallHolder.transform;
                            tempGO.transform.Rotate(-90, -150, 0);
                        }
                        else
                        {
                            pos = new Vector3(x * hexWidth, 0, y * hexWidth - hexWidth + 1 - correction);
                            tempGO = Instantiate(wallModel, pos, transform.rotation, wallHolder.transform);
                            tempGO.name = "down" + x + "," + y;
                            tempGO.transform.parent = wallHolder.transform;
                            tempGO.transform.Rotate(-90, -150, 0);

                            pos = new Vector3(x * hexWidth - hexWidth / 2, 0, y * hexWidth - hexWidth + 1 - correction);
                            tempGO = Instantiate(wallModel, pos, transform.rotation, wallHolder.transform);
                            tempGO.name = "up" + x + "," + y;
                            tempGO.transform.parent = wallHolder.transform;
                            tempGO.transform.Rotate(-90, -30, 0);
                        }
                        }

                        
                    }


                   
                }
                flip = !flip;
            }
            alt = !alt;
            if (!alt)
            {
                correction += 2;
            }
        }
    }

    /// <summary>
    /// This method will set the north, east, south and west object for each node for a quad grid.
    /// </summary>
    private void setNodeNeighoursQuad()
    {
        for (int y = 0; y < _gridLength; y++)
        {
            for (int x = 0; x < _gridWidth; x++)
            {
                string name = string.Format("hor{0},{1}", x, y + 1);
                _grid._nodes[x + y * _gridWidth].north = GameObject.Find(name); //_wallsHorizontal[x, y + 1];
                name = string.Format("vert{0},{1}", x + 1, y);
                _grid._nodes[x + y * _gridWidth].east = GameObject.Find(name); //_wallsVert[x + 1, y];
                name = string.Format("hor{0},{1}", x, y);
                _grid._nodes[x + y * _gridWidth].south = GameObject.Find(name); //_wallsHorizontal[x, y];
                name = string.Format("vert{0},{1}", x, y);
                _grid._nodes[x + y * _gridWidth].west = GameObject.Find(name);//_wallsVert[x, y];
            }
        }
    }


    public IEnumerator backTraceMazeQuad()
    {
        //When navigating nodes, up = +gridWidth, down = -gridwidth, left = -1, right = +1
        System.Random rng = new System.Random();
        int index = 0;
        Node currentNode;
        List<int> stackIndexes = new List<int>();
        List<int> visited = new List<int>();
        visited.Add(index);
        //Add each node to visited as it is jumped to.
        do
        {
            currentNode = _grid._nodes[index];
            bool canUp = false;
            bool canDown = false;
            bool canRight = false;
            bool canLeft = false;
            int options = 0;
            //check what directions we can travel.
            if (index - _gridWidth > 0)
            {
                if (visited.Contains(index - _gridWidth))
                {
                    //Debug.Log("Down visited already");
                }
                else
                {
                    canDown = true;
                    options++;
                }
            }
            if (index + _gridWidth < _gridWidth * _gridLength)
            {
                if (visited.Contains(index + _gridWidth))
                {
                    //Debug.Log("Up visited already");
                }
                else
                {
                    canUp = true;
                    options++;
                }               
            }
            if (index % _gridWidth != 0)
            {
                if (visited.Contains(index - 1))
                {
                    //Debug.Log("Left visited already");
                }
                else
                {
                    canLeft = true;
                    options++;
                }                
            }
            if ((index + 1) % _gridWidth != 0)
            {
                if (visited.Contains(index + 1))
                {
                    //Debug.Log("Right visited already");
                }
                else
                {
                    canRight = true;
                    options++;
                }                
            }
            if (options > 0) //current cell has at least one unvisited neighbour.
            {
                if (!stackIndexes.Contains(index))
                {
                    stackIndexes.Add(index);
                }
                if (canUp && canRight && canDown && canLeft) //not on outer wall
                {
                    int roll = rng.Next(0, 4);
                    if (roll == 0)
                    {
                        index += _gridWidth;
                        currentNode.north.SendMessage("DestroyParent");
                    }
                    else if (roll == 1)
                    {
                        index += 1;
                        currentNode.east.SendMessage("DestroyParent");
                    }
                    else if (roll == 2)
                    {
                        index -= _gridWidth;
                        currentNode.south.SendMessage("DestroyParent");
                    }
                    else if (roll == 3)
                    {
                        index -= 1;
                        currentNode.west.SendMessage("DestroyParent");
                    }
                }

                else if (canUp && canRight && canLeft) //bottom wall
                {
                    int roll = rng.Next(0, 3);
                    if (roll == 0)
                    {
                        index += _gridWidth;
                        currentNode.north.SendMessage("DestroyParent");
                    }
                    else if (roll == 1)
                    {
                        index += 1;
                        currentNode.east.SendMessage("DestroyParent");
                    }
                    else if (roll == 2)
                    {
                        index -= 1;
                        currentNode.west.SendMessage("DestroyParent");
                    }
                }

                else if (canDown && canRight && canLeft) //top wall
                {
                    int roll = rng.Next(0, 3);
                    if (roll == 0)
                    {
                        index -= _gridWidth;
                        currentNode.south.SendMessage("DestroyParent");
                    }
                    else if (roll == 1)
                    {
                        index += 1;
                        currentNode.east.SendMessage("DestroyParent");
                    }
                    else if (roll == 2)
                    {
                        index -= 1;
                        currentNode.west.SendMessage("DestroyParent");
                    }
                }
                else if (canUp && canDown && canRight) //left wall
                {
                    int roll = rng.Next(0, 3);
                    if (roll == 0)
                    {
                        index += _gridWidth;
                        currentNode.north.SendMessage("DestroyParent");
                    }
                    else if (roll == 1)
                    {
                        index += 1;
                        currentNode.east.SendMessage("DestroyParent");
                    }
                    else if (roll == 2)
                    {
                        index -= _gridWidth;
                        currentNode.south.SendMessage("DestroyParent");
                    }
                }
                else if (canUp && canDown && canLeft) //right wall
                {
                    int roll = rng.Next(0, 3);
                    if (roll == 0)
                    {
                        index += _gridWidth;
                        currentNode.north.SendMessage("DestroyParent");
                    }
                    else if (roll == 1)
                    {
                        index -= 1;
                        currentNode.west.SendMessage("DestroyParent");
                    }
                    else if (roll == 2)
                    {
                        index -= _gridWidth;
                        currentNode.south.SendMessage("DestroyParent");
                    }
                }
                else if (canUp && canRight) //bottom left corner (ie origin)
                {
                    int roll = rng.Next(0, 2);
                    if (roll == 0)
                    {
                        index += _gridWidth;
                        currentNode.north.SendMessage("DestroyParent");
                    }
                    else if (roll == 1)
                    {
                        index += 1;
                        currentNode.east.SendMessage("DestroyParent");
                    }
                }
                else if (canUp && canLeft) //bottom right
                {
                    int roll = rng.Next(0, 2);
                    if (roll == 0)
                    {
                        index += _gridWidth;
                        currentNode.north.SendMessage("DestroyParent");
                    }
                    else if (roll == 1)
                    {
                        index -= 1;
                        currentNode.west.SendMessage("DestroyParent");
                    }
                }
                else if (canDown && canLeft) // top right corner
                {
                    int roll = rng.Next(0, 2);
                    if (roll == 0)
                    {
                        index -= _gridWidth;
                        currentNode.south.SendMessage("DestroyParent");
                    }
                    else if (roll == 1)
                    {
                        index -= 1;
                        currentNode.west.SendMessage("DestroyParent");
                    }
                }
                else if (canDown && canRight) //top left
                {
                    int roll = rng.Next(0, 2);
                    if (roll == 0)
                    {
                        index -= _gridWidth;
                        currentNode.south.SendMessage("DestroyParent");
                    }
                    else if (roll == 1)
                    {
                        index += 1;
                        currentNode.east.SendMessage("DestroyParent");
                    }
                }
                else if (canUp)
                {
                    index += _gridWidth;
                    currentNode.north.SendMessage("DestroyParent");
                }
                else if (canDown)
                {
                    index -= _gridWidth;
                    currentNode.south.SendMessage("DestroyParent");
                }
                else if (canRight)
                {
                    index += 1;
                    currentNode.east.SendMessage("DestroyParent");
                }
                else if (canLeft)
                {
                    index -= 1;
                    currentNode.west.SendMessage("DestroyParent");
                }             
                currentNode = _grid._nodes[index];
                visited.Add(index);
                yield return new WaitForSeconds(0.01f);
            }
            else //current cell has no unvisited neighbours, remove it from the stack and backtrack. 
            {
                if (stackIndexes.Count > 0)
                {
                    stackIndexes.Remove(index);
                    if (stackIndexes.Count != 0)
                    {
                        index = stackIndexes[stackIndexes.Count - 1];
                        currentNode = _grid._nodes[index];
                    }
                }
            }        
        } while (stackIndexes.Count > 0); //visited.Count < (_gridLength * _gridWidth) + ((_gridLength + _gridWidth) / 2)
        visited.Clear();
    }



    public IEnumerator NewMaze()
    {
        Destroy(wallHolder);
        yield return new WaitForSeconds(0.25f);
        _grid = null;
        _grid = new Grid(_gridWidth, _gridLength);
        _grid.setupGrid();
        yield return new WaitForSeconds(0.05f);
        if (_wallType == wallType.quad)
        {
            generateWallsQuad();
            yield return new WaitForSeconds(0.001f);
            setNodeNeighoursQuad();
            yield return new WaitForSeconds(0.001f);
            StartCoroutine(backTraceMazeQuad());
        }
    }

    public void OnDestroy()
    {
        Destroy(wallHolder);
        StopAllCoroutines();
    }

}

//enum used for deciding which type of wall structure to generate
public enum wallType
{
    quad,
    hex
}

public enum mazeType
{
    backtrace
}