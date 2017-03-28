using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace LullaWorld

{
    /**
   * Thomas K. Johansen, Thea Alnæs
   * Programmering 3 prosjekt
   * 30.05.2014
   */
    /**
     * Enkelt loginform. Validerer input med regex, sjekker brukernavn og passord mot DB (ikke hashet),
     * henter ut tilhørende informasjon om brukeren og en liste over alle highscores (som vi aldri fikk tid til å bruke)
     */

    public partial class LoginFormet : Form
    {
        public string SpillerNavn { get; set; }
        public int SpillerID { get; set; }
        public int SpillerHighScore { get; set; }
        public List<int> AllHighScore { get; set; }
        public bool DoRun { get; set; }


        private bool _passordValidated;
        private bool _brukerNavnValidated;
        private string _finalPassord;
        private string _finalBrukernavn;
        private readonly DBConnect _dbConnect;

        public LoginFormet()
        {
            InitializeComponent();
            CenterToScreen();
            _dbConnect = new DBConnect();
        }

       /// <summary>
       /// Eventmetode for loginknapp
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
        private void buttonLogin_Click(object sender, EventArgs e)
        {
            if (!_brukerNavnValidated || !_passordValidated) return;

            if (_dbConnect.CheckPassword(_finalBrukernavn, _finalPassord))
            {
                SpillerNavn = _finalBrukernavn;
                SpillerID = _dbConnect.GetPlayerId(_finalBrukernavn);
                SpillerHighScore = _dbConnect.GetPersonalBest(SpillerID);
             //   AllHighScore = _dbConnect.GetHighScores();

                //Program.cs sjekker om DoRun er true, lukker så dette formet sånn at tråden kan kjøre spillet.
                if (!String.IsNullOrEmpty(SpillerNavn) && !String.IsNullOrEmpty(SpillerID.ToString()) &&
                    !String.IsNullOrEmpty(SpillerHighScore.ToString()))
                {
                    DoRun = true;
                    Close();
                }
                else
                    errorProviderUserName.SetError(UserNameBox,
                    "Database error");

                
            }
            else
            {
                UserNameBox.Clear();
                PasswordBox.Clear();
                DoRun = false;
                errorProviderUserName.SetError(UserNameBox, "Wrong username or password!");

            }
        }

        /// <summary>
        /// Validere brukernavn
        /// Minimum antall characters 3, max 15
        /// Alle alfanumeriske tegn
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserNameBox_Validating(object sender, CancelEventArgs e)
        {
            errorProviderUserName.Clear();

            var regex = new Regex(@"^[\w]{3,15}$");
            if (regex.IsMatch(UserNameBox.Text))
            {
                _brukerNavnValidated = true;
                _finalBrukernavn = UserNameBox.Text;
            }
            else
            {
                errorProviderUserName.SetError(UserNameBox,
                    "Wrong format");
            }
        }

        /// <summary>
        /// Validere passord
        /// Minimum antall characters 3, max 15
        /// Alle alfanumeriske tegn
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PasswordBox_Validating(object sender, CancelEventArgs e)
        {
            var regex = new Regex(@"^[\w]{3,15}$");

            //Hasher ikke, fordi tidsklemme.
            if (regex.IsMatch(PasswordBox.Text))
            {
                _passordValidated = true;
                _finalPassord = PasswordBox.Text;
            }
            else
            {
                errorProviderUserName.SetError(PasswordBox,
                    "Wrong format"); //Errorprovider viser melding
            }
        }

        /// <summary>
        /// For å lukke formet.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void labelExit_Click(object sender, EventArgs e)
        {
            DoRun = false;
            Close();
        }
    }
}
