using FreedLOW.FireAtTargets.Code.Weapon;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.Player
{
    public interface IPlayerConstructor
    {
        Gender PlayerGender { get; set; }
        WeaponBehaviour WeaponBehaviour { get; set; }
        
        void SetupPlayerObject(Gender gender, WeaponBehaviour weapon);
    }
}