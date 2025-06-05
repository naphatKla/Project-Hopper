using System;
using MoreMountains.Tools;

namespace Characters.Controllers
{
    public class PlayerController : BaseController
    {
        public static PlayerController Instance { get; private set; }

        protected override void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            base.Awake();
        }
    }
}
  
