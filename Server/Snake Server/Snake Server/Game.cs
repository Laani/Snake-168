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
        private int playersInLobby = 0;
        bool started = false;
        private String gameName;
        List<String> playerNum = new List<String>() { "one<EOF>", "two<EOF>", "three<EOF>", "four<EOF>" };
        List<string> chatMessages = new List<string>();
        
        List<bool> playerReady = new List<bool>() { false, false };

        public Game()
        {

        }

        public Game(Socket handler, string playerName)
        {
            players.Add(new Player(handler, numOfPlayers, playerName));
            players[numOfPlayers].addMessage("one<EOF>");
            numOfPlayers++;
        }
        public Game(Socket handler, Player player, string gameName)
        {
            players.Add(player);
            players[numOfPlayers].addMessage("one<EOF>");
            numOfPlayers++;
            this.gameName = gameName;
        }
        public void changeAReadyToTrue()
        {
            for (int i =0; i<playerReady.Count;i++)
            {
                if (playerReady[i]==false)
                {
                    playerReady[i] = true;
                }
            }
        }
        public void changeAReadyToFalse()
        {
            for (int i = 0; i < playerReady.Count; i++)
            {
                if (playerReady[i] == true)
                {
                    playerReady[i] = false;
                }
            }
        }

        public bool allPlayersReady()
        {
            for (int i = 0; i<playerReady.Count;i++)
            {
                if (playerReady[i]==false)
                {
                    return false;
                }
            }
            return true;
        }
        public List<Player> playerHandlers()
        {
            return players;
        }

        public String getGameName()
        {
            return gameName;
        }
        public void addChatMessage(string message)
        {
            chatMessages.Add(message);
            if (chatMessages.Count>5)
            {
                chatMessages.RemoveAt(0);
            }
        }
        public string getChatMessages()
        {
            string chatBox="cha ";
            for (int i = 0; i < chatMessages.Count;i++ )
            {
                chatBox += chatMessages[i];
                chatBox += "\n";
            }
            chatBox += "<EOF>";
            return chatBox;
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


        public void addPlayer(Socket handler, string name)
        {
            if (this.isGameNotFull())
            {
                players.Add(new Player(handler,numOfPlayers,name));
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

        public int inLobby()
        {
            return playersInLobby;
        }

        public void addLobby()
        {
            playersInLobby++;
        }
    }
}
