using UnityEngine;
using System.Collections;
using System;
using System.IO;

// this handles the statistics and starts games
// it also places pickups at random places and times

public class Statistics : MonoBehaviour
{
	string mapName;
	string statsWriterPath;
	string fileName;
	StreamWriter sw;
	
	int blueKills, orangeKills;			// how many times the blue team has killed an orange, or vice versa
	int blueCaptures, orangeCaptures;	// how many times the blue team has captured the orange base, o.v.v.
	int blueCoins, orangeCoins;
	
	int totalKills = 0;		// the total kills 
	
	public int hoursToRun = 12;			// how long the program should run in hours
	int secondsToRun;					// the same time in seconds
	
	void Start()
	{
		mapName = Application.loadedLevelName;
		statsWriterPath = Directory.GetCurrentDirectory();		// this is where the stats will be written to
		fileName = statsWriterPath+"\\" + mapName + " stats.txt";
		
		sw = new StreamWriter(fileName, true);
		sw.AutoFlush = true;
		sw.WriteLine("Date started " + DateTime.Now);
		sw.WriteLine("");
		sw.WriteLine("");
		
		
		secondsToRun = hoursToRun * 3600;
		Debug.Log("The game finishes in " + secondsToRun + " seconds");
//		Invoke(finish, secondsToRun);
	}
	
	// this method finishes up the stats and quits the game
	void finish()
	{
		// print out the total stats
		sw.WriteLine("Date finished " + DateTime.Now);
		sw.WriteLine("Total kills by Blue: " + blueKills);
		sw.WriteLine("Total kills by Orange: " + orangeKills);
		sw.WriteLine("Orange base captured: " + blueCaptures);
		sw.WriteLine("Blue base captured: " + orangeCaptures);
		sw.WriteLine("Coins collected by Blue Team: " + blueCoins);
		sw.WriteLine("Coins collected by Orange Team: " + orangeCoins);
		
		// close the stream and quit
		sw.Close();
		Application.Quit();
	}
	
	
	// increment the statistics
	void incrementStatistics(string statistic)
	{
		
		if (statistic.Equals("Blue hasCapturedEnemyBase") )
			blueCaptures++;
		
		if (statistic.Equals("Orange hasCapturedEnemyBase") )
			orangeCaptures++;
		
		if (statistic.Equals("Blue hasKilledEnemy") )
		{
			blueKills++;
			totalKills++;
		}
		
		if (statistic.Equals("Orange hasKilledEnemy") )
		{
			orangeKills++;
			totalKills++;
		}
		
		if (statistic.Equals("Blue has picked up a coin") )
			blueCoins++;
		
		if (statistic.Equals("Orange has picked up a coin") )
			orangeCoins++;
		
		// if the total kills exceeds a specific amount, export the statistics
		if ( totalKills > 4 )
		{
			storeStatistics();
		}
	}
	
	// write the stats out
	void storeStatistics()
	{
		
		sw.WriteLine("Team\t Kills\t Coins picked up\t EnemyBaseCaptured");
		sw.WriteLine("Blue\t " + blueKills + "\t " + blueCoins + "\t " + blueCaptures);
		sw.WriteLine("Orange\t " + orangeKills + "\t " + orangeCoins + "\t " + orangeCaptures);
		sw.WriteLine("");			// empty line between the game stats
		
	}
}

