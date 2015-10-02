using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

class SpaceInvaders
{
    // here are all variables that we need to be used more often in both the Main and the additional methods.
    private const int MaxHeight = 30;
    private const int MaxWidth = 70;
    private const int FieldWidth = MaxWidth / 6; // I made the field smaller because otherwise you cannot catch all the enemies.

    private static int PlayerPositionX = FieldWidth / 2;
    private static int PlayerPositionY = MaxHeight - 2;
    static List<int[]> enemies = new List<int[]>(); // the enemies and shots are List because this list holds all the enemies and shots currently on the field so they can drawn. Each enemy and object consists of PositionX and PositionY that is why they are saved in List from int array.
    static List<int[]> shots = new List<int[]>();

    private static char playerSymbol = 'W'; // it looks the most as a spaceship to me
    private static char enemySymbol = '*'; // looks the angriest
    private static char shotSymbol = '|'; // just random shots

    //Level details
    private static int PauseDivider = 16;//changing count of enemies depending on level;
    private static int lives = 3;
    private static int pause; // here I am adjusting the enemies being spawn because there were too many.
    private static int winnedScoresInLevel;//counting points at each level
    private static int scoresToWin = 10;// the count of scores that are needed to go to next level
    private static Random generator = new Random(); // this is the generator for the starting position of the enemies.

    private static bool wonLevel = false;
    //bool values for wining game and level;

    private static int sleepingParameter = 100;
    private static bool frozen = false;


    static void Main()
    {
        // I set the size of the Console it can be changed easily from the constants above
        Console.BufferHeight = Console.WindowHeight = MaxHeight;
        Console.BufferWidth = Console.WindowWidth = MaxWidth;
        PlayingLevel();// using param level as flag to change difficulty;
    }




    static void PlayingLevel()
    {
        int level = 1;
        int numberOfLevels = 3;

        bool frozenUsed = false;
        while (lives > 0)
        {
            // Draw
            SpawnEnemies(frozen);
            DrawField();
            DrawResultTable(level);
            FieldBarrier();

            // Logic
            while (Console.KeyAvailable)
            {
                var keyPressed = Console.ReadKey(true);
                if (keyPressed.Key == ConsoleKey.RightArrow)
                {
                    if (PlayerPositionX < FieldWidth)
                    {
                        PlayerPositionX++;
                    }
                }
                else if (keyPressed.Key == ConsoleKey.LeftArrow)
                {
                    if (PlayerPositionX > 0)
                    {
                        PlayerPositionX--;
                    }
                }
                else if (keyPressed.Key == ConsoleKey.DownArrow)
                {
                    if (PlayerPositionY < MaxHeight - 2)
                    {
                        PlayerPositionY++;
                    }
                }
                else if (keyPressed.Key == ConsoleKey.UpArrow)
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
            UpdatingShotPosition(); // I did the updating of position in this because otherwise if both updates of the position are in one method when the enemy is at a odd Y position and we shoot(our shoot is alway even Y position) they just pass through each other.
            Collision();
            if (!frozen)
            {
                UpdatingEnemyPosition();
                Collision();
            }

            Thread.Sleep(sleepingParameter); // decide how much do you want to slow the game. // 200 was too slow for me


            //Clear. We need to think of an way to clear withour clearing the barrier because right now if we slow the game a little bit more and the barrier will start to flicker.
            Console.Clear();

            DrawField();
            DrawResultTable(level);
            FieldBarrier();
            wonLevel = winnedScoresInLevel >= scoresToWin;
            if (wonLevel)
            {
                level++;
                GoToNextLevel(level, numberOfLevels);

            }
        }
        PrintStringAtCoordinates(MaxWidth / 2, MaxHeight / 2, ConsoleColor.DarkRed, "YOU LOSE!!!");
        while (true)
        {
            var exitButton = Console.ReadKey();
            PrintStringAtCoordinates(MaxWidth / 2, MaxHeight / 2, ConsoleColor.DarkRed, "Press enter to exit!!!");
            if (exitButton.Key == ConsoleKey.Enter)
            {
                
            }
        }   
    }

    

    static void GoToNextLevel(int level, int numberOfLevels)
    {

        if (level > numberOfLevels)
        {

            PrintStringAtCoordinates(MaxHeight/2, FieldWidth/2, ConsoleColor.DarkBlue, "YOU WON!!!");

            while (true)
            {
                Environment.Exit(Environment.ExitCode);
            }

        }

        PrintStringAtCoordinates(20, 7, ConsoleColor.Black, "Press enter to go to next level");//May be more.. beautiful
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

    private static void ConfigurateLevelDetails()
    {
        //Setting all values for the start of the next level;
        enemies.Clear();
        shots.Clear();
        PlayerPositionX = FieldWidth / 2;
        PlayerPositionY = MaxHeight - 2;
        PauseDivider -= 2;
        sleepingParameter -= 10;
        lives++;
    }


    private static void UpdatingShotPosition()
    {
        shots.ForEach(shot => shot[1]--);
    }

    private static void UpdatingEnemyPosition()
    {
        enemies.ForEach(enemy => enemy[1]++);
    }

    private static void Collision()
    {
        List<int> enemiesToRemove = new List<int>();
        List<int> shotsToRemove = new List<int>();
        List<int[]> enemiesLeft = new List<int[]>();
        List<int[]> shotsLeft = new List<int[]>();
        EnemiesVsPlayer(enemiesToRemove);
        EnemiesVsShots(enemiesToRemove, shotsToRemove);
        UpdatingTheEnemies(enemiesLeft, enemiesToRemove); // here we're getting the new list of enemies after the collision
        UpdatingTheShots(shotsLeft, shotsToRemove);
        shots = shotsLeft;
        enemies = enemiesLeft;
    }

    private static void UpdatingTheShots(List<int[]> shotsLeft, List<int> shotsToRemove)
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

    private static void UpdatingTheEnemies(List<int[]> enemiesLeft, List<int> enemiesToRemove)
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

    private static void EnemiesVsShots(List<int> enemiesToRemove, List<int> shotsToRemove)
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

    private static void EnemiesVsPlayer(List<int> enemiesToRemove)
    {
        for (int index = 0; index < enemies.Count; index++)
        {
            if ((enemies[index][0] == PlayerPositionX && enemies[index][1] == PlayerPositionY) || enemies[index][1] >= MaxHeight - 2)
            {
                lives--;
                DrawAtCoordinates(new[] { enemies[index][0], enemies[index][1]}, ConsoleColor.DarkRed, 'X');
                enemiesToRemove.Add(index);
            }
           
        }
    }


    private static void DrawResultTable(int level)
    {
        PrintStringAtCoordinates(20,4, ConsoleColor.Black, "SPACE INVADERS");
        PrintStringAtCoordinates(20, 6, ConsoleColor.Black, string.Format("Lives: {0}", lives));
        PrintStringAtCoordinates(20, 7, ConsoleColor.Black, string.Format("Level: {0}", level));
        PrintStringAtCoordinates(20, 8, ConsoleColor.Black, string.Format("Next level after {0} enemies kills", scoresToWin - winnedScoresInLevel));
       
    }

    private static void FieldBarrier()
    {
        for (int i = 1; i < MaxHeight - 2; i++)
        {
            DrawAtCoordinates(new int[] { FieldWidth + 1, i }, ConsoleColor.Black, '|');
        }
    }

    private static void DrawField()
    {
        DrawEnemies();
        DrawShots();
        DrawPlayer();
    }

    private static void DrawPlayer()
    {
        int[] playerPosition = { PlayerPositionX, PlayerPositionY };
        ConsoleColor playerColor = ConsoleColor.Green; // choose whatever you like; // changed to Green so it's more visible
        DrawAtCoordinates(playerPosition, playerColor, playerSymbol);
    }

    private static void DrawShots()
    {
        foreach (var shot in shots)
        {
            DrawAtCoordinates(new[] { shot[0], shot[1] }, ConsoleColor.DarkBlue, shotSymbol);
        }
    }

    private static void DrawEnemies()
    {
        foreach (var enemy in enemies)
        {
            DrawAtCoordinates(new[] { enemy[0], enemy[1] }, ConsoleColor.Red, enemySymbol);
        }
    }
    private static void DrawAtCoordinates(int[] objectPosition, ConsoleColor objectColor, char objectSymbol)
    {
        Console.SetCursorPosition(objectPosition[0], objectPosition[1]);
        Console.ForegroundColor = objectColor;
        Console.WriteLine(objectSymbol);
        Console.CursorVisible = false;
    }

    private static void PrintStringAtCoordinates(int stringPositionX, int stringPositionY, ConsoleColor stringColor, string message)
    {
        Console.SetCursorPosition(stringPositionX, stringPositionY);
        Console.ForegroundColor = stringColor;
        Console.WriteLine(message);
        Console.CursorVisible = false;
    }
    private static void SpawnEnemies(bool frozen)
    {
        if (!frozen)
        {
            if (pause % PauseDivider == 0)
            {
                int spawningWidth = generator.Next(0, FieldWidth);
                int spawningHeight = generator.Next(0, MaxHeight / 6);
                enemies.Add(new int[] { spawningWidth, spawningHeight });
                pause = 0;
            }
            pause++;
        }

    }
    private static ThreadStart Freeze()
    {
        ThreadStart freeze = () =>
        {
            Stopwatch sb = new Stopwatch();
            int millieSecondsOfFreeze = 4000;
            sb.Start();
            while (sb.ElapsedMilliseconds < millieSecondsOfFreeze)
            {
                frozen = true;
            }
            frozen = false;
        };
        return freeze;
    }
}
