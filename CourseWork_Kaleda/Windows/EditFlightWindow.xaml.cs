using System;
using System.Linq;
using System.Windows;

namespace CourseWork_Kaleda.Windows
{
    /// <summary>
    /// Окно для редактирования информации о рейсе.
    /// </summary>
    public partial class EditFlightWindow : Window
    {
        /// <summary>
        /// Рейс, который нужно обновить.
        /// </summary>
        private Flight _flightToUpdate;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="EditFlightWindow"/> с указанным рейсом.
        /// </summary>
        /// <param name="flightToUpdate">Рейс, который нужно обновить.</param>
        public EditFlightWindow(Flight flightToUpdate)
        {
            InitializeComponent();
            _flightToUpdate = flightToUpdate;
            LoadFlightData();
        }

        /// <summary>
        /// Загружает данные рейса в форму для редактирования.
        /// </summary>
        private void LoadFlightData()
        {
            NewDateTime.SelectedDate = _flightToUpdate._departureTime.Date;
            NewTimeValue.Text = _flightToUpdate._departureTime.ToString("HH:mm");
            NewFreeSeats.Text = _flightToUpdate._freeSeats.ToString();
            NewDeparturePoint.Text = _flightToUpdate._departurePoint;
            NewDestination.Text = _flightToUpdate._destination;
        }

        /// <summary>
        /// Получает обновленные данные о рейсе из формы.
        /// </summary>
        /// <returns>Обновленный объект <see cref="Flight"/>.</returns>
        public Flight GetUpdatedFlight()
        {
            DateTime selectedDate = NewDateTime.SelectedDate ?? DateTime.MinValue;
            TimeSpan selectedTime = TimeSpan.Parse(NewTimeValue.Text);
            DateTime selectedDateTime = selectedDate.Date + selectedTime;

            Flight updatedFlight = new Flight
            {
                _id = _flightToUpdate._id,
                _departureTime = selectedDateTime,
                _freeSeats = int.Parse(NewFreeSeats.Text),
                _departurePoint = NewDeparturePoint.Text,
                _destination = NewDestination.Text
            };

            return updatedFlight;
        }

        /// <summary>
        /// Обработчик события для кнопки "ОК".
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Данные о событии.</param>
        private void EditSuccessButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка корректности введенных данных
            if (!ValidateFields())
                return;

            // Получаем обновленные данные о рейсе
            Flight updatedFlight = GetUpdatedFlight();

            // Проверяем, что измененный рейс уникален
            using (ApplicationContext db = new ApplicationContext())
            {
                if (db._flights.Any(f => f._id != updatedFlight._id && // Исключаем текущий редактируемый рейс
                                          f._departurePoint == updatedFlight._departurePoint &&
                                          f._destination == updatedFlight._destination &&
                                          f._departureTime == updatedFlight._departureTime))
                {
                    MessageBox.Show("Рейс с такими данными уже существует! Пожалуйста, введите уникальные данные.");
                    return;
                }
            }

            // Сохраняем изменения в выбранный рейс
            _flightToUpdate._departureTime = updatedFlight._departureTime;
            _flightToUpdate._freeSeats = updatedFlight._freeSeats;
            _flightToUpdate._departurePoint = updatedFlight._departurePoint;
            _flightToUpdate._destination = updatedFlight._destination;

            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Обработчик события для кнопки "Отмена".
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Данные о событии.</param>
        private void EditCancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Проверяет корректность введенных данных.
        /// </summary>
        /// <returns><c>true</c>, если все поля заполнены корректно; в противном случае <c>false</c>.</returns>
        private bool ValidateFields()
        {
            // Проверка, выбрана ли дата
            if (NewDateTime.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату отправления.");
                return false;
            }

            // Проверка, введено ли время в правильном формате
            string timeInput = NewTimeValue.Text;
            if (!timeInput.Contains(":") || !TimeSpan.TryParse(timeInput, out TimeSpan selectedTime))
            {
                MessageBox.Show("Введите время в правильном формате (например, 14:30).");
                return false;
            }

            // Проверка, что время отправления не раньше текущего времени
            DateTime selectedDateTime = NewDateTime.SelectedDate.Value.Date + selectedTime;
            if (selectedDateTime < DateTime.Now)
            {
                MessageBox.Show("Дата и время отправления не могут быть раньше текущего времени.");
                return false;
            }

            // Проверка, что количество свободных мест указано и больше нуля
            if (string.IsNullOrWhiteSpace(NewFreeSeats.Text) || !int.TryParse(NewFreeSeats.Text, out int freeSeats) || freeSeats <= 0)
            {
                MessageBox.Show("Введите корректное количество свободных мест (больше нуля).");
                return false;
            }

            // Проверка, что пункты отправления и назначения не пустые
            if (string.IsNullOrWhiteSpace(NewDeparturePoint.Text) || string.IsNullOrWhiteSpace(NewDestination.Text))
            {
                MessageBox.Show("Города отправления и прибытия не должны быть пустыми.");
                return false;
            }

            return true;
        }
    }
}
