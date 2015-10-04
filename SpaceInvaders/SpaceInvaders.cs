﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.IO;

class SpaceInvaders
{
	// here are all variables that we need to be used more often in both the Main and the additional methods.
	const int MaxHeight = 30;
	const int MaxWidth = 70;
	const int FieldWidth = MaxWidth / 6; // I made the field smaller because otherwise you cannot catch all the enemies.

	static int PlayerPositionX = FieldWidth / 2;
	static int PlayerPositionY = MaxHeight - 2;

	static List<int[]> enemies = new List<int[]>(); // the enemies and shots are List because this list holds all the enemies and shots currently on the field so they can drawn. Each enemy and object consists of PositionX and PositionY that is why they are saved in List from int array.
	static List<int[]> shots = new List<int[]>();

	const char playerSymbol = 'W'; // it looks the most as a spaceship to me
	const char enemySymbol = '@'; // looks the angriest
	const char shotSymbol = '|'; // just random shots

	static readonly ConsoleColor Blue = ConsoleColor.Blue;

	//Level details
	static int PauseDivider = 16;//changing count of enemies depending on level;
	static int lives = 3;
	static int pause; // here I am adjusting the enemies being spawn because there were too many.
	static int winnedScoresInLevel;//counting points at each level
	static int scoresToWin = 10;// the count of scores that are needed to go to next level
	static int level;
	static int numberOfLevels = 3;

	static Random generator = new Random(); // this is the generator for the starting position of the enemies.

	static bool wonLevel = false;
	//bool values for wining game and level;

	static int sleepingParameter = 100;
	static bool enemiesAreFrozen = false;


	static void Main()
	{
		Console.Title = "!---> Space Invaders <---!"; //sets the title to display in the console bar
													  // I set the size of the Console it can be changed easily from the constants above
		Console.BufferHeight = Console.WindowHeight = MaxHeight;
		Console.BufferWidth = Console.WindowWidth = MaxWidth;
		PlayingLevel();// using param level as flag to change difficulty;
	}




	static void PlayingLevel()
	{
		bool frozenUsed = false;
		while (lives > 0)
		{
			// Draw
			SpawnEnemies(enemiesAreFrozen);
			DrawField();

			// Logic
			while (Console.KeyAvailable)
			{
				var keyPressed = Console.ReadKey(true);

				while (Console.KeyAvailable) //cleans read key buffer when a lot of keys are pressed
				{
					Console.ReadKey(true);
				}

				if (keyPressed.Key == ConsoleKey.RightArrow || keyPressed.Key == ConsoleKey.D)
				{
					if (PlayerPositionX < FieldWidth)
					{
						PlayerPositionX++;
					}
				}
				else if (keyPressed.Key == ConsoleKey.LeftArrow || keyPressed.Key == ConsoleKey.A)
				{
					if (PlayerPositionX > 0)
					{
						PlayerPositionX--;
					}
				}
				else if (keyPressed.Key == ConsoleKey.DownArrow || keyPressed.Key == ConsoleKey.S)
				{
					if (PlayerPositionY < MaxHeight - 2)
					{
						PlayerPositionY++;
					}
				}
				else if (keyPressed.Key == ConsoleKey.UpArrow || keyPressed.Key == ConsoleKey.W)
				{
					if (PlayerPositionY > 1)
					{
						PlayerPositionY--;
					}
				}
				else if (keyPressed.Key == ConsoleKey.Spacebar)
				{
					shots.Add(new int[] { PlayerPositionX, PlayerPositionY });
				}
				else if (keyPressed.Key == ConsoleKey.NumPad0)
				{
					if (!frozenUsed)
					{
						Thread freeze = new Thread(Freeze());
						freeze.Start();
					}
					frozenUsed = true;
				}
			}

			UpdatingShotPosition();
			Collision();

			if (!enemiesAreFrozen)
			{
				UpdatingEnemyPosition();
				Collision();
			}

			Thread.Sleep(sleepingParameter);
			Console.Clear();
			// Redrawing
			DrawField();

			// checking if the 
			wonLevel = winnedScoresInLevel >= scoresToWin;

			if (wonLevel)
			{
				level++;
				GoToNextLevel(level, numberOfLevels);

			}
		}

		WriteLabel(new StreamReader(@"..\..\msg_game_over.txt"));

		Console.ReadLine();
		Environment.Exit(0);
	}

	static void GoToNextLevel(int level, int numberOfLevels)
	{

		if (level > numberOfLevels)
		{

			WriteLabel(new StreamReader(@"..\..\msg_you_won.txt"));

			Console.ReadLine();
			Environment.Exit(0);
		}

		PrintStringAtCoordinates(20, 12, Blue, "PRESS ENTER TO GO TO THE NEXT LEVEL");
		var keyPressed = Console.ReadKey();
		while (true)
		{
			keyPressed = Console.ReadKey();

			if (keyPressed.Key == ConsoleKey.Enter)
			{
				Console.Clear();
				winnedScoresInLevel = 0;
				wonLevel = false;
				ConfigurateLevelDetails();
				break;
			}

		}
	}

	static void ConfigurateLevelDetails()
	{
		enemies.Clear();
		shots.Clear();
		PlayerPositionX = FieldWidth / 2;
		PlayerPositionY = MaxHeight - 2;
		PauseDivider -= 2;
		sleepingParameter -= 10;
		lives++;
	}

	static void DrawResultTable()
	{
		PrintStringAtCoordinates(20, 4, Blue, "SPACE INVADERS");
		PrintStringAtCoordinates(20, 6, Blue, $"Lives: {lives}");
		PrintStringAtCoordinates(20, 7, Blue, $"Level: {level}");
		PrintStringAtCoordinates(20, 8, Blue, $"Next level after {scoresToWin - winnedScoresInLevel} enemies kills");

	}
	static void UpdatingShotPosition()
	{
		shots.ForEach(shot => shot[1]--);
	}

	static void UpdatingEnemyPosition()
	{
		enemies.ForEach(enemy => enemy[1]++);
	}

	static void Collision()
	{
		List<int> enemiesToRemove = new List<int>();
		List<int> shotsToRemove = new List<int>();
		List<int[]> enemiesLeft = new List<int[]>();
		List<int[]> shotsLeft = new List<int[]>();
		EnemiesTakingLife(enemiesToRemove);
		EnemiesVsShots(enemiesToRemove, shotsToRemove);
		UpdatingTheEnemiesList(enemiesLeft, enemiesToRemove); // here we're getting the new list of enemies after the collision
		UpdatingTheShotsList(shotsLeft, shotsToRemove);
		shots = shotsLeft;
		enemies = enemiesLeft;
	}

	static void UpdatingTheShotsList(List<int[]> shotsLeft, List<int> shotsToRemove)
	{
		for (int i = 0; i < shots.Count; i++)
		{
			if (shotsToRemove.Contains(i))
			{
				continue;
			}
			if (shots[i][1] < 1)
			{
				continue;
			}
			shotsLeft.Add(shots[i]);
		}
	}

	static void UpdatingTheEnemiesList(List<int[]> enemiesLeft, List<int> enemiesToRemove)
	{
		for (int i = 0; i < enemies.Count; i++)
		{
			if (enemiesToRemove.Contains(i))
			{
				continue;
			}
			enemiesLeft.Add(enemies[i]);
		}
	}

	static void EnemiesVsShots(List<int> enemiesToRemove, List<int> shotsToRemove)
	{
		for (int i = 0; i < shots.Count; i++)
		{
			int theEnemyCollidedWithAShot = enemies.FindIndex(enemy => enemy[0] == shots[i][0] && enemy[1] == shots[i][1]);
			if (theEnemyCollidedWithAShot >= 0)
			{
				enemiesToRemove.Add(theEnemyCollidedWithAShot);
				shotsToRemove.Add(i);
				winnedScoresInLevel++;
			}

		}
	}

	static void EnemiesTakingLife(List<int> enemiesToRemove)
	{
		for (int index = 0; index < enemies.Count; index++)
		{
			if ((enemies[index][0] == PlayerPositionX && enemies[index][1] == PlayerPositionY) || enemies[index][1] >= MaxHeight - 2)
			{
				lives--;
				DrawAtCoordinates(new[] { enemies[index][0], enemies[index][1] }, ConsoleColor.DarkRed, 'X');
				enemiesToRemove.Add(index);
			}

		}
	}




	static void FieldBarrier()
	{
		for (int i = 1; i < MaxHeight - 2; i++)
		{
			DrawAtCoordinates(new int[] { FieldWidth + 1, i }, Blue, '|');
		}
	}

	static void DrawField()
	{
		DrawEnemies();
		DrawShots();
		DrawPlayer();
		DrawResultTable();
		FieldBarrier();
	}

	static void DrawPlayer()
	{
		int[] playerPosition = { PlayerPositionX, PlayerPositionY };
		ConsoleColor playerColor = Blue;
		DrawAtCoordinates(playerPosition, playerColor, playerSymbol);
	}

	static void DrawShots()
	{
		foreach (var shot in shots)
		{
			DrawAtCoordinates(new[] { shot[0], shot[1] }, ConsoleColor.Red, shotSymbol);
		}
	}

	static void DrawEnemies()
	{
		foreach (var enemy in enemies)
		{
			DrawAtCoordinates(new[] { enemy[0], enemy[1] }, ConsoleColor.Red, enemySymbol);
		}
	}
	static void DrawAtCoordinates(int[] objectPosition, ConsoleColor objectColor, char objectSymbol)
	{
		Console.SetCursorPosition(objectPosition[0], objectPosition[1]);
		Console.ForegroundColor = objectColor;
		Console.WriteLine(objectSymbol);
		Console.CursorVisible = false;
	}

	static void PrintStringAtCoordinates(int stringPositionX, int stringPositionY, ConsoleColor stringColor, string message)
	{
		Console.SetCursorPosition(stringPositionX, stringPositionY);
		Console.ForegroundColor = stringColor;
		Console.WriteLine(message);
		Console.CursorVisible = false;
	}
	static void SpawnEnemies(bool frozen)
	{
		if (!frozen)
		{
			if (pause % PauseDivider == 0)
			{
				int spawningWidth = generator.Next(0, FieldWidth);
				int spawningHeight = generator.Next(0, MaxHeight / 6);
				enemies.Add(new[] { spawningWidth, spawningHeight });
				pause = 0;
			}
			pause++;
		}

	}
	static ThreadStart Freeze()
	{
		ThreadStart freeze = () =>
		{
			Stopwatch sb = new Stopwatch();
			int millieSecondsOfFreeze = 4000;
			sb.Start();
			while (sb.ElapsedMilliseconds < millieSecondsOfFreeze)
			{
				enemiesAreFrozen = true;
			}
			enemiesAreFrozen = false;
		};
		return freeze;
	}

	static void WriteLabel(StreamReader file) //flag
	{
		Console.Clear();
		int y = MaxHeight / 2 - 5;
		int x = MaxWidth / 5;
		using (file)
		{
			string line;
			while (true)
			{
				line = file.ReadLine();
				if (string.IsNullOrEmpty(line))
				{
					break;
				}
				Console.SetCursorPosition(x, y);
				Console.WriteLine(line);

				y++;
			}
		}
	}

}
