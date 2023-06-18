using GridForColliders;

public interface IMovablePresenterInGrid
{
    void UpdatePos(float deltaTimeSec, MyGridForColliders gridForColliders);

    void OnDestroy();
}