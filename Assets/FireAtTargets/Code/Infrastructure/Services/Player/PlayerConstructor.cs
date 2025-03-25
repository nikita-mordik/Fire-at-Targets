using FreedLOW.FireAtTargets.Code.Weapon;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.Player
{
    public class PlayerConstructor : IPlayerConstructor
    {
        public Gender PlayerGender { get; set; }
        //public WeaponBehaviour WeaponBehaviour { get; set; }

        // public void SetupPlayerObject(Gender gender, WeaponBehaviour weapon)
        // {
        //     PlayerGender = gender;
        //     WeaponBehaviour = weapon;
        // }
    }
}