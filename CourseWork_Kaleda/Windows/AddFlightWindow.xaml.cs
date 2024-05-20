using System;
using System.Linq;
using System.Windows;

namespace CourseWork_Kaleda.Windows
{
    /// <summary>
    /// Окно для добавления нового рейса.
    /// </summary>
    public partial class AddFlightWindow : Window
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="AddFlightWindow"/>.
        /// </summary>
        public AddFlightWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Получает данные о рейсе из формы.
        /// </summary>
        /// <returns>
        /// Объект <see cref="Flight"/>, содержащий данные о рейсе, 
        /// или <c>null</c>, если данные введены некорректно.
        /// </returns>
        public Flight GetFlightData()
        {
            // Проверяем, выбрана ли дата
            if (AddDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату отправления.");
                return null;
            }

            // Проверяем, чтобы время было введено в правильном формате (чч:мм)
            string timeInput = AddTimeValue.Text;
            if (!timeInput.Contains(":") || !TimeSpan.TryParse(timeInput, out TimeSpan selectedTime))
            {
                MessageBox.Show("Введите время в правильном формате (например, 14:30).");
                return null;
            }

            // Проверяем, чтобы время отправления не было раньше текущего времени
            DateTime selectedDateTime = AddDatePicker.SelectedDate.Value.Date + selectedTime;
            if (selectedDateTime < DateTime.Now)
            {
                MessageBox.Show("Дата и время отправления не могут быть раньше текущего времени.");
                return null;
            }

            // Проверяем, чтобы количество мест было указано и было больше нуля
            if (string.IsNullOrWhiteSpace(AddfreeSeatsValue.Text) || !int.TryParse(AddfreeSeatsValue.Text, out int freeSeats) || freeSeats <= 0)
            {
                MessageBox.Show("Введите корректное количество свободных мест (больше нуля).");
                return null;
            }

            // Проверяем, чтобы города отправления и прибытия не были пустыми
            if (string.IsNullOrWhiteSpace(AddDeparturePointText.Text) || string.IsNullOrWhiteSpace(AddDestinationText.Text))
            {
                MessageBox.Show("Города отправления и прибытия не должны быть пустыми.");
                return null;
            }

            // Создаем новый объект рейса и заполняем его данными из полей ввода
            Flight newFlight = new Flight
            {
                _departureTime = selectedDateTime,
                _freeSeats = freeSeats,
                _departurePoint = AddDeparturePointText.Text,
                _destination = AddDestinationText.Text
            };

            return newFlight;
        }

        /// <summary>
        /// Обработчик события для кнопки "Ок".
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Данные о событии.</param>
        private void SuccessButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем данные о рейсе из формы
            Flight newFlight = GetFlightData();

            // Проверяем, были ли введены данные
            if (newFlight == null)
            {
                // Если данные не введены, просто возвращаемся
                return;
            }

            // Проверяем уникальность рейса
            using (ApplicationContext db = new ApplicationContext())
            {
                // Если рейс с такими данными уже существует, показываем сообщение об ошибке
                if (db._flights.Any(f => f._departurePoint == newFlight._departurePoint &&
                                          f._destination == newFlight._destination &&
                                          f._departureTime == newFlight._departureTime))
                {
                    MessageBox.Show("Рейс с такими данными уже существует! Пожалуйста, введите уникальные данные.");
                    return;
                }
            }

            // Если все проверки прошли успешно, закрываем окно и возвращаем новый рейс
            this.DialogResult = true;
        }

        /// <summary>
        /// Обработчик события для кнопки "Отмена".
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Данные о событии.</param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Закрываем окно без сохранения данных
            DialogResult = false;
        }
    }
}
