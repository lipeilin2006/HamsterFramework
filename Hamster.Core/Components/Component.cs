namespace Hamster.Core.Components
{
	public abstract class Component : IDisposable
	{
		public abstract bool isRemoved { get; }
		public abstract void OnAdded();
		public abstract void OnRemoved();
		public abstract void OnUpdate();
		public abstract void OnRequestEnter();
		public void Dispose()
		{
			if (!isRemoved)
			{
				OnRemoved();
			}
		}
	}
}
