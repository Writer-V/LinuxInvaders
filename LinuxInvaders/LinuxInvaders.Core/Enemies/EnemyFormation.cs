using System.Linq;
using Microsoft.Xna.Framework;

namespace LinuxInvaders.Core.Enemies
{
    public class EnemyFormation
    {
        public Enemy[,] enemyGrid { get; init; }
        public int EnemiesAlive => enemyGrid.Cast<Enemy>().Count<Enemy>(x => x.Exists);
        
        //For the currently assumed 60px² enemies. Would ofc be set elsewhere in real production
        private int gridCellSize = 70;
        public EnemyFormation (EnemyDefinition[] rowSpread, Vector2 startPos, Vector2 windowSize)
        {
            enemyGrid = new Enemy[rowSpread.Length,6];
            for(int row = 0; row < rowSpread.Length; row++)
            {
                for(int col = 0; col < 6; col++)
                {
                    enemyGrid[row, col] = new Enemy(rowSpread[row], startPos * new Vector2(row * gridCellSize, col * gridCellSize), (int)windowSize.X, (int)windowSize.Y);
                }
            }
        }
    }
}