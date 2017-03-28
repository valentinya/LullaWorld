using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using MySql.Data.MySqlClient;

namespace LullaWorld
{
    /**
   * Thomas K. Johansen, Thea Alnæs
   * Programmering 3 prosjekt
   * 30.05.2014
   */
    internal class DBConnect
    {
        private MySqlConnection _connection;
        private string _server;
        private string _database;
        private string _uid;
        private string _password;

        private MySqlDataAdapter mySqlDataAdapter;

        public DBConnect()
        {
            Initialize();
        }

        private void Initialize()
        {
            //Kan ikke gi ut skolens DB info
        }

        //åpne forbindelse
        public bool OpenConnection()
        {
            try
            {
                _connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 0:
                        MessageBox.Show("Cannot connect to server.  Contact administrator");
                        break;

                    case 1045:
                        MessageBox.Show("Invalid username/password, please try again");
                        break;
                }
                return false;
            }
        }

        //Lukk forbindelse
        private bool CloseConnection()
        {
            try
            {
                _connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message, "Feilmelding");
                return false;
            }
        }

        /// <summary>
        /// Henter spillerID
        /// </summary>
        /// <param name="brukernavn"></param>
        /// <returns></returns>
        public int GetPlayerId(string brukernavn)
        {
            if (!OpenConnection()) return 0;

            const string queryText = "SELECT ID FROM Bruker " +
                                     "WHERE Brukernavn = @Brukernavn";

            using (var cmd = new MySqlCommand(queryText, _connection))
            {
                cmd.Parameters.AddWithValue("@Brukernavn", brukernavn);
                MySqlDataReader dr = cmd.ExecuteReader();
                dr.Read();


                if (!dr.HasRows) return 0;

                int result = Convert.ToInt16(dr["ID"].ToString());
                CloseConnection();
                return result;
            }
        }

        /// <summary>
        /// Henter beste scoren til brukeren
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Int32 GetPersonalBest(int id)
        {
            if (!OpenConnection()) return 0;
            const string queryText = "SELECT MAX(score) as maxscore FROM HighScore " +
                                     "WHERE brukerId = @ID";

            using (var cmd = new MySqlCommand(queryText, _connection))
            {
                cmd.Parameters.AddWithValue("@ID", id); // cmd is SqlCommand 
                // int result = (int)cmd.ExecuteScalar();
                MySqlDataReader dr = cmd.ExecuteReader();


                if (!dr.HasRows) return 0;

                dr.Read();
                int result = Convert.ToInt32(dr["maxscore"].ToString());
                CloseConnection();
                return result;
            }
            return 0;
        }

        /// <summary>
        /// Sjekker passord
        /// </summary>
        /// <param name="brukernavn"></param>
        /// <param name="passord"></param>
        /// <returns></returns>
        public bool CheckPassword(string brukernavn, string passord)
        {
            if (!OpenConnection()) return false;

            const string queryText = "SELECT * FROM Bruker " +
                                     "WHERE Brukernavn = @Brukernavn AND Passord = @Passord";


            using (var cmd = new MySqlCommand(queryText, _connection))
            {
                cmd.Parameters.AddWithValue("@Brukernavn", brukernavn); 
                cmd.Parameters.AddWithValue("@Passord", passord);
                MySqlDataReader dr = cmd.ExecuteReader();
        

                if (!dr.HasRows) return false;

                CloseConnection();

                return true;
            }
        }

        /// <summary>
        ///  Henter ut highscores, sortert stigende og returnerer et dataset
        /// </summary>
        /// <param name="_query">string</param>
        /// <returns>BindingList</returns>
        public List<int> GetHighScores()
        {
            /* if (!OpenConnection()) return null;

             const string queryText = "SELECT HighScore FROM Bruker ORDER BY HighScore DESC";
             var listToReturn = new List<int>();
             using (var cmd = new MySqlCommand(queryText, connection))
             {
                 // cmd.Parameters.AddWithValue("@ID", id); // cmd is SqlCommand 
                 // int result = (int)cmd.ExecuteScalar();
                 MySqlDataReader dr = cmd.ExecuteReader();

                 if (dr.HasRows)
                 {
                     while (dr.Read())
                     {
                         listToReturn.Add(Convert.ToInt16(dr["HighScore"].ToString()));
                     }
                     CloseConnection();
                     return listToReturn;
                 }
             }*/
            return null;
        }

        /// <summary>
        /// Setter inn en ny highscore
        /// </summary>
        /// <param name="_name">String</param>
        /// <param name="_score">int</param>
        public void InsertScore(int _score, int _id)
        {
            String insertQ = String.Format("INSERT INTO HighScore(score, brukerID) VALUES({0}, {1})", _score, _id);

            if (!OpenConnection()) return;

            try
            {
                var updateCommand = new MySqlCommand(insertQ, _connection);
                updateCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                MessageBox.Show("Feil med oppdatering av DB: " + e.Message);
            }

            CloseConnection(); //lukk forbindelse
        }
    }
}
