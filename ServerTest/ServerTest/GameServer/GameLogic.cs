using GameServer.ThreadManager;

namespace ServerTest
{
    public class GameLogic
    {
        public static void Update()
        {
            ThreadManager.UpdateMain();
        }
    }
}
