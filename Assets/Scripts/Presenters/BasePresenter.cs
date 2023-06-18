using GridForColliders;
using UnityEngine;

namespace Presenters
{
    /// <summary>
    /// Presenter in MVP (Passive View) pattern
    /// </summary>
    public abstract class BasePresenter<TModel> : IMovablePresenterInGrid
        where TModel : ObjectInGridWithCollider<ObjectTypeInGrid>
    {
        public readonly TModel Model;
        public readonly GameObject View;

        public Vector2 Pos => Model.Pos;

        protected BasePresenter(TModel model, GameObject view)
        {
            Model = model;
            View = view;
        }

        public void UpdatePos(float deltaTimeSec, MyGridForColliders gridForColliders)
        {
            Model.UpdatePos(deltaTimeSec);
            View.transform.position = Model.Pos;
            gridForColliders.OnObjectAfterMove(Model);
        }

        public virtual void OnDestroy()
        {
        }
    }
}