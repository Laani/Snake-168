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
            players.Add(new Player(handler,numOfPlayers));
            players[numOfPlayers].addMessage("one<EOF>");
            numOfPlayers++;
        }

        public List<Player> playerHandlers()
        {
            return players;
        }

        //public String addPlayer(Socket handler)
        //{
        //    if (this.isGameNotFull())
        //    {
        //        players.Add(new Player(handler));
        //        players[numOfPlayers].addMessage(playerNum[numOfPlayers]);
        //        numOfPlayers++;
                
        //        if (numOfPlayers == 2)
        //        {
        //            started = true;
        //        }
        //        return playerNum[numOfPlayers-1];
        //    }
        //    return "";
        //}
        public String getNextMessage(Socket handler)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].handler() == handler)
                {
                    return players[i].popMessage();
                }
            }
            return "";
        }


        public void addPlayer(Socket handler)
        {
            if (this.isGameNotFull())
            {
                players.Add(new Player(handler,numOfPlayers));
                players[numOfPlayers].addMessage(playerNum[numOfPlayers]);
                numOfPlayers++;

                if (numOfPlayers == 2)
                {
                    started = true;
                }
            }
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

        public List<Player> getOtherPlayers(Socket handler)
        {
            List<Player> otherPlayers = new List<Player>();
            for (int i =0; i<players.Count;i++)
            {
                if (players[i].handler()!=handler)
                {
                    otherPlayers.Add(players[i]);
                }
            }
            return otherPlayers;
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
