using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snake_Server;
using System.Net.Sockets;
namespace Snake_Server
{
    public class Game
    {
        public Player[] players = new Player[2]; //players is an array of 2 Player objects.
        private int numOfPlayers = 0;
        bool started = false;
        

        public Game(Socket handler)
        {
            players[numOfPlayers] = new Player(handler);
            numOfPlayers++;
        }

        public Player[] playerHandlers()
        {
            return players;
        }

        public void addPlayer(Socket handler)
        {
            if (this.isGameNotFull())
            {
                players[numOfPlayers - 1] = new Player(handler);
                numOfPlayers++;
                if (numOfPlayers == 2)
                {
                    started = true;
                }
            }
        }
        public int totalPlayers()
        {
            return numOfPlayers;
        }
        public bool isGameStarted()
        {
            return started;
        }
        public bool isGameNotFull()
        {
            return (numOfPlayers < 2);
        }

        public bool isGameFull()
        {
            return (numOfPlayers == 2);
        }


    }
}
