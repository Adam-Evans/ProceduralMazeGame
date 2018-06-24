using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hexNode {

    //used to identify and locate a node within a grid. 
    [Header("Coordinates")]
    public int _x;
    public int _y;

    [Header("GameObject")]
    //numbers point to respective direction on a clock face. 
    public GameObject _one;
    public GameObject _three;
    public GameObject _five;
    public GameObject _seven;
    public GameObject _nine;
    public GameObject _eleven;
    public float _posX = 0;
    public float _posY = 0;
    public bool endNode = false;
    public bool wideRow = false;

    public hexNode(int x, int y)
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

    public bool isEnd
    {
        get { return endNode; }
        set { endNode = value; }
    }

    public bool isWide
    {
        get { return wideRow; }
        set { wideRow = value; }
    }


    //Used to store walls as objects - note 2 neighbouring nodes may share objects so be sure to check for null. 

    public GameObject One
    {
        get { return _one; }
        set { _one = value; }
    }


    public GameObject Three
    {
        get { return _three; }
        set { _three = value; }
    }

    public GameObject Five
    {
        get { return _five; }
        set { _five = value; }
    }

    public GameObject Seven
    {
        get { return _seven; }
        set { _seven = value; }
    }

    public GameObject Nine
    {
        get { return _nine; }
        set { _nine = value; }
    }

    public GameObject Eleven
    {
        get { return _eleven; }
        set { _eleven = value; }
    }
}
