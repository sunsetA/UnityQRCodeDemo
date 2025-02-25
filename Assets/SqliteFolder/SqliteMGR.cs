using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System;
using Mono.Data.SqliteClient;
public  class SqliteMGR:MonoBehaviour
{
    public class UserData 
    {
        public int UserID;
        public string UserName;
        public GameData UserGameData;

    }
    public class GameData
    {

        public string GameName;
        public string GameCateloge;
        public int GameScore;
        public string GameResult;
        public string GameStartTime;
        public string GameEndTime;
        public float GameDuration;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            KinectGameDBHelper kinectGameDBHelper = new KinectGameDBHelper();
            UserData userData = new UserData();
            userData.UserID = 0;
            userData.UserName = "123";
            userData.UserGameData = new GameData();
            userData.UserGameData.GameName = "kuaile ";
            userData.UserGameData.GameCateloge = "花";
            userData.UserGameData.GameScore = 100;
            userData.UserGameData.GameResult = "成功";
            userData.UserGameData.GameStartTime = "10";
            userData.UserGameData.GameEndTime = "12";
            userData.UserGameData.GameDuration = 2;
            kinectGameDBHelper.InsertUserData(userData);

        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            KinectGameDBHelper kinectGameDBHelper = new KinectGameDBHelper();
            Debug.Log(JsonConvert.SerializeObject(kinectGameDBHelper.GetUserData(0))); ;
        }
    }



    public  class KinectGameDBHelper
    {
        private string connectionString;

        public KinectGameDBHelper() 
        {
            connectionString = "URI=file:" + Path.Combine(Directory.GetParent(Environment.CurrentDirectory).FullName, "KinectGameDB.db");
        }
        public KinectGameDBHelper(string databasePath)
        {
            connectionString = "URI=file:" + databasePath;
        }

        // 插入数据
        public void InsertUserData(UserData userData)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string insertQuery = @"
                INSERT INTO KinectGame (UserID, UserName, GameName, GameCateloge, GameScore, GameResult, GameStartTime, GameEndTime, GameDuration)
                VALUES (@UserID, @UserName, @GameName, @GameCateloge, @GameScore, @GameResult, @GameStartTime, @GameEndTime, @GameDuration);";

                using (SqliteCommand command = new SqliteCommand(insertQuery, connection))
                {
                    command.Parameters.Add("@UserID", userData.UserID);
                    command.Parameters.Add("@UserName", userData.UserName);
                    command.Parameters.Add("@GameName", userData.UserGameData.GameName);
                    command.Parameters.Add("@GameCateloge", userData.UserGameData.GameCateloge);
                    command.Parameters.Add("@GameScore", userData.UserGameData.GameScore);
                    command.Parameters.Add("@GameResult", userData.UserGameData.GameResult);
                    command.Parameters.Add("@GameStartTime", userData.UserGameData.GameStartTime);
                    command.Parameters.Add("@GameEndTime", userData.UserGameData.GameEndTime);
                    command.Parameters.Add("@GameDuration", userData.UserGameData.GameDuration);

                    command.ExecuteNonQuery();
                }
            }
        }

        // 删除数据
        public void DeleteUserData(int userID)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string deleteQuery = "DELETE FROM KinectGame WHERE UserID = @UserID;";

                using (SqliteCommand command = new SqliteCommand(deleteQuery, connection))
                {
                    command.Parameters.Add("@UserID", userID);
                    command.ExecuteNonQuery();
                }
            }
        }

        // 更新数据
        public void UpdateUserData(UserData userData)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string updateQuery = @"
                UPDATE KinectGame
                SET UserName = @UserName,
                    GameName = @GameName,
                    GameCateloge = @GameCateloge,
                    GameScore = @GameScore,
                    GameResult = @GameResult,
                    GameStartTime = @GameStartTime,
                    GameEndTime = @GameEndTime,
                    GameDuration = @GameDuration
                WHERE UserID = @UserID;";

                using (SqliteCommand command = new SqliteCommand(updateQuery, connection))
                {
                    command.Parameters.Add("@UserID", userData.UserID);
                    command.Parameters.Add("@UserName", userData.UserName);
                    command.Parameters.Add("@GameName", userData.UserGameData.GameName);
                    command.Parameters.Add("@GameCateloge", userData.UserGameData.GameCateloge);
                    command.Parameters.Add("@GameScore", userData.UserGameData.GameScore);
                    command.Parameters.Add("@GameResult", userData.UserGameData.GameResult);
                    command.Parameters.Add("@GameStartTime", userData.UserGameData.GameStartTime);
                    command.Parameters.Add("@GameEndTime", userData.UserGameData.GameEndTime);
                    command.Parameters.Add("@GameDuration", userData.UserGameData.GameDuration);

                    command.ExecuteNonQuery();
                }
            }
        }

        // 查询数据
        public UserData GetUserData(int userID)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string selectQuery = "SELECT * FROM KinectGame WHERE UserID = @UserID;";

                using (SqliteCommand command = new SqliteCommand(selectQuery, connection))
                {
                    command.Parameters.Add("@UserID", userID);
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            UserData userData = new UserData
                            {
                                UserID = Convert.ToInt32(reader["UserID"]),
                                UserName = reader["UserName"].ToString(),
                                UserGameData = new GameData
                                {
                                    GameName = reader["GameName"].ToString(),
                                    GameCateloge = reader["GameCateloge"].ToString(),
                                    GameScore = Convert.ToInt32(reader["GameScore"]),
                                    GameResult = reader["GameResult"].ToString(),
                                    GameStartTime = reader["GameStartTime"].ToString(),
                                    GameEndTime = reader["GameEndTime"].ToString(),
                                    GameDuration = Convert.ToSingle(reader["GameDuration"])
                                }
                            };
                            return userData;
                        }
                    }
                }
            }
            return null;
        }
    }

}
