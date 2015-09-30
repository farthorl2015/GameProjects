using System;
using System.Collections.Generic;
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

    static int level = 1;
    static int PauseDivider =16;//changing count of enemies depending on level;
    static int lives = 3;
    static int pause = 0; // here I am adjusting the enemies being spawn because there were too many.
    static int winnedScoresInLevel = 0;//counting points at each level
    static int scoresToWin = 5;// the count of scores that are needed to go to next level
    static Random generator = new Random(); // this is the generator for the starting position of the enemies.

    //bool values for wining game and level;
    static bool wonLevel = false;
    static void Main()
    {
        // I set the size of the Console it can be changed easily from the constants above
        Console.BufferHeight = Console.WindowHeight = MaxHeight;
        Console.BufferWidth = Console.WindowWidth = MaxWidth;

        while (lives > 0)
        {
            // Draw
            DrawField();
            DrawResultTable();
            // I moved the spawning of the enemies outside the keyavailable loop because otherwise not a single enemy is spawn if you don't click a button.
            //  FieldBarrier();
          SpawnEnemies();
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
              if (keyPressed.Key == ConsoleKey.LeftArrow)
              {
                  if (PlayerPositionX > 0)
                  {
                      PlayerPositionX--;
                  }
              }
              if (keyPressed.Key == ConsoleKey.DownArrow)
              {
                  if (PlayerPositionY < MaxHeight - 2)
                  {
                      PlayerPositionY++;
                  }
              }
              if (keyPressed.Key == ConsoleKey.UpArrow)
              {
                  if (PlayerPositionY > 1)
                  {
                      PlayerPositionY--;
                  }
              }
              if (keyPressed.Key == ConsoleKey.Spacebar)
              {
                  shots.Add(new int[] { PlayerPositionX, PlayerPositionY });
              }
          }     
            UpdatingShotPosition(); // I did the updating of position in this because otherwise if both updates of the position are in one method when the enemy is at a odd Y position and we shoot(our shoot is alway even Y position) they just pass through each other.
            Collision();
            UpdatingEnemyPosition();
            Collision();
            CheckScores();
            Thread.Sleep(100); // decide how much do you want to slow the game. // 200 was too slow for me
            wonLevel = CheckScores();
            
            //Clear. We need to think of an way to clear withour clearing the barrier because right now if we slow the game a little bit more and the barrier will start to flicker.
            Console.Clear();
        }
       
    }

    private static bool CheckScores()
    {
        bool wonLevel = false;
        if (winnedScoresInLevel >= scoresToWin)
        {
            level++;
            wonLevel = true;
        }
        return wonLevel;
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
            if (enemies[i][1] > MaxHeight - 2)
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
        int enemyHitPlayer = enemies.FindIndex(enemy => enemy[0] == PlayerPositionX && enemy[1] == PlayerPositionY);
        // if there is no such enemy enemyHit is -1 and so the condition is:
        if (enemyHitPlayer >= 0)
        {
            lives--;
            enemiesToRemove.Add(enemyHitPlayer);
        }
        int EnemyPassingBorder = enemies.FindIndex(enemy => enemy[1]>=MaxHeight-2);
        if (EnemyPassingBorder >= 0)
        {
            lives--;
            enemiesToRemove.Add(EnemyPassingBorder);
        }

    }


    private static void DrawResultTable()
    {

        // TODO all the information we are going to think of.
    }

    private static void FieldBarrier()
    {
        // TODO we need to think of an a way to draw the barrier outside the while and after that we need make a clear method which doesn't remove it
        for (int i = 0; i < MaxHeight; i++)
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
            DrawAtCoordinates(new[] { enemy[0], enemy[1] }, ConsoleColor. Red, enemySymbol);
        }
    }
    private static void DrawAtCoordinates(int[] objectPostion, ConsoleColor objectColor, char objectSymbol)
    {
        Console.SetCursorPosition(objectPostion[0], objectPostion[1]);
        Console.ForegroundColor = objectColor;
        Console.WriteLine(objectSymbol);
        Console.CursorVisible = false;
    }
    private static void SpawnEnemies()
    {       
        if (pause % PauseDivider  == 0)
        {
            int spawningWidth = generator.Next(0, FieldWidth);
            int spawningHeight = generator.Next(0, MaxHeight / 6);
            enemies.Add(new int[] { spawningWidth, spawningHeight });
            pause = 0;
        }
        pause++;
    }
}
  