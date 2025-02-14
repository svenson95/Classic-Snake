namespace Classic_Snake.classes
{
    public class GameState
    {
        public int Rows { get; }
        public int Cols { get; }

        public GridValue[,] Grid { get; }
        public GridDirection CurrentDirection { get; private set; }
        public int Score { get; private set; } = 3;
        public int Moves { get; private set; } = 0;
        public System.Timers.Timer Timer;
        public int ElapsedTime = 0;
        public bool IsGameOver { get; private set; } = false;
        public bool IsPaused { get; set; } = false;

        private readonly LinkedList<GridDirection> directionChanges = new LinkedList<GridDirection>();
        private readonly LinkedList<GridPosition> snakePositions = new LinkedList<GridPosition>();
        private readonly Random random = new Random();

        public GameState(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
            Grid = new GridValue[rows, cols];
            CurrentDirection = GridDirection.Right;
            AddSnake();
            AddFood();
        }

        private void AddSnake()
        {
            int r = Rows / 2;
            
            for (int c = 1; c <= 3; c++)
            {
                Grid[r, c] = GridValue.SNAKE;
                snakePositions.AddFirst(new GridPosition(r, c));
            }
        }

        private IEnumerable<GridPosition> EmptyPositions()
        {
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    if (Grid[r,c] == GridValue.EMPTY)
                    {
                        yield return new GridPosition(r, c);
                    }
                }
            }
        }

        private void AddFood()
        {
            List<GridPosition> empty = new List<GridPosition>(EmptyPositions());

            if (empty.Count == 0)
            {
                return;
            }

            GridPosition pos = empty[random.Next(empty.Count)];
            Grid[pos.Row, pos.Col] = GridValue.FOOD;
        }

        public GridPosition HeadPosition()
        {
            return snakePositions.First.Value;
        }

        public GridPosition TailPosition()
        {
            return snakePositions.Last.Value;
        }

        public IEnumerable<GridPosition> SnakePositions()
        {
            return snakePositions;
        }

        private void AddHead(GridPosition pos)
        {
            snakePositions.AddFirst(pos);
            Grid[pos.Row, pos.Col] = GridValue.SNAKE;
        }

        private void RemoveTail()
        {
            GridPosition tail = snakePositions.Last.Value;
            Grid[tail.Row, tail.Col] = GridValue.EMPTY;
            snakePositions.RemoveLast();
        }

        private GridDirection GetLastDirection()
        {
            if (directionChanges.Count == 0)
            {
                return CurrentDirection;
            }

            return directionChanges.Last.Value;
        }

        private bool CanChangeDirection(GridDirection destination)
        {
            if (directionChanges.Count == 2)
            {
                return false;
            }

            GridDirection lastDir = GetLastDirection();
            return destination != lastDir && destination != lastDir.Opposite();
        }

        public void ChangeDirection(GridDirection destination)
        {
            if (CanChangeDirection(destination))
            {
                directionChanges.AddLast(destination);
            }
        }

        private bool IsOutsideGrid(GridPosition pos)
        {
            return pos.Row < 0 || pos.Row >= Rows || pos.Col < 0 || pos.Col >= Cols;
        }

        private GridValue MoveResult(GridPosition newHeadPos)
        {
            if (IsOutsideGrid(newHeadPos))
            {
                return GridValue.OUTSIDE;
            }

            if (newHeadPos == TailPosition())
            {
                return GridValue.EMPTY;
            }

            return Grid[newHeadPos.Row, newHeadPos.Col];
        }

        public void Move()
        {
            if (directionChanges.Count > 0)
            {
                CurrentDirection = directionChanges.First.Value;
                directionChanges.RemoveFirst();
            }

            GridPosition newHeadPos = HeadPosition().Translate(CurrentDirection);
            GridValue moveResult = MoveResult(newHeadPos);

            if (moveResult == GridValue.OUTSIDE || moveResult == GridValue.SNAKE)
            {
                IsGameOver = true;
            }
            else if (moveResult == GridValue.EMPTY)
            {
                AddHead(newHeadPos);
                RemoveTail();
            }
            else if (moveResult == GridValue.FOOD)
            {
                AddHead(newHeadPos);
                Score++;
                AddFood();
            }

            if (moveResult == GridValue.EMPTY || moveResult == GridValue.FOOD)
            {
                Moves++;
            }
        }
    }
}
