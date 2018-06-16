using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node {

    //used to identify and locate a node within a grid. 
    [Header("Coordinates")]
    public int _x;
    public int _y;

    [Header("GameObject")]
    public GameObject _north;
    public bool n = false;
    public bool e = false;
    public bool s = false;
    public bool w = false;
    public GameObject _east;
    public GameObject _south;
    public GameObject _west;
    public float _posX = 0;
    public float _posY = 0;

    public Node(int x, int y)
    {
        _x = x;
        _y = y;
    }

    public int X
    {
        get { return _x; }
        set { _x = value; }
    }

    public int Y
    {
        get { return _y; }
        set { _y = value; }
    }

    public float posX
    {
        get { return _posX; }
        set { _posX = value; }
    }

    public float posY
    {
        get { return _posY; }
        set { _posY = value; }
    }


    //Used to store walls as objects - note 2 neighbouring nodes may share objects so be sure to check for null. 
    public GameObject north
    {
        get { return _north; }
        set { _north = value; }
    }

    public void northVisited()
    {
        n = true;
    }

    public void eastVisited()
    {
        e = true;
    }

    public void southVisited()
    {
        s = true;
    }

    public void westVisited()
    {
        w = true;
    }

    public GameObject east
    {
        get { return _east; }
        set { _east = value; }
    }

    public GameObject south
    {
        get { return _south; }
        set { _south = value; }
    }

    public GameObject west
    {
        get { return _west; }
        set { _west = value; }
    }
}
