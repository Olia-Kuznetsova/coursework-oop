using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GeometryDash.Models
{
    public class Player : INotifyPropertyChanged
    {
        private double x;
        private double y;
        private bool isJumping;
        private double velocityY;

        public double X
        {
            get => x;
            set { x = value; OnPropertyChanged(); }
        }
        public double Y
        {
            get => y;
            set { y = value; OnPropertyChanged(); }
        }
        public bool IsJumping
        {
            get => isJumping;
            set { isJumping = value; OnPropertyChanged(); }
        }
        public double VelocityY
        {
            get => velocityY;
            set { velocityY = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}