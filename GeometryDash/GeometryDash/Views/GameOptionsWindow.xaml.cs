using System.Windows;

namespace GeometryDash.Views
{
    public partial class GameOptionsWindow : Window
    {
        public GameOptionsWindow()
        {
            InitializeComponent();
        }

        private void ReturnToMenu_Click(object sender, RoutedEventArgs e)
        {
            MainMenu mainMenu = new MainMenu();
            mainMenu.Show();
            Application.Current.MainWindow.Close(); 
            this.Close(); 
        }

        private void ContinueGame_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ExitGame_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
