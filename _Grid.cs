using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid {

    public int width;
    public int height;
    public Node[] _nodes;

    public Grid(int w, int h)
    {
        width = w;
        height = h;
    }

    public void setupGrid()
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
}
