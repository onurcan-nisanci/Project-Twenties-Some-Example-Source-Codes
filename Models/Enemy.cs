
public class Enemy
{
    public short Health { get; set; }

    #region States
    public bool AllowChangeDirection { get; set; }
    public bool IsShooting { get; set; }
    public bool IsShootingWhileWalkingTowards { get; set; }
    public bool AllowShootingWhileMoving { get; set; }
    public bool CanWalk { get; set; }
    public bool IsSearching { get; set; }
    public bool IsReloading { get; set; }
    public short AmountOfFiredBullets { get; set; }
    public bool HasBulletTakenFromFront { get; set; }
    public bool HasBulletTakenFromBack { get; set; }
    public bool HasShotgunBulletTaken { get; set; }
    public bool HasInjuryStarted { get; set; }
    public bool IsInjured { get; set; }
    public bool AllowKicking { get; set; }
    public bool IsKicking { get; set; }
    public bool IsDead { get; set; }
    public bool HasArmBroken { get; set; }
    public bool HasSeenPlayer { get; set; }
    public bool HasSeenPlayerAndFallen { get; set; }
    public bool HasReactionCompleted { get; set; }
    public bool HasFallen { get; set; }
    public bool AllowFlipping { get; set; }
    #endregion
}
