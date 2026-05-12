public interface IGazeable
{
    void OnGazeEnter();
    void OnGazeStay(float progress);
    void OnGazeExit();
    void OnGazeSelect();
}
