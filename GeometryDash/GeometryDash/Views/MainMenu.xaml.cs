using System.Windows;

namespace GeometryDash.Views
{
    public partial class MainMenu : Window
    {
        public MainMenu()
        {
            InitializeComponent();
        }

        private void PlayLevel1Button_Click(object sender, RoutedEventArgs e)
        {
            Level1Window level1Window = new Level1Window();
            level1Window.Show();
            this.Close();
        }

        private void PlayLevel2Button_Click(object sender, RoutedEventArgs e)
        {
            Level2Window level2Window = new Level2Window();
            level2Window.Show();
            this.Close();
        }

        private void PlayLevel3Button_Click(object sender, RoutedEventArgs e)
        {
            Level3Window level3Window = new Level3Window();
            level3Window.Show();
            this.Close();
        }

        

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }



    }
}
