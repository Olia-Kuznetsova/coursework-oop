    using System;
    using System.Windows;

namespace GeometryDash.Views
{
    public partial class LevelCompleteWindow : Window
    {
        public string CompletedLevel { get; set; }

        public LevelCompleteWindow()
        {
            InitializeComponent();
        }

        private void ExitGame_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            Application.Current.Shutdown();
        }

        private void MainMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MainMenu mainMenu = new MainMenu();
                mainMenu.Show();

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка відкриття головного меню: {ex.Message}");
            }
        }

        private void RetryLevel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CompletedLevel == "Level1")
                    new Level1Window().Show();
                else if (CompletedLevel == "Level2")
                    new Level2Window().Show();
                else if (CompletedLevel == "Level3")
                    new Level3Window().Show();

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка перезапуску рівня: {ex.Message}");
            }
        }

        private void NextLevel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CompletedLevel == "Level1")
                    new Level2Window().Show();
                else if (CompletedLevel == "Level2")
                    new Level3Window().Show();
                else if (CompletedLevel == "Level3")
                    MessageBox.Show("Congratulations! You have passed all levels 🎉");

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка переходу на наступний рівень: {ex.Message}");
            }
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
    }
}
