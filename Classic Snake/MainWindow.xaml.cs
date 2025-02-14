using Classic_Snake.classes;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Classic_Snake;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly int rows = 15, cols = 15;
    private readonly Image[,] gridImages;
    private GameState gameState;
    private bool isGameStarted;

    public MainWindow()
    {
        InitializeComponent();
        gridImages = SetupGrid();
        gameState = new GameState(rows, cols);
    }

    private async Task StartGame()
    {
        Draw();
        await ShowGameStart();
        Overlay.Visibility = Visibility.Hidden;
        gameState.StartTimer();

        await GameLoop();
        await ShowGameOver();
        gameState = new GameState(rows, cols);
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (gameState.IsGameOver)
        {
            return;
        }

        switch (e.Key)
        {
            case Key.Escape:
            case Key.Space:
                PauseGame();
                break;
            case Key.Left:
            case Key.A:
                gameState.ChangeDirection(GridDirection.Left);
                break;
            case Key.Right:
            case Key.D:
                gameState.ChangeDirection(GridDirection.Right);
                break;
            case Key.Up:
            case Key.W:
                gameState.ChangeDirection(GridDirection.Up);
                break;
            case Key.Down:
            case Key.S:
                gameState.ChangeDirection(GridDirection.Down);
                break;
        }
    }

    private async void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (Overlay.Visibility == Visibility.Visible)
        {
            e.Handled = true;
        }

        if (!isGameStarted)
        {
            isGameStarted = true;
            await StartGame();
            isGameStarted = false;
        }

        if (gameState.IsPaused)
        {
            gameState.IsPaused = false;
            gameState.Timer.Enabled = true;
            Overlay.Visibility = Visibility.Hidden;
        }
    }

    private async Task GameLoop()
    {
        while (!gameState.IsGameOver)
        {
            await Task.Delay(gameState.MoveDelay);
            if (!gameState.IsPaused)
            {
                gameState.Move();
                Draw();
                IncreaseSpeed();
            }
        }
    }

    private void IncreaseSpeed()
    {
        if (gameState.Score >= 5 && gameState.MoveDelay == 200)
        {
            gameState.MoveDelay -= 50;
        }

        if (gameState.Score >= 10 && gameState.MoveDelay == 150)
        {
            gameState.MoveDelay -= 50;
        }
    }

    private Image[,] SetupGrid()
    {
        Image[,] images = new Image[rows, cols];
        GameGrid.Rows = rows;
        GameGrid.Columns = cols;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Image image = new Image
                {
                    Source = Images.Empty,
                    RenderTransformOrigin = new Point(0.5, 0.5)
                };

                images[r, c] = image;
                GameGrid.Children.Add(image);
            }
        }

        return images;
    }

    private void Draw()
    {
        DrawGrid();
        DrawSnakeHead();
        ScoreText.Text = $"Score {gameState.Score}";
        SpeedText.Text = GetSpeed();
        TimerText.Text = GetTime();
    }

    private void DrawGrid()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                GridValue gridValue = gameState.Grid[r, c];
                Dictionary<GridValue, ImageSource> imageSource = new()
                {
                    { GridValue.EMPTY, Images.Empty },
                    { GridValue.SNAKE, Images.Body },
                    { GridValue.FOOD, Images.Food }
                };
                gridImages[r, c].Source = imageSource[gridValue];
                gridImages[r, c].RenderTransform = Transform.Identity;
            }
        }
    }

    private async Task DrawDeadSnake()
    {
        List<GridPosition> positions = new List<GridPosition>(gameState.SnakePositions());

        for (int i = 0; i < positions.Count; i++)
        {
            GridPosition position = positions[i];
            ImageSource source = (i == 0) ? Images.DeadHead : Images.DeadBody;
            gridImages[position.Row, position.Col].Source = source;
            await Task.Delay(50);
        }
    }

    private void DrawSnakeHead()
    {
        GridPosition headPosition = gameState.HeadPosition();
        Image image = gridImages[headPosition.Row, headPosition.Col];
        image.Source = Images.Head;

        Dictionary<GridDirection, int> rotationValue = new()
        {
            { GridDirection.Up, 0 },
            { GridDirection.Right, 90 },
            { GridDirection.Down, 180 },
            { GridDirection.Left, 270 },
        };
        int rotation = rotationValue[gameState.CurrentDirection];
        image.RenderTransform = new RotateTransform(rotation);
    }

    private string GetTime()
    {
        string TimeInString = "";
        int min = gameState.ElapsedTime / 60;
        int sec = gameState.ElapsedTime % 60;


        TimeInString = ((min < 10) ? "0" + min.ToString() : min.ToString());
        TimeInString += ":" + ((sec < 10) ? "0" + sec.ToString() : sec.ToString());
        return TimeInString;
    }

    private string GetSpeed()
    {
        string Label = "Speed";
        switch (gameState.MoveDelay)
        {
            case 200:
                return Label + " 50%";
            case 150:
                return Label + " 75%";
            case 100:
                return Label + " 100%";
        }
        return Label;
    }
    private async Task ShowGameStart()
    {
        for (int i = 3; i >= 1; i--)
        {
            OverlayText.Text = i.ToString();
            await Task.Delay(1000);
        }
    }

    private void PauseGame()
    {
        gameState.Timer.Enabled = false;
        gameState.IsPaused = true;
        Overlay.Visibility = Visibility.Visible;
        OverlayText.Text = "Press any key to continue.";
    }

    private async Task ShowGameOver()
    {
        await DrawDeadSnake();
        gameState.Timer.Close();
        gameState.ElapsedTime = 0;
        await Task.Delay(1000);
        Overlay.Visibility = Visibility.Visible;
        OverlayText.Text = "Game Over. Press any key to start again.";
    }
}