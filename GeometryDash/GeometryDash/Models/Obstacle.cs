using System.Windows.Shapes;

namespace GeometryDash.Models
{
    public class Obstacle
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Speed { get; set; }
        public ObstacleType Type { get; set; }
      
        public double MoveDirection { get; set; } 
        public double MoveSpeed { get; set; }

        public enum ObstacleType
        {
            Simple,
            MovingBlock,
            Positive 
        }

    }
}
