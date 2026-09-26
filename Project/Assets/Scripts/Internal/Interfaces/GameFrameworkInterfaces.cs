namespace GFWK.Internal.Interfaces
{

    /// <summary>
    /// Handle the UI window that appear after the finish of a match
    /// By default it collect the local player data and show it to the screen
    /// </summary>
    public interface IGFWKResumeScreen
    {
        void CollectData();
        void Show();
    }
}