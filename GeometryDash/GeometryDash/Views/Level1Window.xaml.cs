using GeometryDash.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GeometryDash.Views
{
    public partial class Level1Window : Window
    {
        // --- Константы (вирішення проблеми Magic Numbers) ---
        private const double TimerIntervalMs = 15;
        private const double Gravity = 1;
        private const double JumpForce = -13.5;
        private const double BaseObstacleSpeed = 5;
        private const double PositiveObstacleSpeedMultiplier = 0.3;
        private const double GroundLevelY = 200;
        private const double PlayerStartX = 44;
        private const double DefaultObstacleSize = 30;
        private const double PlayerHeight = 30;
        private const double SideCollisionTolerance = 5;
        private const double LandingTolerance = 15;

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

        public Level1Window()
        {
            InitializeComponent();
            InitializeAudio();
            InitializeObstacles();
            InitializeTimer();

            this.KeyDown += Level1Window_KeyDown;
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

            // Вирішення проблеми жорстко закодованого шляху
            string musicPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Звук Dry Out.mp3"); gameMusic.Open(new Uri(musicPath, UriKind.Absolute));
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

        private void Level1Window_KeyDown(object sender, KeyEventArgs e)
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
            foreach (var obstacle in deadlyObstacles)
            {
                if (CheckCollision(PlayerSprite, obstacle))
                {
                    gameOver = true;
                }
            }

            foreach (var positiveObstacle in positiveObstacles)
            {
                if (CheckDestructivePositiveCollision(positiveObstacle))
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

        private bool CheckDestructivePositiveCollision(Rectangle obstacle)
        {
            if (CheckCollision(PlayerSprite, obstacle))
            {
                double playerTop = Canvas.GetTop(PlayerSprite);
                double playerBottom = playerTop + PlayerHeight;
                double obstacleTop = Canvas.GetTop(obstacle);

                if (verticalVelocity < 0) return true;

                if (playerBottom > obstacleTop + SideCollisionTolerance) return true;

                if (isOnPositiveObstacle && currentPositiveObstacle == obstacle) return false;
            }
            return false;
        }

        private void MoveObstacle(FrameworkElement obstacle)
        {
            double left = Canvas.GetLeft(obstacle);
            double width = obstacle.Width;

            if (width <= 0)
            {
                if (obstacle.ActualWidth > 0)
                    width = obstacle.ActualWidth;
                else if (obstacle is Polygon poly)
                {
                    double minX = double.MaxValue, maxX = double.MinValue;
                    foreach (var p in poly.Points)
                    {
                        if (p.X < minX) minX = p.X;
                        if (p.X > maxX) maxX = p.X;
                    }
                    width = (maxX - minX);
                }

                if (width <= 0) width = DefaultObstacleSize;
            }

            if (left < -width)
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
            if (CheckCollision(player, obstacle))
            {
                double playerBottom = Canvas.GetTop(player) + player.ActualHeight;
                double obstacleTop = Canvas.GetTop(obstacle);

                if (verticalVelocity >= 0 &&
                    playerBottom >= obstacleTop &&
                    playerBottom <= obstacleTop + LandingTolerance)
                {
                    Canvas.SetTop(player, obstacleTop - player.ActualHeight);

                    currentPositiveObstacle = obstacle;
                    isOnPositiveObstacle = true;
                    return true;
                }
            }
            return false;
        }

        private bool CheckCollision(Image player, Shape obstacle)
        {
            Rect playerRect = new Rect(Canvas.GetLeft(player), Canvas.GetTop(player), player.Width, player.Height);
            Rect obstacleRect;

            if (obstacle is Polygon poly)
            {
                if (poly.Points == null || poly.Points.Count == 0)
                {
                    obstacleRect = new Rect(Canvas.GetLeft(poly), Canvas.GetTop(poly), DefaultObstacleSize, DefaultObstacleSize);
                }
                else
                {
                    double minX = double.MaxValue, minY = double.MaxValue;
                    double maxX = double.MinValue, maxY = double.MinValue;
                    foreach (var p in poly.Points)
                    {
                        if (p.X < minX) minX = p.X;
                        if (p.Y < minY) minY = p.Y;
                        if (p.X > maxX) maxX = p.X;
                        if (p.Y > maxY) maxY = p.Y;
                    }

                    double left = Canvas.GetLeft(poly) + minX;
                    double top = Canvas.GetTop(poly) + minY;
                    double width = Math.Max(1, maxX - minX);
                    double height = Math.Max(1, maxY - minY);

                    obstacleRect = new Rect(left, top, width, height);
                }
            }
            else
            {
                double width = obstacle.Width > 0 ? obstacle.Width : obstacle.ActualWidth;
                double height = obstacle.Height > 0 ? obstacle.Height : obstacle.ActualHeight;
                if (width <= 0) width = DefaultObstacleSize;
                if (height <= 0) height = DefaultObstacleSize;

                obstacleRect = new Rect(Canvas.GetLeft(obstacle), Canvas.GetTop(obstacle), width, height);
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

            ResetElementPosition(Obstacle1, 400, GroundLevelY);
            ResetElementPosition(Obstacle2, 600, GroundLevelY);
            ResetElementPosition(Obstacle4, 1065, 202);

            ResetElementPosition(PositiveObstacle1, 821, 168);
            ResetElementPosition(PositiveObstacle2, 1301, 180);
            ResetElementPosition(PositiveObstacle3, 1453, 128);
            ResetElementPosition(PositiveObstacle4, 1591, 71);
            ResetElementPosition(PositiveObstacle5, 1775, 71);

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

        private void ShowGameOptions()
        {
            GameOptionsWindow optionsWindow = new GameOptionsWindow();
            optionsWindow.Owner = this;
            optionsWindow.ShowDialog();
        }

        private void CompleteLevel()
        {
            gameTimer.Stop();
            gameMusic.Stop();

            LevelCompleteWindow levelCompleteWindow = new LevelCompleteWindow
            {
                CompletedLevel = "Level1"
            };

            levelCompleteWindow.Show();
            this.Close();
        }
    }
}