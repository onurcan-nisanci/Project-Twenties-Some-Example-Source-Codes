
public class Player
{
    public float Speed { get; set; }
    public float SlipperyForce { get; set; }

    #region States
    public bool IsDead { get; set; }
    public bool AllowMovement { get; set; }
    public bool IsMoving { get; set; }
    public bool IsItReachedPeakSpeed { get; set; }
    public bool AllowHurting { get; set; }
    public bool IsShooting { get; set; }
    public bool IsReloading { get; set; }
    public bool IsKicking { get; set; }
    public bool IsFlipped { get; set; }
    public bool IsChamberRemoveClipPlayed { get; set; }
    public bool IsChamberLoadClipPlayed { get; set; }
    public bool IsShellsDropped { get; set; }
    public bool IsUmbrellaOpened { get; set; }
    public bool IsUmbrellaCoverActive { get; set; }
    public bool IsCovering { get; set; }
    public bool IsAimingToLeftOnCover { get; set; }
    public bool IsCollidingWithWall { get; set; }
    public bool HasBulletTaken { get; set; }
    public bool HasBeenKicked { get; set; }
    public bool IsOnTheGround { get; set; }
    public bool AllowBreakingArm { get; set; }
    public bool IsBreakingArm { get; set; }
    public bool AllowRolling { get; set; }
    public bool IsRolling { get; set; }
    #endregion
}
