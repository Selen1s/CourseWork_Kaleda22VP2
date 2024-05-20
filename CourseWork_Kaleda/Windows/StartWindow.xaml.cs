using System;
using System.Windows;

namespace CourseWork_Kaleda.Windows
{
    /// <summary>
    /// Окно начального экрана приложения.
    /// </summary>
    public partial class StartWindow : Window
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="StartWindow"/>.
        /// </summary>
        public StartWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Обработчик события для кнопки "Старт".
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Данные о событии.</param>
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            // Создаем экземпляр главного окна MainWindow
            MainWindow mainWindow = new MainWindow();
            // Открываем главное окно
            mainWindow.Show();
            // Закрываем начальное окно
            this.Close();
        }
    }
}
