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


        public Player(Socket handler)
        {
            playerHandler = handler;
            Console.WriteLine(playerHandler.LocalEndPoint);
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
            if (messageQueue.Count>0){
                String message = messageQueue[0];
                messageQueue.Remove(message);
                return message;
            }
            return "";
        }
    }
}
