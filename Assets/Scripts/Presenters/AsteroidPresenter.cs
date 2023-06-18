using UnityEngine;

namespace Presenters
{
    public class AsteroidPresenter : BasePresenter<Asteroid>
    {
        public AsteroidPresenter(Asteroid model, GameObject view)
            : base(model, view)
        {
        }
    }
}