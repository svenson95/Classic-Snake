using Classic_Snake.classes;
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
    private readonly Dictionary<GridValue, ImageSource> gridValueToImage = new()
    {
        { GridValue.EMPTY, Images.Empty },
        { GridValue.SNAKE, Images.Body },
        { GridValue.FOOD, Images.Food }
    };

    private readonly Dictionary<GridDirection, int> directionToRotation = new()
    {
        { GridDirection.Up, 0 },
        { GridDirection.Right, 90 },
        { GridDirection.Down, 180 },
        { GridDirection.Left, 270 },
    };

    private readonly int rows = 15, cols = 15;
    private readonly Image[,] gridImages;
    private GameState gameState;
    private bool isGameRunning;

    public MainWindow()
    {
        InitializeComponent();
        gridImages = SetupGrid();
        gameState = new GameState(rows, cols);
    }

    private async Task RunGame()
    {
        Draw();
        await ShowGameStart();
        Overlay.Visibility = Visibility.Hidden;
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
                TogglePause();
                break;
            case Key.Left:
                gameState.ChangeDirection(GridDirection.Left);
                break;
            case Key.Right:
                gameState.ChangeDirection(GridDirection.Right);
                break;
            case Key.Up:
                gameState.ChangeDirection(GridDirection.Up);
                break;
            case Key.Down:
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

        if (!isGameRunning)
        {
            isGameRunning = true;
            await RunGame();
            isGameRunning = false;
        }

        if (gameState.IsPaused)
        {
            TogglePause();
        }
    }

    private async Task GameLoop()
    { 
        while (!gameState.IsGameOver)
        {
            await Task.Delay(200);
            if (!gameState.IsPaused)
            {
                gameState.Move();
                Draw();
            }
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
        ScoreText.Text = $"SCORE {gameState.Score}";
    }

    private void DrawGrid()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                GridValue gridValue = gameState.Grid[r, c];
                gridImages[r, c].Source = gridValueToImage[gridValue];
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

        int rotation = directionToRotation[gameState.Dir];
        image.RenderTransform = new RotateTransform(rotation);
    }

    private async Task ShowGameStart()
    {
        for (int i = 3; i >= 1; i--)
        {
            OverlayText.Text = i.ToString();
            await Task.Delay(1000);
        }
    }

    private void TogglePause()
    {
        if (gameState.IsPaused)
        {
            gameState.IsPaused = false;
            Overlay.Visibility = Visibility.Hidden;
        }
        else
        {
            gameState.IsPaused = true;
            Overlay.Visibility = Visibility.Visible;
            OverlayText.Text = "Press any key to continue.";
        }
    }

    private async Task ShowGameOver()
    {
        await DrawDeadSnake();
        await Task.Delay(1000);
        Overlay.Visibility = Visibility.Visible;
        OverlayText.Text = "Game Over. Press any key to start again.";
    }
}