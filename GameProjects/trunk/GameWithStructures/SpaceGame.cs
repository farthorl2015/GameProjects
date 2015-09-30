using System;
using System.Collections.Generic;
using System.Threading;

struct Ship //making structure, where each ship has characteristics
{
    public int x;
    public int y;
    public char symbol;
    public ConsoleColor color;
    //we can add more 
}
class SpaceGame
{
    private const int MaxHeight = 40;
    private const int MaxWidth = 50;

    static void Main()
    {
        Console.Title = "Space | Invaders"; //set title bar on the console
        Console.BufferHeight = Console.WindowHeight = MaxHeight;
        Console.BufferWidth = Console.WindowWidth = MaxWidth;

        int playfieldWidth = 20; //setting the field 
        int playfieldHeight = Console.WindowHeight - 1;

        Ship userShip = new Ship();
        userShip.x = playfieldWidth / 2; //the coordinates of the user ship
        userShip.y = playfieldHeight - 1;
        userShip.symbol = 'W'; //ship symbol 
        userShip.color = ConsoleColor.Yellow; //ship color

        List<Ship> ships = new List<Ship>(); //Temporary list of all ships in the game

        int livesCounter = 3;

        double enemySpeed = 400.0;

        while (true)
        {
            enemySpeed += 0.25;

            SpawnEnemyShips(playfieldWidth, playfieldHeight, ships);

            userShip = MoveUserShip(userShip, playfieldWidth, playfieldHeight);

            ships = MoveEnemyShips(ships, userShip, ref livesCounter);

            Console.Clear();

            foreach (Ship enemy in ships)
            {
                PrintOnPosition(enemy.x, enemy.y, enemy.symbol, enemy.color);
            }

            PrintOnPosition(userShip.x, userShip.y, userShip.symbol, userShip.color); //print our ship
            PrintStringOnPosition(25, 8, "Lives: " + livesCounter, ConsoleColor.White);
            PrintStringOnPosition(25, 14, "Enemy speed: " + enemySpeed + "km/s", ConsoleColor.White);
            PrintStringOnPosition(25, 16, "SCORE: todo", ConsoleColor.White);

            Thread.Sleep((int)(600 - enemySpeed));
        }
    }

    private static List<Ship> MoveEnemyShips(List<Ship> ships, Ship userShip, ref int livesCounter)
    {
        List<Ship> newList = new List<Ship>();

        for (int i = 0; i < ships.Count; i++)
        {
            Ship oldShip = ships[i];
            Ship newShip = new Ship();
            newShip.x = oldShip.x;
            newShip.y = oldShip.y + 2;
            newShip.color = oldShip.color;
            newShip.symbol = oldShip.symbol;

            if (newShip.y == userShip.y && newShip.x == userShip.x)
            {
                livesCounter--;

                if (livesCounter <= 0)
                {
                    PrintStringOnPosition(22, 10, "GAME OVER !!!", ConsoleColor.Red);
                    PrintStringOnPosition(22, 12, "Press [enter] to exit!", ConsoleColor.Red);
                    Console.ReadLine();
                    Environment.Exit(0);
                }
            }

            if (newShip.y < Console.WindowHeight)
            {
                newList.Add(newShip);
            }
        }

        ships = newList;
        return ships;
    }

    private static Ship MoveUserShip(Ship userShip, int playfieldWidth, int playfieldHeight)
    {
        if (Console.KeyAvailable) //moving our ship according to the pressed keys
        {
            ConsoleKeyInfo pressedKey = Console.ReadKey(true);

            while (Console.KeyAvailable) //clean read key buffer
            {
                Console.ReadKey(true);
            }

            if (pressedKey.Key == ConsoleKey.LeftArrow)
            {
                if (userShip.x - 1 >= 0)
                {
                    userShip.x = userShip.x - 1;
                }
            }
            else if (pressedKey.Key == ConsoleKey.RightArrow)
            {
                if (userShip.x + 1 < playfieldWidth)
                {
                    userShip.x = userShip.x + 1;
                }
            }
            else if (pressedKey.Key == ConsoleKey.UpArrow)
            {
                if (userShip.y - 1 > 0)
                {
                    userShip.y = userShip.y - 1;
                }
            }
            else if (pressedKey.Key == ConsoleKey.DownArrow)
            {
                if (userShip.y + 1 < playfieldHeight)
                {
                    userShip.y = userShip.y + 1;
                }
            }

            //TODO add shooting
        }
        return userShip;
    }

    private static void SpawnEnemyShips(int playfieldWidth, int playfieldHeight, List<Ship> ships)
    {
        Random randomGenerator = new Random();
        Ship enemyShip = new Ship(); //create enemy ships that we should shoot and avoid
        enemyShip.x = randomGenerator.Next(0, playfieldWidth); //generate random x coordinates
        enemyShip.y = randomGenerator.Next(0, playfieldHeight); //generate random y coordinates
        enemyShip.symbol = '@';
        enemyShip.color = ConsoleColor.Cyan;
        ships.Add(enemyShip);
    }


    private static void PrintOnPosition(int x, int y, char c,
       ConsoleColor color = ConsoleColor.Gray)
    {
        Console.SetCursorPosition(x, y);
        Console.ForegroundColor = color;
        Console.Write(c);
    }
    private static void PrintStringOnPosition(int x, int y, string str,
    ConsoleColor color = ConsoleColor.Gray)
    {
        Console.SetCursorPosition(x, y);
        Console.ForegroundColor = color;
        Console.Write(str);
    }
}
