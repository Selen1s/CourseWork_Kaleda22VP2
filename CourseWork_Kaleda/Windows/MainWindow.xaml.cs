using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.IO;
using System.DirectoryServices;
using System.Windows;
using System.Windows.Media;
using System.Globalization;
using CourseWork_Kaleda.Windows;


namespace CourseWork_Kaleda
{
    /// <summary>
    /// Предоставляет методы расширения для поиска элементов в визуальном дереве WPF.
    /// </summary>
    public static class VisualTreeExtensions
    {
        /// <summary>
        /// Находит родительский элемент указанного типа в визуальном дереве.
        /// </summary>
        /// <typeparam name="T">Тип родительского элемента, который нужно найти.</typeparam>
        /// <param name="obj">Объект, от которого начинается поиск.</param>
        /// <returns>Родительский элемент указанного типа или <c>null</c>, если такой элемент не найден.</returns>
        public static T FindVisualParent<T>(this DependencyObject obj) where T : DependencyObject
        {
            // Получаем родительский элемент
            DependencyObject parent = VisualTreeHelper.GetParent(obj);

            // Итерируем вверх по дереву, пока не найдем родительский элемент нужного типа или не достигнем вершины дерева
            while (parent != null && !(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            // Возвращаем родительский элемент указанного типа или null
            return parent as T;
        }
    }

    /// <summary>
    /// Главное окно приложения.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Список забронированных билетов.
        /// </summary>
        List<ReservedTicket> reservedTickets = new List<ReservedTicket>();

        // Переменные для хранения значений полей для поиска
        private string departureCityForSearch = "";
        private string destinationCityForSearch = "";
        private int seatsNeededForSearch = 0;
        private DateTime? selectedDateForSearch = null;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="MainWindow"/>.
        /// </summary>
        public MainWindow()
        {
            try
            {
                InitializeComponent();

                // Устанавливаем текущую культуру на "ru-RU"
                CultureInfo.CurrentCulture = new CultureInfo("ru-RU");

                // Подключаемся к базе данных и выполняем операции с рейсами
                using (ApplicationContext db = new ApplicationContext())
                {
                    if (db.Database.CanConnect())
                    {
                        // Удаляем рейсы с нулевым количеством свободных мест из базы данных
                        var zeroSeatsFlights = db._flights.Where(f => f._freeSeats == 0).ToList();
                        db._flights.RemoveRange(zeroSeatsFlights);

                        // Удаляем рейсы, которые уже должны были улететь (по дате)
                        var flightsToRemove = db._flights.Where(f => f._departureTime < DateTime.Now).ToList();
                        db._flights.RemoveRange(flightsToRemove);

                        db.SaveChanges();

                        // Получаем список всех оставшихся рейсов из базы данных
                        var flights = db._flights.OrderBy(f => f._departureTime).ToList();

                        // Устанавливаем список рейсов в качестве источника данных для таблицы
                        flightDataGrid.ItemsSource = flights;
                    }
                }
            }
            catch (Exception ex)
            {
                // Отображаем сообщение об ошибке в случае исключения
                MessageBox.Show($"Ошибка: {ex.Message}\n\nТрассировка стека: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Обработчик события для кнопки "Создать БД".
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Данные о событии.</param>
        private void CreateDataBaseButton_Click(object sender, RoutedEventArgs e)
        {
            // Создаем БД, если она ещё не создана
            using (ApplicationContext db = new ApplicationContext())
            {
                bool isCreated = db.Database.EnsureCreated();
                if (isCreated)
                {
                    // Создаем тестовые данные, если база данных только что была создана
                    DateTime nowPlusOneYear = DateTime.Now.AddYears(1);

                    Flight tom = new Flight { _departureTime = nowPlusOneYear, _freeSeats = 33, _departurePoint = "Москва", _destination = "Париж" };
                    Flight alice = new Flight { _departureTime = nowPlusOneYear.AddDays(1), _freeSeats = 45, _departurePoint = "Лондон", _destination = "Сингапур" };
                    Flight bob = new Flight { _departureTime = nowPlusOneYear.AddDays(2), _freeSeats = 20, _departurePoint = "Нью-Йорк", _destination = "Токио" };
                    Flight mary = new Flight { _departureTime = nowPlusOneYear.AddDays(3), _freeSeats = 55, _departurePoint = "Пекин", _destination = "Рим" };
                    Flight john = new Flight { _departureTime = nowPlusOneYear.AddDays(4), _freeSeats = 15, _departurePoint = "Берлин", _destination = "Мадрид" };

                    // Добавляем тестовые данные в базу данных
                    db._flights.AddRange(new List<Flight> { tom, alice, bob, mary, john });
                    db.SaveChanges();

                    MessageBox.Show("БД была создана и заполнена тестовыми данными");
                }
                else
                {
                    MessageBox.Show("БД уже существует");
                }

                // Обновляем DataGrid
                flightDataGrid.ItemsSource = db._flights.ToList();
            }
        }


        /// <summary>
        /// Обработчик события для кнопки "Удалить БД".
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Данные о событии.</param>
        private void DeleteDataBaseButton_Click(object sender, RoutedEventArgs e)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                bool isDeleted = db.Database.EnsureDeleted();
                MessageBox.Show(isDeleted ? "БД удалена" : "Никаких БД нет чтобы удалять");

                // Очищаем таблицу
                flightDataGrid.ItemsSource = null;
                secondDataGrid.ItemsSource = null;
                reservedTicketsDataGrid.ItemsSource = null;
            }
        }

        /// <summary>
        /// Обработчик события для правого клика мыши по DataGrid с забронированными билетами.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Данные о событии.</param>
        private void ReservedTicketsDataGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid == null) return;

            var hitTest = VisualTreeHelper.HitTest(dataGrid, e.GetPosition(dataGrid));
            var dataGridRow = hitTest.VisualHit.FindVisualParent<DataGridRow>();

            if (dataGridRow == null) return;

            var selectedReservedTicket = (ReservedTicket)dataGridRow.Item;
            if (selectedReservedTicket == null) return;

            // Подтверждение удаления билета
            var result = MessageBox.Show("Вы уверены, что хотите отменить бронирование данного билета?", "Подтверждение отмены бронирования", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
                return;

            // Удаляем выбранный билет из списка забронированных билетов
            reservedTickets.Remove(selectedReservedTicket);

            // Увеличиваем количество свободных мест на рейсе на один
            using (ApplicationContext db = new ApplicationContext())
            {
                var flightToUpdate = db._flights.FirstOrDefault(f =>
                    f._departureTime == selectedReservedTicket.DepartureTime &&
                    f._departurePoint == selectedReservedTicket.DeparturePoint &&
                    f._destination == selectedReservedTicket.Destination);

                if (flightToUpdate != null)
                {
                    flightToUpdate._freeSeats++;
                    db._flights.Update(flightToUpdate);
                    db.SaveChanges();
                }
            }

            // Обновляем данные в верхних таблицах
            UpdateFlightsTable();
            UpdateReservedTicketsTable();
            UpdateSearchResultsTable();
        }



        /// <summary>
        /// Обработчик события для кнопки "Добавить рейс в БД".
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Данные о событии.</param>
        private void AddFlightButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем подключение к базе данных
            using (ApplicationContext db = new ApplicationContext())
            {
                if (!db.Database.CanConnect())
                {
                    MessageBox.Show("База данных ещё не создана!");
                    return;
                }
            }

            // Создаем новое окно для ввода данных о рейсе
            AddFlightWindow addFlightWindow = new AddFlightWindow();

            // Открываем окно как диалоговое окно (модально)
            bool? result = addFlightWindow.ShowDialog();

            // Проверяем результат диалога
            if (result == true)
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    Flight newFlight = addFlightWindow.GetFlightData();
                    db._flights.Add(newFlight);
                    db.SaveChanges();

                    // Загружаем данные в таблицу и сортируем по дате
                    var flights = db._flights.OrderBy(f => f._departureTime).ToList();
                    flightDataGrid.ItemsSource = flights;
                }
            }
            UpdateSearchResultsTable();
        }

        /// <summary>
        /// Обработчик события для кнопки "Сохранить БД в JSON".
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Данные о событии.</param>
        private void SaveToJSONButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем подключение к базе данных и наличие записей
            using (ApplicationContext db = new ApplicationContext())
            {
                if (!db.Database.CanConnect())
                {
                    MessageBox.Show("БД ещё не была создана, чтобы сохранять");
                    return;
                }

                if (!db._flights.Any())
                {
                    MessageBox.Show("Невозможно сохранить пустую БД");
                    return;
                }

                // Создаем диалоговое окно для выбора пути сохранения файла
                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    FilterIndex = 1,
                    FileName = $"database_{DateTime.Now:yyyyMMddHHmmssfff}.json"
                };

                // Отображаем диалоговое окно и проверяем результат выбора пользователя
                if (saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;

                    // Получаем все записи из базы данных
                    var flights = db._flights.ToList();

                    // Настраиваем параметры сериализации JSON
                    var settings = new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented, // Отступы для улучшенного форматирования
                        ContractResolver = new CamelCasePropertyNamesContractResolver() // Преобразование имён свойств в camelCase
                    };

                    // Сериализуем данные в формат JSON с учетом настроек
                    string json = JsonConvert.SerializeObject(flights, settings);

                    // Записываем JSON-строку в выбранный файл
                    File.WriteAllText(filePath, json);

                    MessageBox.Show($"Данные сохранены в файл {filePath}");
                }
            }
        }



        /// <summary>
        /// Обработчик события для кнопки "Удалить выбранный рейс из БД".
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Данные о событии.</param>
        private void DeleteFlightButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, есть ли выбранный элемент в таблице
            if (flightDataGrid.SelectedItem != null)
            {
                // Получаем выбранный элемент из таблицы
                Flight selectedFlight = (Flight)flightDataGrid.SelectedItem;

                // Удаляем выбранный элемент из базы данных
                using (ApplicationContext db = new ApplicationContext())
                {
                    // Находим выбранный элемент в базе данных
                    Flight flightToRemove = db._flights.FirstOrDefault(f => f._id == selectedFlight._id);

                    // Проверяем, найден ли элемент в базе данных
                    if (flightToRemove != null)
                    {
                        // Удаляем элемент из базы данных
                        db._flights.Remove(flightToRemove);

                        // Удаляем соответствующие забронированные билеты из списка
                        reservedTickets.RemoveAll(rt => rt.DepartureTime == selectedFlight._departureTime &&
                                                         rt.DeparturePoint == selectedFlight._departurePoint &&
                                                         rt.Destination == selectedFlight._destination);

                        db.SaveChanges();
                    }
                }

                // Обновляем источник данных, чтобы таблица отобразила изменения
                flightDataGrid.ItemsSource = null;
                using (ApplicationContext db = new ApplicationContext())
                {
                    flightDataGrid.ItemsSource = db._flights.ToList();
                }
                UpdateSearchResultsTable();
                UpdateReservedTicketsTable(); // Обновляем таблицу забронированных билетов
            }
            else
            {
                // Если ничего не выбрано в таблице, выводим сообщение об ошибке или предупреждение
                MessageBox.Show("Выберите рейс для удаления.");
            }
        }

        /// <summary>
        /// Обработчик события для кнопки "Редактировать выбранный рейс".
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Данные о событии.</param>
        private void EditFlightButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, есть ли выбранный элемент в таблице
            if (flightDataGrid.SelectedItem != null)
            {
                // Получаем выбранный элемент из таблицы
                Flight selectedFlight = (Flight)flightDataGrid.SelectedItem;

                // Создаем копию выбранного рейса
                Flight originalFlight = new Flight
                {
                    _id = selectedFlight._id,
                    _departureTime = selectedFlight._departureTime,
                    _freeSeats = selectedFlight._freeSeats,
                    _departurePoint = selectedFlight._departurePoint,
                    _destination = selectedFlight._destination
                };

                // Создаем новое окно редактирования рейса, передавая в него копию рейса
                EditFlightWindow editFlightWindow = new EditFlightWindow(originalFlight);

                // Открываем окно как диалоговое окно (модально)
                bool? result = editFlightWindow.ShowDialog();

                // Проверяем результат диалога
                if (result == true)
                {
                    // Если пользователь нажал "ОК", то обновляем запись в базе данных
                    using (ApplicationContext db = new ApplicationContext())
                    {
                        // Получаем обновленные данные из окна редактирования
                        Flight updatedFlight = editFlightWindow.GetUpdatedFlight();

                        // Находим выбранный рейс в базе данных
                        Flight flightInDB = db._flights.FirstOrDefault(f => f._id == selectedFlight._id);

                        // Проверяем, найден ли рейс в базе данных
                        if (flightInDB != null)
                        {
                            // Обновляем данные рейса в базе данных
                            flightInDB._departureTime = updatedFlight._departureTime;
                            flightInDB._freeSeats = updatedFlight._freeSeats;
                            flightInDB._departurePoint = updatedFlight._departurePoint;
                            flightInDB._destination = updatedFlight._destination;

                            // Сохраняем изменения в базе данных
                            db.SaveChanges();

                            // Обновляем источник данных таблицы, чтобы отразить изменения
                            flightDataGrid.ItemsSource = null;
                            var flights = db._flights.OrderBy(f => f._departureTime).ToList(); // Сортировка по дате
                            flightDataGrid.ItemsSource = flights;

                            // Обновляем список забронированных билетов
                            foreach (ReservedTicket ticket in reservedTickets)
                            {
                                if (ticket.DepartureTime == selectedFlight._departureTime &&
                                    ticket.DeparturePoint == selectedFlight._departurePoint &&
                                    ticket.Destination == selectedFlight._destination)
                                {
                                    ticket.DepartureTime = updatedFlight._departureTime;
                                    ticket.DeparturePoint = updatedFlight._departurePoint;
                                    ticket.Destination = updatedFlight._destination;
                                }
                            }
                        }
                    }
                }
                UpdateReservedTicketsTable();
                UpdateSearchResultsTable();
            }
            else
            {
                // Если ничего не выбрано в таблице, выводим сообщение об ошибке или предупреждение
                MessageBox.Show("Выберите рейс для редактирования.");
            }
        }

        /// <summary>
        /// Обработчик события для кнопки "Поиск подходящих рейсов".
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Данные о событии.</param>
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, создана ли база данных
            using (ApplicationContext db = new ApplicationContext())
            {
                if (!db.Database.CanConnect())
                {
                    MessageBox.Show("База данных не создана.");
                    return;
                }
            }

            // Получаем данные, введенные пользователем
            string departureCity = departureCityTextBox.Text;
            string destinationCity = destinationCityTextBox.Text;
            int seatsNeeded;

            // Проверяем, что город отправления и город прибытия не пустые
            if (string.IsNullOrWhiteSpace(departureCity) || string.IsNullOrWhiteSpace(destinationCity))
            {
                MessageBox.Show("Введите город отправления и город прибытия.");
                return;
            }

            // Пытаемся преобразовать введенное количество мест в число
            if (!int.TryParse(seatsNedeedTextBox.Text, out seatsNeeded))
            {
                MessageBox.Show("Введите корректное количество мест.");
                return;
            }

            // Проверяем, выбрана ли дата в DatePicker
            if (DatePicker_Search.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату отправления.");
                return;
            }

            // Сохраняем для последующего обновления таблицы
            departureCityForSearch = departureCityTextBox.Text;
            destinationCityForSearch = destinationCityTextBox.Text;
            selectedDateForSearch = DatePicker_Search.SelectedDate;
            seatsNeededForSearch = seatsNeeded;

            // Получаем выбранную дату из DatePicker
            DateTime? selectedDate = DatePicker_Search.SelectedDate;

            using (ApplicationContext db = new ApplicationContext())
            {
                // Поиск подходящих рейсов в базе данных LINQ
                var matchingFlights = db._flights
                    .Where(f => f._departurePoint == departureCity
                             && f._destination == destinationCity
                             && f._freeSeats >= seatsNeeded
                             && (selectedDate == null || f._departureTime.Date == selectedDate.Value.Date))
                    .OrderBy(f => f._departureTime)
                    .ToList();

                // Убираем рейсы с нулевым количеством свободных мест
                matchingFlights = matchingFlights.Where(f => f._freeSeats > 0).ToList();

                // Обновляем таблицу с найденными рейсами
                secondDataGrid.ItemsSource = matchingFlights;

                // Проверяем, найдены ли рейсы
                if (matchingFlights.Count == 0)
                {
                    MessageBox.Show("Подходящих рейсов не найдено.");
                }
                else
                {
                    MessageBox.Show("Ближайший рейс на данный день указан первым");
                }
            }
        }


        /// <summary>
        /// Обработчик события для кнопки "Забронировать выбранный билет".
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Данные о событии.</param>
        private void ReserveButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, выбран ли какой-либо рейс в таблице
            if (secondDataGrid.SelectedItem != null)
            {
                // Получаем выбранный рейс из таблицы
                Flight selectedFlight = (Flight)secondDataGrid.SelectedItem;

                // Проверяем, что пользователь ввел корректное количество билетов для бронирования
                int ticketsToBook;
                if (!int.TryParse(numTickets_TextBox.Text, out ticketsToBook) || ticketsToBook <= 0)
                {
                    MessageBox.Show("Введите корректное количество билетов для бронирования.");
                    return;
                }

                // Проверяем, что есть достаточно свободных мест на рейсе
                if (selectedFlight._freeSeats < ticketsToBook)
                {
                    MessageBox.Show("Недостаточно свободных мест на выбранном рейсе.");
                    return;
                }

                // Проверяем, что количество билетов после бронирования удовлетворяет критериям поиска
                if (selectedFlight._freeSeats - ticketsToBook < seatsNeededForSearch)
                {
                    // Запрашиваем у пользователя подтверждение на бронирование
                    MessageBoxResult result = MessageBox.Show(
                        "Количество оставшихся мест на рейсе будет меньше, чем указано в фильтре поиска. Продолжить бронирование?",
                        "Подтверждение бронирования",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.No)
                    {
                        return;
                    }
                }

                // Добавляем указанное количество билетов в список забронированных билетов
                for (int i = 0; i < ticketsToBook; i++)
                {
                    ReservedTicket reservedTicket = new ReservedTicket
                    {
                        TicketID = Guid.NewGuid(), // Генерируем новый уникальный ID
                        DepartureTime = selectedFlight._departureTime,
                        DeparturePoint = selectedFlight._departurePoint,
                        Destination = selectedFlight._destination
                    };

                    // Добавляем забронированный билет в список забронированных билетов
                    reservedTickets.Add(reservedTicket);
                }

                // Уменьшаем количество свободных мест на рейсе в базе данных
                using (ApplicationContext db = new ApplicationContext())
                {
                    selectedFlight._freeSeats -= ticketsToBook;
                    db._flights.Update(selectedFlight);
                    db.SaveChanges();
                }

                // Обновляем данные в таблицах с рейсами и забронированными билетами
                UpdateFlightsTable();
                UpdateReservedTicketsTable();
                UpdateSearchResultsTable();
            }
            else
            {
                // Если ничего не выбрано в таблице, выводим сообщение об ошибке или предупреждение
                MessageBox.Show("Выберите рейс для бронирования.");
            }
        }




        /// <summary>
        /// Обновляет таблицу забронированных билетов.
        /// </summary>
        private void UpdateReservedTicketsTable()
        {
            reservedTicketsDataGrid.ItemsSource = null; // Очистка текущего источника данных
            reservedTicketsDataGrid.ItemsSource = reservedTickets; // Установка обновленного списка забронированных билетов в качестве источника данных для таблицы
        }

        /// <summary>
        /// Обновляет данные в таблице с рейсами.
        /// </summary>
        private void UpdateFlightsTable()
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                var flights = db._flights.OrderBy(f => f._departureTime).ToList();
                flightDataGrid.ItemsSource = flights;
            }
        }


        /// <summary>
        /// Метод для обновления таблицы с найденными рейсами.
        /// </summary>
        private void UpdateSearchResultsTable()
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                // Поиск подходящих рейсов в базе данных LINQ
                var matchingFlights = db._flights
                    .Where(f => f._departurePoint == departureCityForSearch
                             && f._destination == destinationCityForSearch
                             && f._freeSeats >= seatsNeededForSearch
                             && (selectedDateForSearch == null || f._departureTime.Date == selectedDateForSearch.Value.Date))
                    .OrderBy(f => f._departureTime)
                    .ToList();

                // Обновляем источник данных для таблицы с найденными рейсами
                secondDataGrid.ItemsSource = matchingFlights;
            }
        }

        /// <summary>
        /// Обработчик события для кнопки "Выход".
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Данные о событии.</param>
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}