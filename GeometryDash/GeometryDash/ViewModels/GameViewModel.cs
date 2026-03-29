using GeometryDash.Managers;
using GeometryDash.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;
using static GeometryDash.Models.Obstacle;

public class RelayCommand : ICommand
{
    private readonly Action<object> _execute;
    private readonly Func<object, bool> _canExecute;

    public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }
    public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
    public void Execute(object parameter) => _execute(parameter);
    public event EventHandler CanExecuteChanged;
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

namespace GeometryDash.ViewModels
{
    public class GameViewModel : INotifyPropertyChanged
    {
        private const double PlayerSize = 30;
        private const double GroundY = 200; 
        private const double PositiveObstacleSpeedMultiplier = 0.3;
        private const double CollisionBuffer = 10; 

        private DispatcherTimer gameTimer;
        private double gravity = 1;
        private double jumpPower = 20;
        private double obstacleSpeed = 7;
        private int currentLevelNumber;
        private bool isRunning;
        private Obstacle currentPositiveObstacle = null;

        public Player Player { get; set; }
        public ObservableCollection<Obstacle> Obstacles { get; set; }
        public ICommand JumpCommand { get; }

        public event Action LevelCompleted;
        public event Action PlayerDied; 

        public GameViewModel(int levelNumber)
        {
            currentLevelNumber = levelNumber;
            Player = new Player { X = 50, Y = GroundY, IsJumping = false, VelocityY = 0 };

            LoadLevel();

            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromMilliseconds(15);
            gameTimer.Tick += GameLoop;
            StartGame();

            JumpCommand = new RelayCommand(Jump, (p) => !Player.IsJumping);
        }

        private void LoadLevel()
        {
            if (Obstacles == null)
            {
                Obstacles = new ObservableCollection<Obstacle>();
            }
            Obstacles.Clear();
            foreach (var item in LevelManager.GenerateLevel(currentLevelNumber))
            {
                Obstacles.Add(item);
            }
        }

        private void GameLoop(object sender, EventArgs e)
        {
            if (!isRunning) return;

            UpdatePlayerPosition();

            UpdateObstaclePositions();

            CheckDestructiveCollisions();

            if (Obstacles.Any())
            {
                var lastObstacle = Obstacles.Last();
                if (Player.X > lastObstacle.X + lastObstacle.Width * 2)
                {
                    StopGame();
                    LevelCompleted?.Invoke();
                }
            }
        }

        private void UpdatePlayerPosition()
        {
            if (Player.IsJumping)
            {
                Player.VelocityY += gravity;
                Player.Y += Player.VelocityY;
            }

            Obstacle landingObstacle = CheckTopLandingCollision();

            if (landingObstacle != null)
            {
                Player.Y = landingObstacle.Y - PlayerSize;
                Player.IsJumping = false;
                Player.VelocityY = 0;
                currentPositiveObstacle = landingObstacle;
            }
            else if (Player.Y >= GroundY)
            {
                Player.Y = GroundY;
                Player.IsJumping = false;
                Player.VelocityY = 0;
                currentPositiveObstacle = null;
            }

            if (currentPositiveObstacle != null)
            {
                if (Player.X > currentPositiveObstacle.X + currentPositiveObstacle.Width - PlayerSize / 2)
                {
                    currentPositiveObstacle = null;
                    Player.IsJumping = true; 
                }
                else
                {
                    Player.X -= (obstacleSpeed * PositiveObstacleSpeedMultiplier);
                }
            }
        }

        private void UpdateObstaclePositions()
        {
            foreach (var obstacle in Obstacles)
            {
                obstacle.X -= obstacleSpeed;

                if (obstacle.X < -obstacle.Width)
                {
                    obstacle.X = 1500;
                }
            }
        }

        private void CheckDestructiveCollisions()
        {
            foreach (var obstacle in Obstacles)
            {
                if (obstacle.Type == ObstacleType.Simple || obstacle.Type == ObstacleType.MovingBlock)
                {
                    if (CheckAABBCollision(Player, obstacle))
                    {
                        StopGame();
                        PlayerDied?.Invoke();
                        return;
                    }
                }
                else if (obstacle.Type == ObstacleType.Positive)
                {
                    if (CheckAABBCollision(Player, obstacle) && !CheckTopCollisionOnly(Player, obstacle))
                    {
                        StopGame();
                        PlayerDied?.Invoke();
                        return;
                    }
                }
            }
        }

        private bool CheckAABBCollision(Player player, Obstacle obstacle)
        {
            return player.X < obstacle.X + obstacle.Width &&
                   player.X + PlayerSize > obstacle.X &&
                   player.Y < obstacle.Y + obstacle.Height &&
                   player.Y + PlayerSize > obstacle.Y;
        }

        private bool CheckTopCollisionOnly(Player player, Obstacle obstacle)
        {
            if (currentPositiveObstacle == obstacle) return true;

            if (player.VelocityY < 0) return false;

            if (player.Y + PlayerSize > obstacle.Y &&
                player.Y + PlayerSize < obstacle.Y + CollisionBuffer)
            {
                return true;
            }

            return false;
        }

        private Obstacle CheckTopLandingCollision()
        {
            foreach (var obstacle in Obstacles.Where(o => o.Type == ObstacleType.Positive))
            {
                if (Player.VelocityY >= 0)
                {
                    bool horizontalOverlap = Player.X + PlayerSize > obstacle.X && Player.X < obstacle.X + obstacle.Width;


                    if (horizontalOverlap &&
                        Player.Y + PlayerSize >= obstacle.Y && 
                        Player.Y + PlayerSize <= obstacle.Y + Player.VelocityY + 1)
                    {
                        return obstacle;
                    }
                }
            }
            return null;
        }


        public void StartGame()
        {
            gameTimer.Start();
            isRunning = true;
        }

        public void PauseGame()
        {
            gameTimer.Stop();
            isRunning = false;
        }

        public void ResumeGame()
        {
            gameTimer.Start();
            isRunning = true;
        }

        public void StopGame()
        {
            gameTimer.Stop();
            isRunning = false;
        }

        public void RestartGame()
        {
            StopGame();

            Player.X = 50;
            Player.Y = GroundY;
            Player.IsJumping = false;
            Player.VelocityY = 0;
            currentPositiveObstacle = null;

            LoadLevel();

            StartGame();
        }

        public void Jump(object parameter)
        {
            if (!Player.IsJumping)
            {
                Player.IsJumping = true;
                Player.VelocityY = -jumpPower;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}