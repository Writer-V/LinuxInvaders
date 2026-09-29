using System;
using Microsoft.Xna.Framework;

namespace LinuxInvaders.Core.Enemies
{
    public class EnemyHitEventArgs : EventArgs
    {
        public EnemyType Type {get;}
        public Point Location {get;}
        public int Points {get;}
        public bool Killed {get;}
        public EnemyHitEventArgs(EnemyType type, Point location, bool killed, int points = 0)
        {
            Type = type;
            Location = location;
            Points = points;
            Killed = killed;
        }
    }
}