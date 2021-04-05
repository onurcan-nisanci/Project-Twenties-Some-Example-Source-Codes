using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Code.Interfaces
{
    public interface IBaseWeaponController
    {
        void Shooting();
        void Reloading();
        void ReloadingFinished(bool isCancelled = false);
        void DropWeapon();
    }
}
