using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
namespace Snake_Server
{
    public class Player
    {
        bool dead = false;
        public Socket playerHandler;

        public Player(Socket handler)
        {
            playerHandler = handler;
        }

        public Socket handler()
        {
            return playerHandler;
        }

        public bool isDead()
        {
            return dead;
        }
    }
}
