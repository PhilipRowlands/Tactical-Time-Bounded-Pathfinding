using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System;

// taken from unitygems.com/astar-1-journey-start-single-step/
// and adapted so that navigations scripts are generally independent of the mesh

public class CreateNavMesh : MonoBehaviour {
	
	Bounds bounds;
	Vector3 topLeftCorner;
	int width, height;
	int boundsX, boundsZ;		// size of the bounds of the model
	
	public GridCell[,] cells;
	public float cellSize = 0.5f;
	
	int walkableLayer, orangeLayer, blueLayer, unwalkableLayer;
	List<GridCell> unwalkableNodes = new List<GridCell>();		// the cells that cannot be walked on
	
	// Use this for initialization
	public void Scan () {
		
		walkableLayer = LayerMask.NameToLayer("Walkable");
		blueLayer = LayerMask.NameToLayer("Blue");
		orangeLayer = LayerMask.NameToLayer("Orange");
		
		// get the bounds of the model and make a grid from it
		bounds = gameObject.collider.bounds;
		boundsX = (int) bounds.size.x;
		boundsZ = (int) bounds.size.z;
		
		// find the top left corner
		topLeftCorner = bounds.center - bounds.extents + new Vector3(0, bounds.size.y, 0);
		Debug.Log("Top left corner of mesh is " + topLeftCorner);
		
		width = Mathf.RoundToInt(bounds.size.x / cellSize);
		height = Mathf.RoundToInt(bounds.size.z / cellSize);
		
//		Debug.Log("Mesh height is " + height + "; width is " + width);
		
		cells = new GridCell[width, height];
		
		// scan for walkable terrain in each cell
		for (int x = 0; x < width; x++)
		{
			for(int y = 0; y < height; y++)
	        {
	            //Get the position for a ray
	            Vector3 currentPosition = topLeftCorner + new Vector3(x * cellSize, 0, y * cellSize);
	            RaycastHit hit;
	            //Create a cell for the grid
	            GridCell cell = new GridCell();
				cells[x,y] = cell;
//				cell.height = 2;
				
				currentPosition.x += cellSize / 2;
				currentPosition.z += cellSize / 2;
				cell.position = currentPosition;
			
	            //Cast the ray - main problem is that it only checks the corner of the cell, not the entire cell
				if(Physics.Raycast(currentPosition, -Vector3.up, out hit, bounds.size.y))
	            {
					
//					Debug.Log("Object at " + hit.point + " is in layer " + LayerMask.LayerToName(hit.collider.gameObject.layer) );
	                //The height of the highest item in the cell
	                cell.height = hit.point.y;
					cell.position.y = cell.height;
					
	                //Test if the thing we hit was walkable - either it's in the walkable layer,
					// or there's a collider that is set to isTrigger
					// adding in the team layers to make sure their locations aren't turned into unwalkable nodes
	                if(hit.collider.isTrigger == true || hit.collider.gameObject.layer == walkableLayer
						|| hit.collider.gameObject.layer == blueLayer
						|| hit.collider.gameObject.layer == orangeLayer )
					{
						
	                    //Flag the cell as walkable
	                    cell.walkable = true;
	                }
					else 
						unwalkableNodes.Add (cell);
	            }
				
	        }
		}
		
//		getWalkableCellCount();
		postProcessing();		// process the grid
	}
	
	
	// post-processing to add a bit of space around the edges of objects
	// this was originally optional
	public void postProcessing()
	{	
		// get the neighbours of each unwalkable cell
		foreach(GridCell cell in unwalkableNodes)
		{
			List<GridCell> neighbours = getNeighbours(cell);
			
			foreach(GridCell node in neighbours)
			{
				if (unwalkableNodes.Contains(node) == true)
					continue;
				
				if (node.walkable == true)
				{
					node.walkable = false;
				}
					
			}
		}
		
		// check how many nodes are walkable
		getWalkableCellCount();

	}
	
	// save the navigation mesh
	public void saveMesh()
	{
		if (cells == null)
			return;
		
		// store the mesh as an XML file - but it only accepts 1-dimensional arrays!
		string localDirectoryName = Directory.GetCurrentDirectory();
		string fileName = localDirectoryName+"\\Navmesh.xml";
		TextWriter meshStorage = new StreamWriter(fileName);
		XmlSerializer serial = new XmlSerializer(typeof(GridCell[,]));
		serial.Serialize(meshStorage, cells);
		meshStorage.Close();
		
	}
	
	public GridCell[,] getNavMesh()
	{
		GridCell[,] meshClone = (GridCell[,]) cells.Clone();
		
		return meshClone;
	}
	
	// converts a world position to a GridCell
	public GridCell getCellFromWorld(Vector3 position)
	{	
		position -= topLeftCorner;
		float x = position.x;
		float z = position.z;
		
		// account for the cell size
		x = Mathf.FloorToInt( x / cellSize );
		z = Mathf.FloorToInt( z / cellSize);
		
//		Debug.Log("X is " + x + ", Z is " + z );
//		x = Mathf.Abs(x);
//		z = Mathf.Abs (z);
		 
		GridCell cell = cells[ (int) x, (int) z];
		
		return cell;
		
	}
	
	// converts a GridCell's position to a world position
	public Vector3 getWorldPosition(GridCell cell)
	{
		Vector3 location = new Vector3(cell.position.x * cellSize, cell.height, cell.position.z * cellSize);
		
		return location + topLeftCorner;
	}

	// get the current neighbours of the node
	public List<GridCell> getNeighbours(GridCell thisNode)
	{
		List<GridCell> theList = new List<GridCell>();
		float distance = cellSize;
		
		float nodeX = thisNode.position.x;
		float nodeZ = thisNode.position.z;
		
		for (float x = nodeX - distance; x <= nodeX + distance; x++)
		{
			for (float z = nodeZ - distance; z <= nodeZ + distance; z++)
			{
//				Debug.Log(x + ", " + z);
				Vector3 temp = new Vector3(x, 0, z);
				
				// don't bother if the position is outside the navmesh cube
//				Debug.Log(bounds.Contains (temp) );					
				if (bounds.Contains(temp) == false)
					continue;
				
				else
				{
					GridCell neighbour = getCellFromWorld(temp);
//					GridCell neighbour = cells[(int ) x, (int) z]; 
				
					if (neighbour!= thisNode && neighbour.walkable == true )
						theList.Add(neighbour);
				}
			}
		}
//		Debug.Log("The node at " +thisNode.position + " has " + theList.Count + " neighbours");
		
		return theList;
	}
	
	// draws the navmesh in the scene view
	void OnDrawGizmosSelected()
	{
		if(cells == null || width == 0 || height == 0)
			return;
		
		for(var x = 0; x < width; x++)
		{
			for(var y = 0; y < height; y++)
			{
				var cell = cells[x,y];
				Gizmos.color = cell.walkable ? Color.green : Color.red;
//				var drawPosition = topLeftCorner + new Vector3(((float)x+0.5f) * cellSize, 0, ((float)y + 0.5f) * cellSize);
				var drawPosition = cell.position;
				
				drawPosition.y = cell.height;
				
				Gizmos.DrawCube(drawPosition, 
					Vector3.one * cellSize * 0.7f);
			}
		}
	}
	
	void getWalkableCellCount()
	{
				// debugging to check if the cells are actually being made walkable
		int walkableCellCount = 0;
		int totalCells = 0;
		foreach (GridCell cell in cells)
		{
			totalCells++;
			if (cell.walkable == true )
			{
				walkableCellCount++;
			}
			
			
		}
		Debug.Log(walkableCellCount + " walkable cells out of " + totalCells);
	}

}
