using System;
using System.Collections.Generic;
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
        // --- Константи (вирішення проблеми Magic Numbers) ---
        private const double TimerIntervalMs = 15;
        private const double Gravity = 1;
        private const double JumpForce = -12;
        private const double BaseObstacleSpeed = 7;
        private const double PositiveObstacleSpeedMultiplier = 0.3;
        private const double GroundLevelY = 200;
        private const double PlayerStartX = 44;
        private const double LandingTolerance = 5;

        // --- Основні змінні ---
        private DispatcherTimer gameTimer;
        private bool isJumping = false;
        private double verticalVelocity = 0;
        private bool gameOver = false;
        private DateTime startTime;
        private bool isFalling = false;
        private bool isOnPositiveObstacle = false;
        private Rectangle currentPositiveObstacle = null;
        private MediaPlayer gameMusic;

        // --- Колекції (вирішення проблеми Duplicated Code) ---
        private List<Shape> deadlyObstacles;
        private List<Rectangle> positiveObstacles;

        public Level2Window()
        {
            InitializeComponent();
            InitializeAudio();
            InitializeObstacles();
            InitializeTimer();

            this.KeyDown += Level2Window_KeyDown;
            this.Closed += Level2Window_Closed;

            startTime = DateTime.Now;
        }

        private void InitializeAudio()
        {
            if (BackgroundMusic.Source != null)
            {
                BackgroundMusic.Play();
            }
            else
            {
                MessageBox.Show("Файл музики не знайдено. Перевірте шлях до файлу.");
            }

            gameMusic = new MediaPlayer();

            // Вирішення проблеми жорстко закодованого шляху та уникнення конфлікту CS0104
            string musicPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Forever Bound Stereo Madness (Geometry Dash).mp3");
            gameMusic.Open(new Uri(musicPath, UriKind.Absolute));
            gameMusic.Play();
        }

        private void InitializeObstacles()
        {
            deadlyObstacles = new List<Shape> { Obstacle1, Obstacle2, Obstacle4 };
            positiveObstacles = new List<Rectangle> { PositiveObstacle1, PositiveObstacle2, PositiveObstacle3, PositiveObstacle4, PositiveObstacle5 };
        }

        private void InitializeTimer()
        {
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromMilliseconds(TimerIntervalMs);
            gameTimer.Tick += GameLoop;
            gameTimer.Start();
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
                verticalVelocity = JumpForce;
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

            UpdatePlayerPhysics();
            UpdateObstacles();
            CheckGameConditions();
        }

        private void UpdatePlayerPhysics()
        {
            if (isJumping)
            {
                Canvas.SetTop(PlayerSprite, Canvas.GetTop(PlayerSprite) + verticalVelocity);
                verticalVelocity += Gravity;

                if (verticalVelocity > 0)
                {
                    isFalling = true;
                }

                if (CheckLandingOnPositiveObstacle())
                {
                    StopJumping();
                    isOnPositiveObstacle = true;
                }
                else if (Canvas.GetTop(PlayerSprite) >= GroundLevelY)
                {
                    Canvas.SetTop(PlayerSprite, GroundLevelY);
                    StopJumping();
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
                    Canvas.SetLeft(PlayerSprite, playerLeft + BaseObstacleSpeed * PositiveObstacleSpeedMultiplier);
                }
            }
        }

        private void UpdateObstacles()
        {
            foreach (var obstacle in deadlyObstacles)
            {
                MoveObstacle(obstacle);
            }

            foreach (var positiveObstacle in positiveObstacles)
            {
                MoveObstacle(positiveObstacle);
            }
        }

        private void CheckGameConditions()
        {
            // Перевірка зіткнень з усіма перешкодами
            foreach (var obstacle in deadlyObstacles)
            {
                if (CheckCollision(PlayerSprite, obstacle))
                {
                    gameOver = true;
                }
            }

            foreach (var positiveObstacle in positiveObstacles)
            {
                if (CheckCollision(PlayerSprite, positiveObstacle))
                {
                    gameOver = true;
                }
            }

            if (Canvas.GetLeft(PlayerSprite) > Canvas.GetLeft(PositiveObstacle5))
            {
                CompleteLevel();
            }
        }

        private void StopJumping()
        {
            isJumping = false;
            isFalling = false;
            verticalVelocity = 0;
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
                left -= BaseObstacleSpeed;
            }
            Canvas.SetLeft(obstacle, left);
        }

        private bool CheckLandingOnPositiveObstacle()
        {
            foreach (var obstacle in positiveObstacles)
            {
                if (CheckLanding(PlayerSprite, obstacle))
                {
                    return true;
                }
            }
            return false;
        }

        private bool CheckLanding(Image player, Rectangle obstacle)
        {
            Rect playerRect = new Rect(Canvas.GetLeft(player), Canvas.GetTop(player), player.Width, player.Height);
            Rect obstacleRect = new Rect(Canvas.GetLeft(obstacle), Canvas.GetTop(obstacle), obstacle.ActualWidth, obstacle.ActualHeight);

            bool isAbovePlatform = playerRect.Bottom <= obstacleRect.Top + LandingTolerance &&
                                     playerRect.Right > obstacleRect.Left + LandingTolerance &&
                                     playerRect.Left < obstacleRect.Right - LandingTolerance &&
                                     verticalVelocity >= 0;

            if (isAbovePlatform && playerRect.Bottom + verticalVelocity >= obstacleRect.Top)
            {
                Canvas.SetTop(player, obstacleRect.Top - player.Height);
                currentPositiveObstacle = obstacle;
                isOnPositiveObstacle = true;
                StopJumping();
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

        private void ResetElementPosition(FrameworkElement element, double left, double top)
        {
            Canvas.SetLeft(element, left);
            Canvas.SetTop(element, top);
        }

        public void RestartGame()
        {
            ResetElementPosition(PlayerSprite, PlayerStartX, GroundLevelY);

            ResetElementPosition(Obstacle1, 331, GroundLevelY);
            ResetElementPosition(Obstacle2, 789, GroundLevelY);
            ResetElementPosition(Obstacle4, 1338, 202);

            ResetElementPosition(PositiveObstacle1, 550, 166);
            ResetElementPosition(PositiveObstacle2, 1019, 162);
            ResetElementPosition(PositiveObstacle3, 1604, 162);
            ResetElementPosition(PositiveObstacle4, 1722, 125);
            ResetElementPosition(PositiveObstacle5, 1870, 200);

            StopJumping();
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