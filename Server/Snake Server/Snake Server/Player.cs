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

        public Player(Socket handler,int num)
        {
            playerHandler = handler;
            playerNum = num;
            Console.WriteLine(playerHandler.LocalEndPoint);
        }

        public int getPlayerNum()
        {
            return playerNum;
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
    }
}
