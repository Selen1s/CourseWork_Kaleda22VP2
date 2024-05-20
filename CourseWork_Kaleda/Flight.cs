using System;
using System.ComponentModel.DataAnnotations;

namespace CourseWork_Kaleda
{
    /// <summary>
    /// Класс для представления информации о рейсе.
    /// </summary>
    public class Flight
    {
        /// <summary>
        /// Получает или задает уникальный идентификатор рейса.
        /// </summary>
        [Key]
        public int _id { get; set; }

        /// <summary>
        /// Получает или задает время отправления рейса.
        /// </summary>
        public DateTime _departureTime { get; set; }

        /// <summary>
        /// Получает или задает количество свободных мест на рейсе.
        /// </summary>
        public int _freeSeats { get; set; }

        /// <summary>
        /// Получает или задает пункт отправления рейса.
        /// </summary>
        public string? _departurePoint { get; set; }

        /// <summary>
        /// Получает или задает пункт назначения рейса.
        /// </summary>
        public string? _destination { get; set; }
    }
}
