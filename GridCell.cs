// taken and adapted from unitygems.com/astar-1-journey-start-single-step/

using UnityEngine;
using System;

public class GridCell {
	public bool walkable;
	public float height;				// the height is probably it's y-coordinate
	
	public Vector3 position;
	public int moveCost = 1;			// the default movement cost per node
	
	// this adds a discount to the heuristic if the cell is next to an obstacle
	// it should be limited to [0,1]
	public float tacticalDiscount = 0.0f;
	
}
