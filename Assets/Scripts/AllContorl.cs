using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllContorl : MonoBehaviour
{
    public class GameManager
    {
        //µ¥ÀýÄ£Ê½
        private static GameManager _instance;
        
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new GameManager();
                return _instance;
            }
        }

        //¼Æ·Ö°åÊý¾Ý
        public int score = 0;

        public int playerLives = 3;

        public void ResetRunState()
        {
            score = 0;
            playerLives = 3;
        }
    }
}
