using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Timers;

namespace Hamster.Utils.Components
{
	public class Scheduler : Component
	{
		private Dictionary<string, Task> tasks = new Dictionary<string, Task>();
		private Dictionary<string, bool> isStopRequested = new Dictionary<string, bool>();

		private bool isremoved = false;
		public override bool isRemoved { get => isremoved; }

		/// <summary>
		/// Add a schedule task which will be executed every interval of time.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="interval"></param>
		/// <param name="action"></param>
		/// <exception cref="Exception"></exception>
		public void AddTask(string name,TimeSpan interval,Action action)
		{
			if(!tasks.ContainsKey(name))
			{
				isStopRequested.Add(name, false);
				tasks.Add(name, Task.Run(async () =>
				{
					while (!isStopRequested[name])
					{
						await Task.Delay(interval);
						action();
					}
					isStopRequested[name] = false;
				}));
			}
			else
			{
				throw new Exception($"Task \"{name}\" is already Added");
			}
		}
		/// <summary>
		/// Add a task which will be executed at dateTime.It will be removed when dateTime is earlier than now.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="dateTime"></param>
		/// <param name="action"></param>
		/// <exception cref="Exception"></exception>
		public void AddTask(string name, DateTime dateTime, Action action)
		{
			if (!tasks.ContainsKey(name))
			{
				isStopRequested.Add(name, false);
				tasks.Add(name, Task.Run(async () =>
				{
					while (!isStopRequested[name])
					{
						if (dateTime < DateTime.Now)
						{
							RemoveTask(name);
						}
						await Task.Delay(dateTime - DateTime.Now);
						action();
					}
					isStopRequested[name] = false;
				}));
			}
			else
			{
				throw new Exception($"Task \"{name}\" is already Added");
			}
		}
		/// <summary>
		/// Remove a task.
		/// </summary>
		/// <param name="name"></param>
		public async Task RemoveTask(string name)
		{
			if (tasks[name].IsCompleted)
			{
				isStopRequested[name] = true;
				await Task.Run(() => { while (isStopRequested[name]) ; });
			}
			tasks.Remove(name);
			isStopRequested.Remove(name);
		}
		public override void OnAdded() { }

		public async override void OnRemoved()
		{
			if (!isRemoved)
			{
				foreach (string name in tasks.Keys)
				{
					await RemoveTask(name);
				}
				isremoved = true;
			}
		}

		public override void OnUpdate() { }

		public override void OnRequestEnter() { }
	}
}
