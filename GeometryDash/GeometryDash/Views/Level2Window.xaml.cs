using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GeometryDash.Views
{
    public partial class GameOptionsWindow : Window
    {
        public bool ShouldNavigateToMainMenu { get; set; } = false;
    }

    public partial class Level2Window : Window
    {
        private DispatcherTimer gameTimer;
        private bool isJumping = false;
        private double gravity = 1;
        private double jumpStrength = -12;
        private double verticalVelocity = 0;
        private double obstacleSpeed = 7;
        private bool gameOver = false;
        private DateTime startTime;
        private bool isFalling = false;
        private bool isOnPositiveObstacle = false;
        private double positiveObstacleSpeedMultiplier = 0.3;
        private Rectangle currentPositiveObstacle = null;
        private MediaPlayer gameMusic;


        public Level2Window()
        {
            InitializeComponent();

            if (BackgroundMusic.Source != null)
            {
                BackgroundMusic.Play();
            }
            else
            {
                MessageBox.Show("Файл музики не знайдено. Перевірте шлях до файлу.");
            }

            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromMilliseconds(15);
            gameTimer.Tick += GameLoop;
            gameTimer.Start();

            this.KeyDown += Level2Window_KeyDown;
            this.Closed += Level2Window_Closed; 

            startTime = DateTime.Now;
            gameMusic = new MediaPlayer();
            gameMusic.Open(new Uri("D:\\Сourse work\\GeometryDash\\GeometryDash\\Resources\\Forever Bound Stereo Madness (Geometry Dash).mp3"));
            gameMusic.Play();

        }

        private void Level2Window_Closed(object sender, EventArgs e)
        {
            if (gameMusic != null)
            {
                gameMusic.Stop();
            }
            if (gameTimer != null)
            {
                gameTimer.Stop();
            }
        }

        private void Level2Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space && !isJumping)
            {
                isJumping = true;
                verticalVelocity = jumpStrength;
            }
            else if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                ShowGameOptions();
            }
        }

        private void GameLoop(object sender, EventArgs e)
        {
            if (gameOver)
            {
                RestartGame();
                return;
            }

            if (isJumping)
            {
                Canvas.SetTop(PlayerSprite, Canvas.GetTop(PlayerSprite) + verticalVelocity);
                verticalVelocity += gravity;

                if (verticalVelocity > 0)
                {
                    isFalling = true;
                }

                if (CheckLandingOnPositiveObstacle())
                {
                    isJumping = false;
                    isFalling = false;
                    verticalVelocity = 0;
                    isOnPositiveObstacle = true;
                }
                else if (Canvas.GetTop(PlayerSprite) >= 200)
                {
                    Canvas.SetTop(PlayerSprite, 200);
                    isJumping = false;
                    isFalling = false;
                    verticalVelocity = 0;
                }
            }

            if (isOnPositiveObstacle)
            {
                double playerLeft = Canvas.GetLeft(PlayerSprite);
                double obstacleLeft = Canvas.GetLeft(currentPositiveObstacle);
                double obstacleRight = obstacleLeft + currentPositiveObstacle.ActualWidth;

                if (playerLeft + PlayerSprite.ActualWidth / 2 > obstacleRight)
                {
                    isOnPositiveObstacle = false;
                    isFalling = true;
                    isJumping = true;
                    currentPositiveObstacle = null;
                }
                else
                {
                    Canvas.SetLeft(PlayerSprite, playerLeft + obstacleSpeed * positiveObstacleSpeedMultiplier);
                }
            }

            MoveObstacle(Obstacle1);
            MoveObstacle(Obstacle2);
            MoveObstacle(Obstacle4);
            MoveObstacle(PositiveObstacle1);
            MoveObstacle(PositiveObstacle2);
            MoveObstacle(PositiveObstacle3);
            MoveObstacle(PositiveObstacle4);
            MoveObstacle(PositiveObstacle5);

            if (CheckCollision(PlayerSprite, Obstacle1) ||
                CheckCollision(PlayerSprite, Obstacle2) ||
                CheckCollision(PlayerSprite, Obstacle4) ||
                CheckCollision(PlayerSprite, PositiveObstacle1) ||
                CheckCollision(PlayerSprite, PositiveObstacle2) ||
                CheckCollision(PlayerSprite, PositiveObstacle3) ||
                CheckCollision(PlayerSprite, PositiveObstacle4) ||
                CheckCollision(PlayerSprite, PositiveObstacle5))
            {
                gameOver = true;
            }


            if (Canvas.GetLeft(PlayerSprite) > Canvas.GetLeft(PositiveObstacle5))
            {
                CompleteLevel();
            }
        }

        private void MoveObstacle(Shape obstacle)
        {
            double left = Canvas.GetLeft(obstacle);
            if (left < -obstacle.ActualWidth)
            {
                left = GameCanvas.ActualWidth;
            }
            else
            {
                left -= obstacleSpeed;
            }
            Canvas.SetLeft(obstacle, left);
        }

        private bool CheckLandingOnPositiveObstacle()
        {
            return CheckLanding(PlayerSprite, PositiveObstacle1) ||
                       CheckLanding(PlayerSprite, PositiveObstacle2) ||
                       CheckLanding(PlayerSprite, PositiveObstacle3) ||
                       CheckLanding(PlayerSprite, PositiveObstacle4) ||
                       CheckLanding(PlayerSprite, PositiveObstacle5);
        }

        private bool CheckLanding(Image player, Rectangle obstacle)
        {
            Rect playerRect = new Rect(Canvas.GetLeft(player), Canvas.GetTop(player), player.Width, player.Height);
            Rect obstacleRect = new Rect(Canvas.GetLeft(obstacle), Canvas.GetTop(obstacle), obstacle.ActualWidth, obstacle.ActualHeight);

            bool isAbovePlatform = playerRect.Bottom <= obstacleRect.Top + 5 &&
                                     playerRect.Right > obstacleRect.Left + 5 &&
                                     playerRect.Left < obstacleRect.Right - 5 &&
                                     verticalVelocity >= 0;

            if (isAbovePlatform && playerRect.Bottom + verticalVelocity >= obstacleRect.Top)
            {
                Canvas.SetTop(player, obstacleRect.Top - player.Height);
                currentPositiveObstacle = obstacle;
                isOnPositiveObstacle = true;
                isJumping = false;
                isFalling = false;
                verticalVelocity = 0;
                return true;
            }

            return false;
        }


        private bool CheckCollision(Image player, Shape obstacle)
        {
            Rect playerRect = new Rect(Canvas.GetLeft(player), Canvas.GetTop(player), player.Width, player.Height);
            Rect obstacleRect = new Rect(Canvas.GetLeft(obstacle), Canvas.GetTop(obstacle), obstacle.ActualWidth, obstacle.ActualHeight);

            if (obstacle.Name != null && obstacle.Name.StartsWith("PositiveObstacle"))
            {
                Rect dangerousObstacleRect = new Rect(
                    obstacleRect.Left,
                    obstacleRect.Top + 1, 
                    obstacleRect.Width,
                    obstacleRect.Height - 1 
                );

                if (playerRect.IntersectsWith(dangerousObstacleRect))
                {
                    return true; 
                }
                return false;
            }

            return playerRect.IntersectsWith(obstacleRect);
        }

        public void RestartGame()
        {
            Canvas.SetLeft(PlayerSprite, 44);
            Canvas.SetTop(PlayerSprite, 200);

            Canvas.SetLeft(Obstacle1, 331);
            Canvas.SetTop(Obstacle1, 200);

            Canvas.SetLeft(Obstacle2, 789);
            Canvas.SetTop(Obstacle2, 200);

            Canvas.SetLeft(Obstacle4, 1338);
            Canvas.SetTop(Obstacle4, 202);

            Canvas.SetLeft(PositiveObstacle1, 550);
            Canvas.SetTop(PositiveObstacle1, 166);

            Canvas.SetLeft(PositiveObstacle2, 1019);
            Canvas.SetTop(PositiveObstacle2, 162);

            Canvas.SetLeft(PositiveObstacle3, 1604);
            Canvas.SetTop(PositiveObstacle3, 162);

            Canvas.SetLeft(PositiveObstacle4, 1722);
            Canvas.SetTop(PositiveObstacle4, 125);

            Canvas.SetLeft(PositiveObstacle5, 1870);
            Canvas.SetTop(PositiveObstacle5, 200);
            isJumping = false;
            verticalVelocity = 0;
            gameOver = false;
            startTime = DateTime.Now;
            isOnPositiveObstacle = false;
            currentPositiveObstacle = null;

            gameMusic.Stop(); 
            gameMusic.Position = TimeSpan.Zero; 
            gameMusic.Play(); 
            gameTimer.Start();
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            ShowGameOptions();
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ShowGameOptions()
        {
            gameTimer.Stop();
            gameMusic.Pause();

            GameOptionsWindow optionsWindow = new GameOptionsWindow();
            optionsWindow.Owner = this;

            optionsWindow.ShowDialog();

            if (optionsWindow.ShouldNavigateToMainMenu) 
            {
                NavigateToMainMenu();
            }
            else if (!gameOver) 
            {
                gameTimer.Start();
                gameMusic.Play();
            }
        }

        public void NavigateToMainMenu()
        {
            gameTimer.Stop();
            gameMusic.Stop();

            try
            {
                MainMenu mainMenu = new MainMenu();
                mainMenu.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка відкриття головного меню: {ex.Message}");
            }

            this.Close();
        }


        private void CompleteLevel()
        {
            gameTimer.Stop();

            LevelCompleteWindow levelCompleteWindow = new LevelCompleteWindow
            {
                CompletedLevel = "Level2" 
            };

            levelCompleteWindow.Show();
            this.Close();
        }

    }
}