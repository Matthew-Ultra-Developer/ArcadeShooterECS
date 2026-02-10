namespace ArcadeShooter.Core
{
    public static class GameLayers
    {
        public const int Player = 6;
        public const int Enemy = 7;
        
        public const uint PlayerMask = 1u << Player;
        public const uint EnemyMask = 1u << Enemy;
    }
}
