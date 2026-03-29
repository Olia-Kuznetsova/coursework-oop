using System;
using System.Collections.Generic;
using System.IO;
using GeometryDash.Models;
using Newtonsoft.Json;
using static GeometryDash.Models.Obstacle;

namespace GeometryDash.Managers
{
    public static class LevelManager
    {
        private const string LevelsDirectory = "Levels";

        static LevelManager()
        {
            if (!Directory.Exists(LevelsDirectory))
            {
                Directory.CreateDirectory(LevelsDirectory);
            }
        }

        public static void SaveLevel(string levelName, List<Obstacle> obstacles)
        {
            var levelData = JsonConvert.SerializeObject(obstacles);
            File.WriteAllText(Path.Combine(LevelsDirectory, $"{levelName}.json"), levelData);
        }

        public static List<Obstacle> LoadLevel(string levelName)
        {
            var filePath = Path.Combine(LevelsDirectory, $"{levelName}.json");
            if (File.Exists(filePath))
            {
                var levelData = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<List<Obstacle>>(levelData);
            }

            return new List<Obstacle>();
        }

        public static List<Obstacle> GenerateLevel(int levelNumber)
        {
            List<Obstacle> obstacles = new List<Obstacle>();

            switch (levelNumber)
            {
                case 1:
                    obstacles.Add(new Obstacle { X = 400, Y = 200, Width = 30, Height = 30, Speed = 10 });
                    obstacles.Add(new Obstacle { X = 600, Y = 200, Width = 30, Height = 30, Speed = 8 });
                    break;
                case 2:
                    obstacles.Add(new Obstacle { X = 400, Y = 200, Width = 30, Height = 30, Speed = 10 });
                    obstacles.Add(new Obstacle { X = 600, Y = 200, Width = 30, Height = 30, Speed = 8 });
                    obstacles.Add(new Obstacle { X = 700, Y = 150, Width = 50, Height = 50, Speed = 12 });
                    break;
               
                default:
                    throw new NotImplementedException("Level not implemented.");
            }

            return obstacles;
        }

     

        public static Obstacle CreateMovingBlock(double x, double y, double width, double height, double moveDirection, double moveSpeed)
        {
            return new Obstacle { X = x, Y = y, Width = width, Height = height, Type = ObstacleType.MovingBlock, MoveDirection = moveDirection, MoveSpeed = moveSpeed };
        }

    }
}
