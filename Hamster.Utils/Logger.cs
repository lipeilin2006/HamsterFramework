using System.Text;

namespace Hamster.Utils
{
	public static class Logger
	{
		public static LogLevel Level;
		private static FileStream? fileStream;
		public static void SetLogFile(string filename)
		{
			if (File.Exists(filename))
			{
				fileStream = File.OpenWrite(filename);
			}
			else
			{
				fileStream = File.Create(filename);
			}
		}
		public static void LogDebug(string msg)
		{
			if (Level <= LogLevel.Debug)
			{
				Console.ForegroundColor = ConsoleColor.Blue;
				Console.WriteLine("[" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "]" + msg);
				Console.ForegroundColor = ConsoleColor.White;
				WriteToFile(msg);
			}
		}
		public static void LogInfo(string msg)
		{
			if (Level <= LogLevel.Info)
			{
				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine("[" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "]" + msg);
				Console.ForegroundColor = ConsoleColor.White;
				WriteToFile(msg);
			}
		}
		public static void LogError(string msg) 
		{ 
			if (Level <= LogLevel.Error)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("[" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "]" + msg);
				Console.ForegroundColor = ConsoleColor.White;
				WriteToFile(msg);
			}
		}
		private static void WriteToFile(string msg)
		{
			if (fileStream != null)
			{
				Task.Run(async () => { await fileStream.WriteAsync(Encoding.UTF8.GetBytes(msg + Environment.NewLine)); });
			}
		}
		public static void Close()
		{
			fileStream?.Close();
			fileStream = null;
		}
	}
	public enum LogLevel
	{
		Debug,
		Info,
		Error
	}
}
