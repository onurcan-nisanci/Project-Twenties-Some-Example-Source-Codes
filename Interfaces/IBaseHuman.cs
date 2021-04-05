
public interface IBaseHuman
{
    void Movement();
    bool IsFacingRight();
    void GetDamage(float bulletScaleX, WeaponType weaponType, short amountOfDamage, float pushForce = 0, bool canInstantiateBloodEffect = true);
    void InstantiateBloodOnWall(WeaponType weaponType);
    void SetIsDead();
    void DestroyComponents();
    void DestroyAnimator();
}

