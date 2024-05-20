using System;

namespace CourseWork_Kaleda
{
    /// <summary>
    /// Класс для представления информации о забронированных билетах.
    /// </summary>
    public class ReservedTicket
    {
        /// <summary>
        /// Получает или задает уникальный идентификатор билета.
        /// </summary>
        public Guid TicketID { get; set; }

        /// <summary>
        /// Получает или задает время отправления.
        /// </summary>
        public DateTime DepartureTime { get; set; }

        /// <summary>
        /// Получает или задает пункт отправления.
        /// </summary>
        public string DeparturePoint { get; set; }

        /// <summary>
        /// Получает или задает пункт назначения.
        /// </summary>
        public string Destination { get; set; }
    }
}
