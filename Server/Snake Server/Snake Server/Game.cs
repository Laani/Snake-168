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
        public List<Player> players= new List<Player>(); //players is an array of 2 Player objects.
        private int numOfPlayers = 0;
        bool started = false;
        List<String> playerNum = new List<String>() { "one<EOF>", "two<EOF>", "three<EOF>", "four<EOF>" };

        public Game()
        {

        }

        public Game(Socket handler)
        {
            players.Add(new Player(handler));
            numOfPlayers++;
        }

        public List<Player> playerHandlers()
        {
            return players;
        }

        public String addPlayer(Socket handler)
        {
            if (this.isGameNotFull())
            {
                players.Add(new Player(handler));
                numOfPlayers++;
                if (numOfPlayers == 2)
                {
                    started = true;
                }
                return playerNum[numOfPlayers-1];
            }
            return "";
        }

        public bool playerInThisGame(Socket handler)
        {
            for (int i =0; i<players.Count;i++)
            {
                if (players[i].handler() == handler)
                {
                    return true;
                }
            }
            return false;
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
