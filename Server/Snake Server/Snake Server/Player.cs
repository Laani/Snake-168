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
        List<String> messageQueue=new List<String>();
        int playerNum;
        string playerName;
        int score;

        public Player(Socket handler, string name)
        {
            playerHandler = handler;
            playerName = name;
        }

        public Player(Socket handler,int num,string name)
        {
            playerHandler = handler;
            playerNum = num;
            playerName = name;
            score = 0;
           
        }

        public int getPlayerNum()
        {
            return playerNum;
        }

        public string getPlayerName()
        {
            return playerName;
        }

        public Socket handler()
        {
            return playerHandler;
        }

        public bool isDead()
        {
            return dead;
        }
        public void addMessage(String data)
        {
            messageQueue.Add(data);
        }
        public String popMessage()
        {
            Console.WriteLine("Inside Pop Message");
            if (messageQueue.Count>0){
                String message = messageQueue[0];
                Console.WriteLine("Popped");
                messageQueue.Remove(message);
                return message;
            }
            return "";
        }
        public void addScore()
        {
            score++;
        }
    }
}
