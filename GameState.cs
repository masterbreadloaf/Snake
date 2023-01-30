using System;
using System.Collections.Generic;

namespace Snake
{
    public class GameState
    {
        public int Rows { get; }
        public int Cols { get; }
        public GridValue[,] Grid { get; }
        public Direction Dir { get; private set; }
        public int Score { get; private set; }
        public int Coin { get; private set; }
        public bool GameOver { get; private set; }

        private readonly LinkedList<Direction> dirChanges = new LinkedList<Direction>(); // buffer for movement
        private readonly LinkedList<Position> snakePositions = new LinkedList<Position>(); // add or delete from both ends of the list
        private readonly Random random = new Random(); // To generate food

        public GameState(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
            Grid = new GridValue[rows, cols];
            Dir = Direction.Right; // stating position of the snake

            AddSnake();
            AddFood();
            AddCoin();
        }

        private void AddSnake() // spawns snake at specified position
        {
            int r = Rows / 2;

            for (int c = 1; c <= 3; c++)
            {
                Grid[r, c] = GridValue.Snake;
                snakePositions.AddFirst(new Position(r, c));
            }
        }
        private IEnumerable<Position> EmptyPositions() // checks for empty positions in the grid and and yields it
        {
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                    if (Grid[r, c] == GridValue.Empty)
                    {
                        yield return new Position(r, c); //yields an empty positions
                    }

        }

        private void AddFood()
        {
            List<Position> empty = new List<Position>(EmptyPositions());

            if (empty.Count == 0)
            {
                return;
            }

            Position pos = empty[random.Next(empty.Count)];
            Grid[pos.Row, pos.Col] = GridValue.Food;
        }
        private void AddCoin() // adds coins for the shop
        {
            List<Position> empty = new List<Position>(EmptyPositions());

            if (empty.Count == 0)
            {
                return;
            }

            Position pos = empty[random.Next(empty.Count)];
            Grid[pos.Row, pos.Col] = GridValue.Coin;
        }
        public Position HeadPosition() // read list begining of the list 
        {
            return snakePositions.First.Value;
        }

        public Position TailPosition() // read list end of the list
        {
            return snakePositions.Last.Value;
        }

        public IEnumerable<Position> SnakePosition() // returns rest of snake values as ienum
        {
            return snakePositions;
        }

        private void AddHead(Position pos) //moves the position of the head while moving
        {
            snakePositions.AddFirst(pos);
            Grid[pos.Row, pos.Col] = GridValue.Snake;
        }
        private void RemoveTail() //removes the last position of the snake while moving
        {
            Position tail = snakePositions.Last.Value;
            Grid[tail.Row, tail.Col] = GridValue.Empty;
            snakePositions.RemoveLast();
        }

        private Direction GetLastDirection()
        {
            if (dirChanges.Count == 0)
            {
                return Dir;
            }
            return dirChanges.Last.Value;
        }

        private bool CanChangeDirection(Direction newDir)
        {
            if (dirChanges.Count == 2)
            {
                return false;
            }

            Direction lastDir = GetLastDirection();
            return newDir != lastDir && newDir != lastDir.Opposite();
        }
        public void ChangeDirection(Direction dir)
        {
            if(CanChangeDirection(dir))
            {
                dirChanges.AddLast(dir);
            }
            
        }

        private bool OutsideGrid(Position pos) //checks if the position is outside the grid
        {
            return pos.Row < 0 || pos.Row >= Rows || pos.Col < 0 || pos.Col >= Cols;
        }

        private GridValue WillHit(Position newHeadPosition) //
        {
            if (OutsideGrid(newHeadPosition)) // checks if the head is outside and returns grid value
            {
                return GridValue.Outside;
            }

            if (newHeadPosition == TailPosition()) // check if the head will collide with the tail
            {
                return GridValue.Empty;
            }


            return Grid[newHeadPosition.Row, newHeadPosition.Col];
        }

        public void Move() // moves the snake by one space
        {
            if(dirChanges.Count > 0)
            {
                Dir = dirChanges.First.Value;
                dirChanges.RemoveFirst();
            }

            Position newHeadPosition = HeadPosition().Translate(Dir); //head position
            GridValue hit = WillHit(newHeadPosition); //checks if head hits  anything

            if (hit == GridValue.Outside || hit == GridValue.Snake) // check if outside grid
            {
                GameOver = true;
            }
            else if (hit == GridValue.Empty) // move empty space moves tail
            {
                RemoveTail();
                AddHead(newHeadPosition);
            }
            else if (hit == GridValue.Food) // hit position of food 
            {
                AddHead(newHeadPosition);
                Score++;
                AddFood();
            }
            else if (hit == GridValue.Coin)
            {
                AddHead(newHeadPosition);
                Score = Score + 2;
                AddCoin();
            }
        }

    }
}
