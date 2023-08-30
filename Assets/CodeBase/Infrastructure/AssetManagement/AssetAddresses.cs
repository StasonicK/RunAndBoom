namespace CodeBase.Infrastructure.AssetManagement
{
    public static class AssetAddresses
    {
        public const string UIRoot = "UIRoot";
        public const string Hero = "Hero";
        public const string Spawner = "SpawnPoint";
        public const string Hud = "Hud";

        // Windows
        public const string ShopWindow = "ShopWindow";
        public const string DeathWindow = "DeathWindow";
        public const string SettingsWindow = "SettingsWindow";
        public const string GiftsWindow = "GiftsWindow";
        public const string ResultWindow = "ResultWindow";
        public const string LeaderBoardWindow = "LeaderBoardWindow";
        public const string AuthorizationWindow = "AuthorizationWindow";
        public const string GameEndWindow = "GameEndWindow";

        // Roots for game objects created by pool
        public const string SpawnersRoot = "SpawnersRoot";
        public const string EnemyProjectilesRoot = "EnemyProjectilesRoot";
        public const string HeroProjectilesRoot = "HeroProjectilesRoot";
        public const string ShotVfxsRoot = "ShotVfxsRoot";

        // Projectiles created by pool
        public const string PistolBullet = "PistolBullet";
        public const string Shot = "Shot";
        public const string Bomb = "Bomb";
        public const string Grenade = "Grenade";
        public const string RocketLauncherRocket = "RocketLauncherRocket";
        public const string RpgRocket = "RpgRocket";

        // Muzzle vfxs created by pool
        public const string BulletMuzzleFire = "BulletMuzzleFire";
        public const string ShotMuzzleFire = "GunFireYellow";
        public const string GrenadeMuzzleFire = "GrenadeMuzzleFire";
        public const string RpgMuzzleFire = "RocketMuzzleFire";

        public const string RocketLauncherMuzzleBlue = "RocketMuzzleBlue";

        // public const string BombMuzzle = "NukeMuzzleBlue";
        public const string BombMuzzle = "BombMuzzleBlue";
    }
}