using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GeometryDash.Views
{
    public partial class Level3Window : Window
    {
        private const double TimerIntervalMs = 15;
        private const double Gravity = 1;
        private const double JumpForce = -13;
        private const double BaseObstacleSpeed = 10;
        private const double PositiveObstacleSpeedMultiplier = 0.3;
        private const double GroundLevelY = 200;
        private const double PlayerStartX = 44;
        private const double LandingTolerance = 5;

        private DispatcherTimer gameTimer;
        private bool isJumping = false;
        private double verticalVelocity = 0;
        private bool gameOver = false;
        private bool isFalling = false;
        private bool isOnPositiveObstacle = false;
        private Rectangle currentPositiveObstacle = null;
        private MediaPlayer gameMusic;
        private DateTime startTime;

        private List<Shape> deadlyObstacles;
        private List<Rectangle> positiveObstacles;

        public Level3Window()
        {
            InitializeComponent();
            InitializeAudio();
            InitializeObstacles();
            InitializeTimer();

            this.KeyDown += Level3Window_KeyDown;
            startTime = DateTime.Now;
        }

        private void InitializeAudio()
        {
            if (BackgroundMusic.Source != null)
                BackgroundMusic.Play();
            else
                MessageBox.Show("Файл музики не знайдено. Перевірте шлях до файлу.");

            gameMusic = new MediaPlayer();
            try
            {
                string musicPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Geometry_Dash-Back_On_Track-world76.spcs.bio.mp3");
                gameMusic.Open(new Uri(musicPath, UriKind.Absolute));
                gameMusic.Play();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження музики: {ex.Message}");
            }
        }

        private void InitializeObstacles()
        {
            deadlyObstacles = new List<Shape> { Obstacle1, Obstacle2 };
            positiveObstacles = new List<Rectangle> { PositiveObstacle1, PositiveObstacle2, PositiveObstacle3, PositiveObstacle5 };
        }

        private void InitializeTimer()
        {
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromMilliseconds(TimerIntervalMs);
            gameTimer.Tick += GameLoop;
            gameTimer.Start();
        }

        private void Level3Window_KeyDown(object sender, KeyEventArgs e)
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
                    isFalling = true;

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
            foreach (var obstacle in deadlyObstacles)
            {
                if (CheckCollisionWithDanger(PlayerSprite, obstacle))
                {
                    gameOver = true;
                }
            }

            foreach (var positiveObstacle in positiveObstacles)
            {
                if (CheckCollisionWithDanger(PlayerSprite, positiveObstacle))
                {
                    gameOver = true;
                }
            }

            if (Canvas.GetLeft(PlayerSprite) > Canvas.GetLeft(PositiveObstacle5) + PositiveObstacle5.Width)
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
            if (left < -obstacle.Width)
                left = GameCanvas.ActualWidth;
            else
                left -= BaseObstacleSpeed;
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

            bool isAbove = playerRect.Bottom <= obstacleRect.Top + LandingTolerance &&
                           playerRect.Right > obstacleRect.Left + LandingTolerance &&
                           playerRect.Left < obstacleRect.Right - LandingTolerance &&
                           verticalVelocity >= 0;

            if (isAbove && playerRect.Bottom + verticalVelocity >= obstacleRect.Top)
            {
                Canvas.SetTop(player, obstacleRect.Top - player.Height);
                currentPositiveObstacle = obstacle;
                isOnPositiveObstacle = true;
                StopJumping();
                return true;
            }

            return false;
        }

        private bool CheckCollisionWithDanger(Image player, Shape obstacle)
        {
            Rect playerRect = new Rect(Canvas.GetLeft(player), Canvas.GetTop(player), player.Width, player.Height);
            Rect obstacleRect = new Rect(Canvas.GetLeft(obstacle), Canvas.GetTop(obstacle), obstacle.ActualWidth, obstacle.ActualHeight);

            if (!obstacle.Name.StartsWith("PositiveObstacle"))
                return playerRect.IntersectsWith(obstacleRect);

            Rect dangerousZone = new Rect(
                obstacleRect.Left,
                obstacleRect.Top + LandingTolerance,
                obstacleRect.Width,
                obstacleRect.Height - LandingTolerance
            );

            if (playerRect.IntersectsWith(dangerousZone))
                return true;

            return false;
        }

        private void ResetElementPosition(FrameworkElement element, double left, double top)
        {
            Canvas.SetLeft(element, left);
            Canvas.SetTop(element, top);
        }

        public void RestartGame()
        {
            ResetElementPosition(PlayerSprite, PlayerStartX, GroundLevelY);

            ResetElementPosition(Obstacle1, 683, GroundLevelY);
            ResetElementPosition(Obstacle2, 350, GroundLevelY);

            ResetElementPosition(PositiveObstacle1, 1579, 95);
            ResetElementPosition(PositiveObstacle2, 1042, 150);
            ResetElementPosition(PositiveObstacle3, 1281, 95);
            ResetElementPosition(PositiveObstacle5, 1882, 152);

            StopJumping();
            gameOver = false;
            isOnPositiveObstacle = false;
            currentPositiveObstacle = null;
            startTime = DateTime.Now;

            try
            {
                gameMusic.Stop();
                gameMusic.Position = TimeSpan.Zero;
                gameMusic.Play();
            }
            catch { /* Ігнорувати помилки музики */ }

            gameTimer.Start();
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            ShowGameOptions();
        }

        private void ShowGameOptions()
        {
            MessageBox.Show("Game options would be shown here.");
        }

        private void CompleteLevel()
        {
            gameTimer.Stop();
            try
            {
                if (gameMusic != null)
                    gameMusic.Stop();
            }
            catch { /* Ігнорувати помилки музики при зупинці */ }

            LevelCompleteWindow levelCompleteWindow = new LevelCompleteWindow
            {
                CompletedLevel = "Level3"
            };
            levelCompleteWindow.Show();

            this.Close();
        }
    }
}