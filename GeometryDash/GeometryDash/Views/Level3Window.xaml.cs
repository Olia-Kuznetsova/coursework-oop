using System;
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
        private DispatcherTimer gameTimer;
        private bool isJumping = false;
        private double gravity = 1;
        private double jumpStrength = -13;
        private double verticalVelocity = 0;
        private double obstacleSpeed = 10;
        private bool gameOver = false;
        private bool isFalling = false;
        private bool isOnPositiveObstacle = false;
        private double positiveObstacleSpeedMultiplier = 0.3;
        private Rectangle currentPositiveObstacle = null;
        private MediaPlayer gameMusic;
        private DateTime startTime;

        public Level3Window()
        {
            InitializeComponent();

            if (BackgroundMusic.Source != null)
                BackgroundMusic.Play();
            else
                MessageBox.Show("Файл музики не знайдено. Перевірте шлях до файлу.");

            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromMilliseconds(15);
            gameTimer.Tick += GameLoop;
            gameTimer.Start();

            this.KeyDown += Level3Window_KeyDown;

            startTime = DateTime.Now;
            gameMusic = new MediaPlayer();
            try
            {
                gameMusic.Open(new Uri("D:\\Сourse work\\GeometryDash\\GeometryDash\\Resources\\Geometry_Dash-Back_On_Track-world76.spcs.bio.mp3"));
                gameMusic.Play();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження музики: {ex.Message}");
            }
        }

        private void Level3Window_KeyDown(object sender, KeyEventArgs e)
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
                    isFalling = true;

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
            MoveObstacle(PositiveObstacle1);
            MoveObstacle(PositiveObstacle2);
            MoveObstacle(PositiveObstacle3);
            MoveObstacle(PositiveObstacle5);

            if (CheckCollisionWithDanger(PlayerSprite, Obstacle1) ||
                CheckCollisionWithDanger(PlayerSprite, Obstacle2) ||
                CheckCollisionWithDanger(PlayerSprite, PositiveObstacle1) ||
                CheckCollisionWithDanger(PlayerSprite, PositiveObstacle2) ||
                CheckCollisionWithDanger(PlayerSprite, PositiveObstacle3) ||
                CheckCollisionWithDanger(PlayerSprite, PositiveObstacle5))
            {
                gameOver = true;
            }

            if (Canvas.GetLeft(PlayerSprite) > Canvas.GetLeft(PositiveObstacle5) + PositiveObstacle5.Width)
            {
                CompleteLevel();
            }
        }

        private void MoveObstacle(Shape obstacle)
        {
            double left = Canvas.GetLeft(obstacle);
            if (left < -obstacle.Width)
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
                       CheckLanding(PlayerSprite, PositiveObstacle5);
        }

        private bool CheckLanding(Image player, Rectangle obstacle)
        {
            Rect playerRect = new Rect(Canvas.GetLeft(player), Canvas.GetTop(player), player.Width, player.Height);
            Rect obstacleRect = new Rect(Canvas.GetLeft(obstacle), Canvas.GetTop(obstacle), obstacle.ActualWidth, obstacle.ActualHeight);

            bool isAbove = playerRect.Bottom <= obstacleRect.Top + 5 &&
                           playerRect.Right > obstacleRect.Left + 5 &&
                           playerRect.Left < obstacleRect.Right - 5 &&
                           verticalVelocity >= 0;

            if (isAbove && playerRect.Bottom + verticalVelocity >= obstacleRect.Top)
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

        private bool CheckCollisionWithDanger(Image player, Shape obstacle)
        {
            Rect playerRect = new Rect(Canvas.GetLeft(player), Canvas.GetTop(player), player.Width, player.Height);
            Rect obstacleRect = new Rect(Canvas.GetLeft(obstacle), Canvas.GetTop(obstacle), obstacle.ActualWidth, obstacle.ActualHeight);

            if (!obstacle.Name.StartsWith("PositiveObstacle"))
                return playerRect.IntersectsWith(obstacleRect);

            Rect dangerousZone = new Rect(
                obstacleRect.Left,
                obstacleRect.Top + 5, 
                obstacleRect.Width,
                obstacleRect.Height - 5
            );

            if (playerRect.IntersectsWith(dangerousZone))
                return true; 

            return false; 
        }

        public void RestartGame()
        {
            Canvas.SetLeft(PlayerSprite, 44);
            Canvas.SetTop(PlayerSprite, 200);

            Canvas.SetLeft(Obstacle1, 683);
            Canvas.SetTop(Obstacle1, 200);

            Canvas.SetLeft(Obstacle2, 350);
            Canvas.SetTop(Obstacle2, 200);

            Canvas.SetLeft(PositiveObstacle1, 1579);
            Canvas.SetTop(PositiveObstacle1, 95);

            Canvas.SetLeft(PositiveObstacle2, 1042);
            Canvas.SetTop(PositiveObstacle2, 150);

            Canvas.SetLeft(PositiveObstacle3, 1281);
            Canvas.SetTop(PositiveObstacle3, 95);

            Canvas.SetLeft(PositiveObstacle5, 1882);
            Canvas.SetTop(PositiveObstacle5, 152);

            isJumping = false;
            verticalVelocity = 0;
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