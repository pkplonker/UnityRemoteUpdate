namespace RemoteUpdateEditor
{
	public interface IEditorChangeHandler
	{
		public string Path { get; }
		void OnMessage(string data);
	}
}