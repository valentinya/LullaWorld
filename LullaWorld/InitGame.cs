#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

#endregion

namespace LullaWorld
{

    /**
    * Thomas K. Johansen, Thea Alnæs
    * Programmering 3 prosjekt
    * 30.05.2014
    */

    /**
     * Dette er hovedblobben. 
     * Den tar seg av:
     * Tegning av kart
     * Initialisering av Verden
     * Initialisering av Spiller (og kalling på Draw i Spiller)
     * Utskrift til spiller
     * Insetting av highscore
     * Loading av det meste av content
     * Holde styr på GameState og Level 
     * Bruker mye static siden InitGame aldri vil bli instansiert på nytt og det ble kronglete med å sende verdier hit og dit.
     * 
     * "Hierarkiet" (rotet) ser slik ut:
     * InitGame -> InnloggetBruker
     * InitGame -> Verden
     *          Verden -> Fiende -> TurretLaser
     *          Verden -> Spiller
     *          Verden -> Juvel
     *          Verden -> Boss -> TurretLaser
     *          
     * InitGame : Game
     * Verden : DrawableGameComponent
     * Vi synes egentlig ikke at vi hadde spesielt behov for components her og der, så i Fiende.cs, Spiller.cs og Boss.cs
     * bruker vi heller eget interface IMyDrawable for LoadContent/Update/Draw. 
     *  
     * Vi har jobbet på prosjektet inntil siste frist, så noe av dette er ganske ferskt og uprøvd 
     * (spesielt Boss har ikke blitt testet så mye)
     * 
     */


    public enum GameState
    {
        Loading,
        Playing,
        End,
        Won
    }


   public class InitGame : Game
    {
        private GameState _currentGameState = GameState.Playing;
        protected internal static Level CurrentLevel { get; set; }

        //Mucho static
        protected internal static Camera Camera;
        protected internal static Color[] KollisjonsKartData; //Fargedata fra kollisjonskartet, brukes for å plassere fiender (turrets)

        //Textures, lyd og shader
        protected internal static Texture2D KollisjonsKart; //Texture til kart
        protected internal static Texture2D JuvelTexture2D; //Texture til juvel
        protected internal static Texture2D FiendeGunTexture2D; //Texture til turret
        protected internal static Texture2D FiendeLaserTexture2D; //Texture til laser
        protected internal static SoundEffect JuvelLyd; //Lyd som spilles av når man plukker opp juvel
        

        protected internal static Spiller Spilleren;
        protected internal static Boss TheBoss;
        private InnloggetBruker _brukeren;
        private Verden _verden;

        private Texture2D _fargeKartBakke; //Texture til fargekart bakke
        private Texture2D _fargeKartBakkeDel2; //Kart#2 som ikke lenger brukes
        private Texture2D _fargeKartHimmel; //Texture til fargekart himmel
        private Texture2D _blomstKartDel1; //Ville egentlig ha parallaxing men hadde ikke tid
        private Texture2D _blomstKartDel2; //Kart#2 som ikke lenger brukes
        private Effect _myEffect; //Shader
        private SpriteFont _font1;

       // private List<int> _highScores;  //Skulle ha skrevet ut en liste med highscores men hadde ikke tid
        private readonly GraphicsDeviceManager _graphics;
        private readonly StringBuilder _outputStringBuilder;
        
        private SpriteBatch _spriteBatch;
        private Color _fontColor = Color.Black;
        private SoundEffect _backMusic;
        private SoundEffectInstance _songInstance;   
        private TimeSpan _totalTime;
       

        private bool _isDbUpdated;
        private int _finalScore;
        private float _fontScale;
        private float _helseScale;
        private double _spillerHelse;

        public InitGame()

        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Window.Title = "LullaWorld";
            Window.AllowUserResizing = true;
            _graphics.IsFullScreen = true;
            TheBoss = new Boss();
            _outputStringBuilder=new StringBuilder();
        }

        protected override void Initialize()
        {
            // Sette skjermstørrelse
            //Monogame er fremdeles litt buggy og vi har ikke tilgang på Windows.Forms
            //Fullscreen og position kan bl.a. ikke settes.
            _graphics.PreferredBackBufferHeight = 820;
            _graphics.PreferredBackBufferWidth = 1260;
            _graphics.ApplyChanges();
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _fontScale = 1.0f;
            CurrentLevel = Level.One;
            var spillerPos = new Vector2(1700,800);
            Spilleren = new Spiller(spillerPos);

            base.Initialize();
        }


        protected override void LoadContent()
        {
           
            _font1 = Content.Load<SpriteFont>("SpriteFont1");
            _backMusic = Content.Load<SoundEffect>("sang");  //Bakgrunnsmusikk

            KollisjonsKart = Content.Load<Texture2D>("CollisionMapPart1");
            JuvelTexture2D = Content.Load<Texture2D>("shard2");
            FiendeGunTexture2D = Content.Load<Texture2D>("enemy_gun.png");
            FiendeLaserTexture2D = Content.Load<Texture2D>("laserBeam.png");
            JuvelLyd = Content.Load<SoundEffect>("Xylo1");

            _fargeKartBakke = Content.Load<Texture2D>("ColorMapPart1");
            _fargeKartBakkeDel2 = Content.Load<Texture2D>("ColorMapPart2");
            _fargeKartHimmel = Content.Load<Texture2D>("colormaphimmel");
            _blomstKartDel1 = Content.Load<Texture2D>("FlowerMap");
            _blomstKartDel2 = Content.Load<Texture2D>("FlowerMapPart2");

            _myEffect = Content.Load<Effect>("himmelEffekt"); //Shader 

            Spilleren.LoadContent(this);

            //Kamera
            Camera = new Camera(_fargeKartBakke.Width, new Vector2(Spilleren.Posisjon.X + 250, Spilleren.Posisjon.Y));

            _verden = new Verden(this);
           Components.Add(_verden); //Legger til Verden som Game component

            if(!TheBoss.IsDead)
                TheBoss.LoadContent(this); //Loade content for boss

            _songInstance = _backMusic.CreateInstance();

            _songInstance.IsLooped = true;
            _songInstance.Volume = 0.1f;
            _songInstance.Pitch = 1f;
            _songInstance.Pan = 1f;
            _songInstance.Play();
        }


        protected override void UnloadContent()
        {
           Content.Unload();
           
        }

   
       /// <summary>
       /// UPDATE
       /// </summary>
       /// <param name="gameTime"></param>
        protected override void Update(GameTime gameTime)
        {
            _totalTime += gameTime.ElapsedGameTime;
          
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Spilleren != null)
            {
               
                //Bytter gamestate avhengig av helse til spiller og boss
                if (!Spilleren.IsDead && TheBoss.IsDead)
                    _currentGameState = GameState.Won;
                else if (Spilleren.IsDead)
                    _currentGameState = GameState.End;
                else
                    _currentGameState = GameState.Playing;
               
                switch (_currentGameState)
                {

                    case GameState.Playing:

                        InitCameraAndShader();
                        _spillerHelse = Spilleren.Helse;
                        Spilleren.Update(gameTime);
                        CheckPlayerHealth();
                        CurrentLevel=CheckLevel();

                        if (CurrentLevel == Level.Boss && !TheBoss.IsDead)
                            TheBoss.Update(gameTime);
                        break;

                    case GameState.End:
                        CurrentLevel = Level.None;
                         _finalScore = (int)(Spilleren.Score/1+(gameTime.ElapsedGameTime.TotalSeconds)); //Score avhenger av tid
                        _totalTime = gameTime.ElapsedGameTime;
                        _fontColor = Color.Red;
                        _fontScale = 1.5f;

                        //Starter spillet på nytt om brukeren trykker space
                        if (Keyboard.GetState().IsKeyDown(Keys.Space))
                        {

                            Spilleren.Posisjon = new Vector2(1700, 800);
                            Spilleren.Hastighet *= 0;
                            Spilleren.IsDead = false;
                            Spilleren.Helse = 100;
                            Spilleren.Score = 0;
                            
                            _totalTime = new TimeSpan();
                            _isDbUpdated = false;
                            _helseScale = (float)Spilleren.Helse / 100;
  
                            _verden.ReInit(); //Tegner juveler på nytt

                           CurrentLevel = Level.One;
                           _currentGameState = GameState.Playing;
                        }


                        break;
                    case GameState.Won:
                        CurrentLevel = Level.Won;
                        _finalScore = (int)(Spilleren.Score/1+(gameTime.ElapsedGameTime.TotalSeconds));
                        _totalTime = gameTime.ElapsedGameTime;
                        _fontColor = Color.Green;
                        _fontScale = 1.5f;

                        //Lagrer highscore
                        if (!_isDbUpdated && _finalScore > _brukeren.HighScore)  //Spiller er død og oppdataring har ikke skjedd, update score i db og sett bool til true
                        {

                            DBConnect db = new DBConnect();
                            db.InsertScore(Spilleren.Score, _brukeren.Id); //sette inn score når spiller er død.
                            _isDbUpdated = true;
                        }
                        break;
                }
            }

            WriteToScreen(CurrentLevel);
            base.Update(gameTime);
        }

       /// <summary>
       /// Utskrift til skjermen basert på level
       /// </summary>
       /// <param name="currentLevel"></param>
        private void WriteToScreen(Level currentLevel)
        {
            _outputStringBuilder.Clear();
            _outputStringBuilder.AppendLine("");

            switch (currentLevel)
            {

                case Level.One:
                    _outputStringBuilder.Append("   Helse: " + (int) Spilleren.Helse);
                    _outputStringBuilder.AppendLine("   Juveler igjen i Level 1: " + _verden.GetJuvelSum(Level.One));
                    _outputStringBuilder.AppendLine("   Score: " + Spilleren.Score);
                    _outputStringBuilder.AppendLine("   Tid: " + _totalTime);
                    break;

                case Level.Two:
                    _outputStringBuilder.AppendLine("   Helse: " + (int) Spilleren.Helse);
                    _outputStringBuilder.AppendLine("   Juveler igjen i Level 2: " + _verden.GetJuvelSum(Level.Two));
                    _outputStringBuilder.AppendLine("   Score: " + Spilleren.Score);
                    _outputStringBuilder.AppendLine("   Tid: " + _totalTime);
                    break;

                case Level.Three:
                     _outputStringBuilder.AppendLine("   Helse: " + (int) Spilleren.Helse);
                    _outputStringBuilder.AppendLine("   Juveler igjen i Level 3: " + _verden.GetJuvelSum(Level.Three));
                    _outputStringBuilder.AppendLine("   Score: " + Spilleren.Score);
                    _outputStringBuilder.AppendLine("   Tid: " + _totalTime);
                    break;
                case Level.Boss:
                     _outputStringBuilder.AppendLine("   Helse: " + (int) Spilleren.Helse);
                     _outputStringBuilder.AppendLine("Boss helse: " + TheBoss.Helse);
                    _outputStringBuilder.AppendLine("   Score: " + Spilleren.Score);
                    _outputStringBuilder.AppendLine("   Tid: " + _totalTime);
                    break;

                case Level.None:
                    _outputStringBuilder.AppendLine("Haha you are dead!");
                    _outputStringBuilder.AppendLine("Din score: " + _finalScore);
                    _outputStringBuilder.AppendLine(" Trykk SPACE for aa starte paa nytt!");
                    break;

                case Level.Won:
                    _outputStringBuilder.Append("GRATULERER! Du vant!");
                    _outputStringBuilder.Append("Din score: " +_finalScore);
                    break;
            }
        }


       /// <summary>
       /// Sjekker hvilken level vi er i avhengig av posisjon
       /// </summary>
       /// <returns></returns>
        private Level CheckLevel()
        {

            if (_verden.GetJuvelSum(Level.Boss) == 0)
            {
               return Level.Boss;
               
            }
                

            if (Spilleren.Posisjon.X < (KollisjonsKart.Width / 3)) 
               return Level.One;
            if ((Spilleren.Posisjon.X > (KollisjonsKart.Width / 3) 
                && Spilleren.Posisjon.X < 2 * (KollisjonsKart.Width / 3)))
                return Level.Two;
            if ((Spilleren.Posisjon.X > 2 * (KollisjonsKart.Width / 3)) 
                && Spilleren.Posisjon.X < KollisjonsKart.Width)
                return Level.Three;

            return Level.None;
        }

       /// <summary>
       /// Sjekker helsen til spilleren og endrer fontfarge
       /// Men funker ikke helt som den skal, men ikke nok tid til debug..
       /// </summary>
        private void CheckPlayerHealth()
        {
            //Gjør spiller oppmerksom på forandring i helse, bytte farge og skalere

            if (Spilleren.Helse < _spillerHelse)
            {
                _fontColor = Color.Red;

                if(Spilleren.Score>0)
                    Spilleren.Score -= 1;
                
            }
           if (Spilleren.Helse > _spillerHelse)
            {
                _fontColor = Color.LightGreen;
               
            }
            if(Spilleren.Helse==_spillerHelse)
            {
                _fontColor = Color.Black;
                _fontScale = 1f;
            }
        }

        private void InitCameraAndShader()
        {
            if (Spilleren.Posisjon.X >= 700)
                Camera.Posisjon = Spilleren.Posisjon;
            
   
            _myEffect.Parameters["ShipX"].SetValue(Spilleren.Posisjon.X); //til shader
            _myEffect.Parameters["ShipY"].SetValue(Spilleren.Posisjon.Y * 2.6f); //til shader
        }

    /// <summary>
    /// DRAW
    /// </summary>
    /// <param name="gameTime"></param>
        protected override void Draw(GameTime gameTime)
        {
            _graphics.GraphicsDevice.Clear(Color.White);

            if (_helseScale >= 0) _helseScale = (float)Spilleren.Helse / 100f;   //skalerer helsebar med denne verdien

            //Tegner himmelen og bakgrunnen
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.AnisotropicClamp, DepthStencilState.Default, RasterizerState.CullNone,_myEffect);
            _spriteBatch.Draw(
                _fargeKartHimmel, 
                new Vector2(-Spilleren.Posisjon.X * 0.0000001f - 512, -Spilleren.Posisjon.Y * 0.0000001f - 280),
                Color.White
                );
            _spriteBatch.End();
            
           if(CurrentLevel==Level.Boss)
                 TheBoss.Draw(_spriteBatch); //Tegner bossen

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
                    DepthStencilState.Default, RasterizerState.CullNone, null, Camera.GetTransformasjon());
            _spriteBatch.Draw(
                _blomstKartDel1,
                new Rectangle(0, 0, _blomstKartDel1.Width, _blomstKartDel1.Height), 
                Color.White
                );
            _spriteBatch.Draw(
                _blomstKartDel2,
                new Rectangle(_blomstKartDel1.Width - 18, 0, _blomstKartDel2.Width, _blomstKartDel2.Height), 
                Color.White)
                ;
            _spriteBatch.End(); //

            _spriteBatch.Begin();//tegne HelseBar
            _spriteBatch.Draw(
                FiendeLaserTexture2D,
                new Vector2(0, 25),
                null, Color.Multiply(Color.LightGreen, 1.4f - _helseScale), 
                0f, 
                new Vector2(0, 0), 
                new Vector2(5.0f * _helseScale, 3.0f),
                SpriteEffects.None, 0f
                );
            _spriteBatch.End();

            //Tegner bakke og hinder
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.AnisotropicClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Camera.GetTransformasjon());
            _spriteBatch.Draw(
                _fargeKartBakke, 
                new Rectangle(0, 0, KollisjonsKart.Width, KollisjonsKart.Height),
                Color.White
                );
            _spriteBatch.Draw(
                _fargeKartBakkeDel2, 
                new Rectangle(KollisjonsKart.Width, 0, KollisjonsKart.Width, KollisjonsKart.Height),
                Color.White
                );
            _spriteBatch.End();

            //Tegner strengen
            _spriteBatch.Begin();
            _spriteBatch.DrawString(
                _font1,
                _outputStringBuilder,
                new Vector2(0, 0), 
                _fontColor,
                0, 
                new Vector2(0,0), 
                _fontScale, 
                SpriteEffects.None, 0.5f
                );

            _spriteBatch.End(); //End drawing stuff

            Spilleren.Draw(_spriteBatch); //Tegner spilleren

            base.Draw(gameTime); 
        }
    }
}
