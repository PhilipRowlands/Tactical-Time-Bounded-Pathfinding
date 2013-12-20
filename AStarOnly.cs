using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

// this enables A* pathfinding for the blue team
// adapted from unitygems.com/astar-1-journey-start-single-step/ and Wikipedia's pseudocode on A*

public class AStarOnly : MonoBehaviour {
	
	public Vector3 startPosition, destination, currentPosition, previousPosition, nextPosition;
	GridCell currentNode, startNode, targetNode, previousNode;
	
	public Transform mapMesh;
	CreateNavMesh mapNodes;
	GridCell[,] nodeList;
	
	public List<Vector3> path;			// the path is a list of nodes
	
	int gScore;		// cost from start along best known path, AKA estimated total cost from start to goal
	int fScore;		// estimated total cost from start to goal through current position
	
	int tempGScore, tempFScore;

	// Use this for initialization
	void Start () {
		
		mapMesh = GameObject.Find("NavMesh").transform;
		
		mapNodes = (CreateNavMesh) mapMesh.GetComponent<CreateNavMesh>();
		mapNodes.Scan();
		nodeList = mapNodes.getNavMesh();
		
	}
	
	// set the agent's destination
	void setDestination(Vector3 position)
	{
//		Debug.Log(gameObject.name + " is looking for a path to " + position);
		// set the new destination, clear the old path and create a new one
		destination = position;
		cancelPath();
		
		path = new List<Vector3>();
		
		findPath(transform.position, destination);
		
	}
	
	
	// finds a path between a starting point and the destination
	public void findPath(Vector3 position, Vector3 end)
	{

		startPosition = position;
		
		Dictionary<GridCell, int> openSet = new Dictionary<GridCell, int>();
		List<GridCell> closedSet = new List<GridCell>();
		Dictionary<GridCell, GridCell> cameFrom = new Dictionary<GridCell, GridCell>();
		
		startNode = mapNodes.getCellFromWorld(position);
		targetNode = mapNodes.getCellFromWorld(end);
		openSet.Add (startNode, fScore);
		cameFrom.Add(startNode, startNode);
		
		gScore = 0;
		fScore = gScore + getHeuristicCostEstimate(startNode, targetNode);
		
		while (openSet.Count > 0)
		{
			// select the node with the lowest fScore
			currentNode = findLowestFScore(openSet);
			previousNode = cameFrom[currentNode];
			
			
			if (currentNode == targetNode 
//				|| DateTime.Now.Second - startTime.Second >= 2
				)
			{
				
//				Debug.Log("Found the target node at " + DateTime.Now);
//				CancelInvoke("callFindPath");
				openSet.Clear();
				closedSet.Clear();
				buildPath(cameFrom, currentNode);		// build the actual path
				break;
			}
			
			openSet.Remove(currentNode);
			closedSet.Add(currentNode);
			
			List<GridCell> neighbourList = mapNodes.getNeighbours(currentNode);
			
			foreach(GridCell cell in neighbourList )
			{
				tempGScore = gScore + getGScore(currentNode, cell);
				tempFScore = tempGScore + getHeuristicCostEstimate(cell, targetNode);
				
				

				if ( (closedSet.Contains(cell) == true && tempFScore >= fScore ) || cell.walkable == false )
				{
//					Debug.Log("The node at " + cell.position + " isn't suitable. Skipping to the next node");
					continue;
				}
				
				if ( openSet.ContainsKey(cell) == false || tempFScore < fScore)
				{
					// the previous node for each potential neighbour is the current node
					cameFrom[cell] = currentNode;
//					Debug.Log("The previous node to " + cell.position + " is at " + cameFrom[cell].position);
					gScore = tempGScore;
					fScore = tempFScore;
					
					if (openSet.ContainsKey(cell) == false)
						openSet.Add(cell, fScore);
				}
				
			}
			
		}
	}
	
	// get the neighbours of the current nodes
	List<GridCell> getCurrentNodeNeighbours(GridCell currentNode)
	{
		List<GridCell> theList = new List<GridCell>();
		float distance = mapNodes.cellSize;			// the distance between each node
		
		Vector3 currentNodePosition = currentNode.position;
		float currentNodex = currentNodePosition.x;
		float currentNodez = currentNodePosition.z;
		
		for (float x = currentNodex - distance; x <= currentNodex + distance; x++)
		{
			for (float z = currentNodez - distance; z <= currentNodez + distance; z++)
			{
				
				x = Mathf.RoundToInt(x);
				z = Mathf.RoundToInt(z);
				GridCell node = mapNodes.getCellFromWorld(new Vector3(x, 0, z));
				if (node.walkable == true)
					theList.Add(node);
			}
		}
		theList.Remove(currentNode);
//		Debug.Log("The current node has " +theList.Count + " walkable neighbours");
		return theList;
		
	}
	
	// called once a frame
//	void Update()
//	{	
//		
//		if (Input.GetKey(KeyCode.H) )
//		{
//			showPath();
//		}
//		
//	}
	
	// this will return the approximate straight line distance between two nodes
	// it uses Manhattan distances for speed
	int getHeuristicCostEstimate(GridCell firstNode, GridCell secondNode)
	{
		Vector3 node1 = firstNode.position;
		Vector3 node2 = secondNode.position;
//		int estimate = (int) Vector3.Distance(firstNode.position, secondNode.position);
		int estimate = Mathf.RoundToInt( Mathf.Abs (node1.x - node2.x) + Mathf.Abs(node1.z - node2.z) );

		return estimate;
	}
	
	// this is the cost of moving onto a node
	// it is 14 for diagonal movements and 10 for non-diagonal
	// using ints avoids rounding errors
	int getGScore(GridCell firstNode, GridCell secondNode)
	{
		int score = secondNode.moveCost;	// the default cost
		
		// get the positions of each node and then see if they are diagonally adjacent
		Vector3 firstPosition = firstNode.position;
		Vector3 secondPosition = secondNode.position;
		float xDifference = Mathf.Abs (firstPosition.x - secondPosition.x);
		float zDifference = Mathf.Abs (firstPosition.z - secondPosition.z);
//		Debug.Log(xDifference + "," + zDifference);
		
		if (xDifference > 0 && zDifference > 0)
			score = 14;
		else
			score = 10;
		
		return score;
	}
	
	
	// returns the path for external scripts
	public List<Vector3> getPath()
	{
		return path;
	}
	
	// build the path 
	void buildPath(Dictionary<GridCell, GridCell> listOfNodes, GridCell endNode)
	{
		
		GridCell theNode;
		Debug.Log(gameObject.name + " is building the path from " + startNode.position + " to " + endNode.position);
//		Debug.Log(listOfNodes.Count);
		
		theNode = endNode;
		path.Add(endNode.position);
		
		// loop through the path. Using a for loop prevents an infinite while loop
		// and if the path is more than 300 nodes long, it is TOO long
		for (int i = 0; i < listOfNodes.Count; i++)
		{
			// get the node that came before the current node, and check if it's the starting node
			previousNode = listOfNodes[theNode];
			if (previousNode == startNode)
				break;
			else
			{
				if (path.Contains(previousNode.position) == false )
					path.Insert(0, previousNode.position);
				theNode = previousNode;
			}
		}
//		showPath();
		transform.SendMessage("setPath", path);		
	}
	
	
	// find the node on the openSet with the lowest fScore
	GridCell findLowestFScore(Dictionary<GridCell, int> theSet)
	{
		GridCell cell;
		int lowestScore = theSet.Values.Min();
		
		foreach(KeyValuePair<GridCell, int> pair in theSet)
		{
			if (pair.Value == lowestScore)
			{
				cell = pair.Key;
				return cell;
			}
			
		}
		cell = currentNode;	// if the lowest cost can't be found, set it to the current node
		
//		Debug.Log("Lowest cost node is " + cell + " at" + cell.position);
		return cell;
	}
	
	// show the path on the screen
	void showPath()
	{
		if (path.Count == 0)
			return;
		
		Vector3 pathZero = path[0];
		Color teamColour;
		if (transform.gameObject.layer == LayerMask.NameToLayer("Blue") )
			teamColour = Color.blue;
		else if (transform.gameObject.layer == LayerMask.NameToLayer("Orange") )
			teamColour = Color.yellow;
		else
			teamColour = Color.white;
		
		foreach (Vector3 position in path)
		{
			Debug.DrawLine(pathZero, position, teamColour);
			pathZero = position;
		}
		// get a screen shot if needed
		Application.CaptureScreenshot("AStarPathTest.jpg");
	}
	
	// clear the path if need be
	void cancelPath()
	{
		path.Clear();
		path = null;
	}
	
}
