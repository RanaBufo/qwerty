
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder();

builder.Logging.AddMyCustomFileLogger(Path.Combine(Directory.GetCurrentDirectory(), "logger.txt"));

var app = builder.Build();


app.Run(async (context) =>
{
    app.Logger.LogInformation($"Path: {context.Request.Path}  Time:{DateTime.Now.ToLongTimeString()}");
    await context.Response.WriteAsync("Hello World!");
});
// Логирование приложения используя провайдер Консоли
/*
var consoleLoggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.AddFilter("Microsoft", LogLevel.Warning); // Установим для категории "Microsoft" уровень логирования на Warning
});
ILogger consoleLogger = consoleLoggerFactory.CreateLogger<Program>();

app.Run(async (context) =>
{
    var path = context.Request.Path;
    consoleLogger.LogInformation($"LogInformation {path}");
    consoleLogger.LogCritical($"LogCritical {path}");
    consoleLogger.LogError($"LogError {path}");
    consoleLogger.LogWarning($"LogWarning {path}");

    await context.Response.WriteAsync("CAAAAAAAAAAAAAATS!");
});
*/

// Логирование приложения используя провайдер Windows Event Log
/*
var eventLogLoggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddEventLog();
    builder.AddFilter("Microsoft", LogLevel.Information); // Установим для категории "Microsoft" уровень логирования на Error
});
ILogger eventLogLogger = eventLogLoggerFactory.CreateLogger<Program>();

app.Run(async (context) =>
{
    eventLogLogger.LogInformation($"Requested Path: {context.Request.Path}");
    await context.Response.WriteAsync("Kitty");
});
*/

// Логирование приложения используя провайдер Debug
/*
var debugLoggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddDebug();
    builder.AddFilter("Microsoft", LogLevel.Debug); // Установим для категории "Microsoft" уровень логирования на Debug
});
ILogger debugLogger = debugLoggerFactory.CreateLogger<Program>();

app.Run(async (context) =>
{
    debugLogger.LogInformation($"Requested Path: {context.Request.Path}");
    await context.Response.WriteAsync("Step");
});
*/

app.Run();


// Класс для логирования в файл
public class MyCustomFileLogger : ILogger, IDisposable
{
    private readonly string _filePath; // Путь к файлу логов
    private static readonly object _lock = new object(); // Объект блокировки для многопоточной безопасности

    // Конструктор, принимающий путь к файлу логов
    public MyCustomFileLogger(string path)
    {
        _filePath = path;
    }

    // Метод для начала нового логического блока
    public IDisposable BeginScope<TState>(TState state)
    {
        return this;
    }

    // Метод для очистки ресурсов 
    public void Dispose() { }

    // Метод для проверки, включено ли логирование для указанного уровня
    public bool IsEnabled(LogLevel logLevel)
    {
        // Здесь можно добавить вашу проверку для включения/выключения логирования на основе уровня
        // В данной реализации всегда включено
        return true;
    }

    // Метод для записи сообщения логирования в файл
    public void Log<TState>(LogLevel logLevel, EventId eventId,
                TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        // Проверка вычислений перед логированием
        if (!IsValidCalculation(state))
            return;

        // Блокируем доступ к файлу для многопоточной безопасности
        lock (_lock)
        {
            // Добавляем текст сообщения логирования в конец файла
            File.AppendAllText(_filePath, formatter(state, exception) + Environment.NewLine);
        }
    }

    // Метод для проверки вычислений перед логированием
    private bool IsValidCalculation<TState>(TState state)
    {
        // Здесь можно добавить вашу логику проверки вычислений перед логированием
        // В данной реализации всегда считаем вычисления допустимыми
        return true;
    }
}

// Провайдер логгера для записи логов в файл
public class MyCustomFileLoggerProvider : ILoggerProvider
{
    private readonly string _path; // Путь к файлу логов

    // Конструктор, принимающий путь к файлу логов
    public MyCustomFileLoggerProvider(string path)
    {
        _path = path;
    }

    // Метод для создания экземпляра логгера
    public ILogger CreateLogger(string categoryName)
    {
        return new MyCustomFileLogger(_path); // Создаем новый экземпляр логгера с указанным путем к файлу
    }

    // Метод для очистки ресурсов 
    public void Dispose() { }
}

// Класс-расширение для добавления провайдера логгера в конвейер логирования
public static class MyCustomFileLoggerExtensions
{
    // Метод для добавления провайдера логгера в конвейер логирования
    public static ILoggingBuilder AddMyCustomFileLogger(this ILoggingBuilder builder, string filePath)
    {
        builder.AddProvider(new MyCustomFileLoggerProvider(filePath)); // Добавляем провайдер логгера в конвейер
        return builder;
    }
}