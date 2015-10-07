using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

class SpaceInvaders
{
	// Variables used in the methods.
	const int MaxHeight = 30;
	const int MaxWidth = 70;
	const int FieldWidth = MaxWidth / 6;

	static int playerPositionX = FieldWidth / 2;
	static int playerPositionY = MaxHeight - 2;

	static List<int[]> enemies = new List<int[]>();
	static List<int[]> shots = new List<int[]>();

	const char PlayerSymbol = 'W';
	const char EnemySymbol = '@';
	const char ShotSymbol = '|';

	const ConsoleColor Green = ConsoleColor.Green;

	// Level details
	static int pauseDivider = 16; //changing count of enemies depending on level;
	static int lives = 3;
	static int pause; // Adjustment of enemies being spawned
	static int winnedScoresInLevel; // counting points at each level
	static int scoresToWin = 10; // the count of scores that are needed to go to next level
	static int level = 1;
	static int numberOfLevels = 3;

	static Random generator = new Random(); // This is the generator for the starting position of the enemies.

	static bool wonLevel;
	static int timestep = 100; // Timestep in milliseconds. Determines how fast the enemies fall down.
	static bool frozenUsed;
	static bool enemiesAreFrozen;


	static void Main()
	{
		Console.Title = "!---> SoftUni Invaders <---!";
		Console.BufferHeight = Console.WindowHeight = MaxHeight;
		Console.BufferWidth = Console.WindowWidth = MaxWidth;

		DisplayStartScreen();
		PlayingLevel();
	}

	static void DisplayStartScreen()
	{
		// msg_startscreen.txt map:
		// Line 01-06: Stars
		// Line 07-09: Start screen art
		// Line 10-16: stars
		// Line 17-20: controls
		// Line 23	 : start message

		int padding = 7;
		Console.CursorVisible = false;

		Console.Write(new string('\n', 4)); // 4 lines of p adding so the stars don't feel glued to the top

		using (var file = new StreamReader(@"..\..\msg_startscreen.txt")) // print start screen
		{
			for (int line = 1; line <= 23; line++)
			{
				if (line < 7 || (line > 9 && line <= 16)) // draw stars with yellow color
				{
					Console.ForegroundColor = ConsoleColor.Yellow;
				}
				else if (line <= 9)
				{
					Console.ForegroundColor = ConsoleColor.White; // draw text with white
				}
				else
				{
					Console.ForegroundColor = ConsoleColor.Red; // draw start text with red
				}

				Console.WriteLine(new string(' ', padding) + file.ReadLine());
			}
		}

		ConsoleKeyInfo keyPressed = Console.ReadKey(true);
		while (keyPressed.Key != ConsoleKey.Enter)
		{
			keyPressed = Console.ReadKey(true);
		}
	}

	static void PlayingLevel()
	{
		Stopwatch syncTimer = Stopwatch.StartNew(); // timestep stopwatch

		while (lives > 0)
		{
			ReadPlayerInput();

			if (syncTimer.ElapsedMilliseconds % (timestep - (level * 10)) == 0) // difficulty rises as level rises
			{
				// Draw
				SpawnEnemies(enemiesAreFrozen);
				DrawField();

				// Logic
				UpdateShotPosition();
				Collision();

				if (!enemiesAreFrozen)
				{
					UpdatingEnemyPosition();
					Collision();
				}

				Console.Clear();

				// Redrawing
				DrawField();
			}

			CheckLevelStatus();
		}

		WriteLabel(new StreamReader(@"..\..\msg_game_over.txt"));

		Console.ReadKey();
		Environment.Exit(0);
	}

	static void CheckLevelStatus()
	{
		wonLevel = winnedScoresInLevel >= scoresToWin;

		if (wonLevel)
		{
			level++;
			GoToNextLevel();
		}
	}

	static void ReadPlayerInput()
	{
		while (Console.KeyAvailable)
		{
			var keyPressed = Console.ReadKey(true);

			while (Console.KeyAvailable) // cleans read key buffer when a lot of keys are pressed
			{
				Console.ReadKey(true);
			}

			switch (keyPressed.Key)
			{
				case ConsoleKey.RightArrow:
				case ConsoleKey.D:

					if (playerPositionX < FieldWidth)
					{
						playerPositionX++;
					}
					break;

				case ConsoleKey.LeftArrow:
				case ConsoleKey.A:

					if (playerPositionX > 0)
					{
						playerPositionX--;
					}
					break;

				case ConsoleKey.DownArrow:
				case ConsoleKey.S:

					if (playerPositionY < MaxHeight - 2)
					{
						playerPositionY++;
					}
					break;

				case ConsoleKey.UpArrow:
				case ConsoleKey.W:

					if (playerPositionY > 1)
					{
						playerPositionY--;
					}
					break;

				case ConsoleKey.Spacebar:

					shots.Add(new[] { playerPositionX, playerPositionY });
					break;

				case ConsoleKey.NumPad0:

					if (!frozenUsed)
					{
						Thread freeze = new Thread(Freeze());
						freeze.Start();
					}

					frozenUsed = true;
					break;
			}
		}
	}

	static void GoToNextLevel()
	{
		if (level > numberOfLevels)
		{

			WriteLabel(new StreamReader(@"..\..\msg_you_won.txt"));

			Console.ForegroundColor = ConsoleColor.Gray;
			string credits = "SoftUni Team Farthorl 2015 credits:";
            Console.SetCursorPosition(1, Console.BufferHeight-1);

			Console.WriteLine(credits);
			Console.WriteLine(" bacuty, Nezhdetov, Simooo93, Housey, krisitown, zhecho15");

			Console.ReadLine();
			Environment.Exit(0);
		}

		PrintStringAtCoordinates(20, 12, Green, "PRESS ENTER TO GO TO THE NEXT LEVEL");
		while (true)
		{
			var keyPressed = Console.ReadKey();

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
		playerPositionX = FieldWidth / 2;
		playerPositionY = MaxHeight - 2;
		pauseDivider -= 2;
		timestep -= 10;
		lives++;
	}

	static void DrawResultTable()
	{
		PrintStringAtCoordinates(20, 4, Green, "SOFTUNI INVADERS");
		PrintStringAtCoordinates(20, 6, Green, string.Format("Lives: {0}", lives));
		PrintStringAtCoordinates(20, 7, Green, string.Format("Level: {0}", level));
		PrintStringAtCoordinates(20, 8, Green, string.Format("Next level after {0} enemies kills", scoresToWin - winnedScoresInLevel));

	}
	static void UpdateShotPosition()
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

				new Thread(() => Console.Beep(300, 100)).Start();
			}
		}
	}

	static void EnemiesTakingLife(List<int> enemiesToRemove)
	{
		for (int index = 0; index < enemies.Count; index++)
		{
			if ((enemies[index][0] == playerPositionX && enemies[index][1] == playerPositionY) || enemies[index][1] >= MaxHeight - 2)
			{
				lives--;
				DrawAtCoordinates(new[] { enemies[index][0], enemies[index][1] }, ConsoleColor.DarkRed, 'X');
				enemiesToRemove.Add(index);
				new Thread(() => Console.Beep(100, 100)).Start();
			}

		}
	}

	static void FieldBarrier()
	{
		for (int i = 1; i < MaxHeight - 2; i++)
		{
			DrawAtCoordinates(new[] { FieldWidth + 1, i }, Green, '|');
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
		int[] playerPosition = { playerPositionX, playerPositionY };
		ConsoleColor playerColor = Green;
		DrawAtCoordinates(playerPosition, playerColor, PlayerSymbol);
	}

	static void DrawShots()
	{
		foreach (var shot in shots)
		{
			DrawAtCoordinates(new[] { shot[0], shot[1] }, ConsoleColor.Yellow, ShotSymbol);
		}
	}

	static void DrawEnemies()
	{
		foreach (var enemy in enemies)
		{
			DrawAtCoordinates(new[] { enemy[0], enemy[1] }, ConsoleColor.Red, EnemySymbol);
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
		if (frozen) return;

		if (pause % pauseDivider == 0)
		{
			int spawningWidth = generator.Next(0, FieldWidth);
			int spawningHeight = generator.Next(0, MaxHeight / 6);
			enemies.Add(new[] { spawningWidth, spawningHeight });
			pause = 0;
		}
		pause++;
	}
	static ThreadStart Freeze()
	{
		ThreadStart freeze = () =>
		{
			Stopwatch sb = new Stopwatch();
			const int millisecondsOfFreeze = 4000;
			sb.Start();
			while (sb.ElapsedMilliseconds < millisecondsOfFreeze)
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
			while (true)
			{
				var line = file.ReadLine();
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