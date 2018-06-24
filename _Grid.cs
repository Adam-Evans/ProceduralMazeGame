using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid {

    public int width;
    public int height;
    public Node[] _nodes;
    public hexNode[,] _hexNodes;

    public Grid(int w, int h)
    {
        width = w;
        height = h;
    }

    public void setupQuadGrid()
    {
        _nodes = new Node[width * height];
        int yPos = 0;
        int xPos= 0;
        int xCount = 0;
        for (int x = 0; x < width * height; x++)
        {
            if (xCount == width)
            {
                xCount = 0;
                yPos++;
                xPos -= width;
            }
            Node tempNode = new Node(xPos, yPos);
            _nodes[x] = tempNode;
            xCount++;
            xPos++;
        }
    }

    public void setupHexGrid()
    {
        int count = 0;
        _hexNodes = new hexNode[width, height];
        Debug.Log("node count: " + _hexNodes.Length);

        for (int y = 0; y < height; y++)
        {
            if (y % 2 == 0)
            {
                for (int x = 0; x < width - 1; x++)
                {
                    hexNode tempNode = new hexNode(x, y);
                    if (x == width - 2)
                    {
                        tempNode.isEnd = true;
                    }
                    _hexNodes[x, y] = tempNode;
                    count++;
                }
            }
            else
            {
                for (int x = 0; x < width; x++)
                {
                    hexNode tempNode = new hexNode(x, y);
                    if (x == width - 1)
                    {
                        tempNode.isEnd = true;
                    }
                    tempNode.isWide = true;
                    _hexNodes[x, y] = tempNode;
                    count++;
                }
            }
        }

        /*
        for (int x = 0; x < width * height; x++)
        {
            hexNode tempNode = new hexNode(count, actualY);
            _hexNodes[x] = tempNode;

            count++;
            if (actualY % 2 == 0)
            {
                if (count == width - 2)
                {
                    count = 0;
                    actualY++;
                }
            }
            else
            {
                if (count == width - 1)
                {
                    count = 0;
                    actualY++;
                }
            }
        }
        */
       
    }
}
