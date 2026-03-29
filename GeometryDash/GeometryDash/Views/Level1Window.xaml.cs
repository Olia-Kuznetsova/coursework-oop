using GeometryDash.Models;
using System;
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
        private DispatcherTimer gameTimer;
        private bool isJumping = false;
        private double gravity = 1;
        private double jumpStrength = -13.5;
        private double verticalVelocity = 0;
        private double obstacleSpeed = 5;
        private bool gameOver = false;
        private DateTime startTime;
        private bool isFalling = false;
        private bool isOnPositiveObstacle = false;
        private double positiveObstacleSpeedMultiplier = 0.3;
        private Rectangle currentPositiveObstacle = null;
        private MediaPlayer gameMusic;

        public Level1Window()
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

            this.KeyDown += Level1Window_KeyDown;

            startTime = DateTime.Now;
            gameMusic = new MediaPlayer();
            gameMusic.Open(new Uri("D:\\Сourse work\\GeometryDash\\GeometryDash\\Resources\\Звук Dry Out.mp3"));
            gameMusic.Play();
        }

        private void Level1Window_KeyDown(object sender, KeyEventArgs e)
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
                CheckCollision(PlayerSprite, Obstacle4))
            {
                gameOver = true;
            }

            if (CheckDestructivePositiveCollision(PositiveObstacle1) ||
                CheckDestructivePositiveCollision(PositiveObstacle2) ||
                CheckDestructivePositiveCollision(PositiveObstacle3) ||
                CheckDestructivePositiveCollision(PositiveObstacle4) ||
                CheckDestructivePositiveCollision(PositiveObstacle5))
            {
                gameOver = true;
            }

            if (Canvas.GetLeft(PlayerSprite) > Canvas.GetLeft(PositiveObstacle5) + PositiveObstacle5.Width)
            {
                CompleteLevel();
            }
        }

        private bool CheckDestructivePositiveCollision(Rectangle obstacle)
        {
            const double PlayerHeight = 30;
            const double SideCollisionTolerance = 5;

            if (CheckCollision(PlayerSprite, obstacle))
            {
                double playerTop = Canvas.GetTop(PlayerSprite);
                double playerBottom = playerTop + PlayerHeight;
                double obstacleTop = Canvas.GetTop(obstacle);

                if (verticalVelocity < 0)
                {
                    return true;
                }

                if (playerBottom > obstacleTop + SideCollisionTolerance)
                {
                    return true;
                }

                if (isOnPositiveObstacle && currentPositiveObstacle == obstacle)
                {
                    return false;
                }
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

                if (width <= 0) width = 30;
            }

            if (left < -width)
                left = GameCanvas.ActualWidth;
            else
                left -= obstacleSpeed;

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
            const double LandingTolerance = 15; 

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
                    obstacleRect = new Rect(Canvas.GetLeft(poly), Canvas.GetTop(poly), 30, 30);
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
                if (width <= 0) width = 30;
                if (height <= 0) height = 30;

                obstacleRect = new Rect(Canvas.GetLeft(obstacle), Canvas.GetTop(obstacle), width, height);
            }

            return playerRect.IntersectsWith(obstacleRect);
        }

        private bool CheckCollision(Image player, Rectangle obstacle)
        {
            Rect playerRect = new Rect(Canvas.GetLeft(player), Canvas.GetTop(player), player.Width, player.Height);
            Rect obstacleRect = new Rect(Canvas.GetLeft(obstacle), Canvas.GetTop(obstacle), obstacle.Width, obstacle.Height);
            return playerRect.IntersectsWith(obstacleRect);
        }



        public void RestartGame()
        {
            Canvas.SetLeft(PlayerSprite, 44);
            Canvas.SetTop(PlayerSprite, 200);

            Canvas.SetLeft(Obstacle1, 400);
            Canvas.SetTop(Obstacle1, 200);

            Canvas.SetLeft(Obstacle2, 600);
            Canvas.SetTop(Obstacle2, 200);

            Canvas.SetLeft(Obstacle4, 1065);
            Canvas.SetTop(Obstacle4, 202);

            Canvas.SetLeft(PositiveObstacle1, 821);
            Canvas.SetTop(PositiveObstacle1, 168);

            Canvas.SetLeft(PositiveObstacle2, 1301);
            Canvas.SetTop(PositiveObstacle2, 180);

            Canvas.SetLeft(PositiveObstacle3, 1453);
            Canvas.SetTop(PositiveObstacle3, 128);

            Canvas.SetLeft(PositiveObstacle4, 1591);
            Canvas.SetTop(PositiveObstacle4, 71);

            Canvas.SetLeft(PositiveObstacle5, 1775);
            Canvas.SetTop(PositiveObstacle5, 71);

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