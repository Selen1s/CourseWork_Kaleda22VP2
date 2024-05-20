using Microsoft.EntityFrameworkCore;
using System;
using System.Windows;

namespace CourseWork_Kaleda
{
    /// <summary>
    /// Контекст базы данных для приложения.
    /// </summary>
    public class ApplicationContext : DbContext
    {
        /// <summary>
        /// Получает или задает набор сущностей <see cref="Flight"/>.
        /// </summary>
        public DbSet<Flight> _flights => Set<Flight>();

        /// <summary>
        /// Конфигурирует параметры контекста базы данных.
        /// </summary>
        /// <param name="optionsBuilder">Построитель опций для контекста.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            try
            {
                // Устанавливаем использование SQLite с указанием источника данных
                optionsBuilder.UseSqlite("Data Source=myApp.db");
            }
            catch (Exception ex)
            {
                // В случае ошибки показываем сообщение с исключением и трассировкой стека
                MessageBox.Show($"Ошибка конфигурации БД: {ex.Message}\nТрассировка стека: {ex.StackTrace}");
                throw;
            }
        }
    }
}
