namespace RemoteUpdateEditor
{
	public interface IEditorController
	{
		void OnMessage(string endpoint, string eData);
	}
}