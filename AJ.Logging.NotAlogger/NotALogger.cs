using AJ.Logging.Interfaces;

namespace AJ.Logging.NotAlogger
{
    public class NotALogger : ILogger
    {
        private const string TITLE_HEADER = "[{0}][{1}][{2}]";

        public void LogInfo(string title, params string[] messages)
        {
           WriteMessage(title, ConsoleColor.Green, LogTypes.INFO.ToString(), messages);
        }

        public void LogWarning(string title, params string[] messages)
        {
            WriteMessage(title, ConsoleColor.Yellow, LogTypes.WARNING.ToString(), messages);
        }

        public void LogError(string title, params string[] messages)
        {
            WriteMessage(title, ConsoleColor.Red, LogTypes.ERROR.ToString(), messages);
        }

        public void LogDebug(string title, params string[] messages)
        {
            WriteMessage(title, ConsoleColor.Magenta, LogTypes.DEBUG.ToString(), messages);
        }

        private void WriteMessage(string title, ConsoleColor color, string headerInfo, params string[] messages)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(string.Format(TITLE_HEADER, headerInfo, title, DateTime.Now.ToString("ddd HH:mm:ss")));
            Console.ForegroundColor = ConsoleColor.White;
            foreach (var message in messages)
            {
                Console.WriteLine(message);
            }
            Console.WriteLine();
        }
    }
}