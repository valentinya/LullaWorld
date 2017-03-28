using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;


namespace LullaWorld
{

    /**
     * Thomas K. Johansen, Thea Alnæs
     * Programmering 3 prosjekt
     * 30.05.2014
     */

    //Brukes for det meste i InitGame
   public enum Level
    {
        None, One, Two, Three, Boss, Won
    }


    /**
     * Verden.cs
     * Tar seg av:
     * 
     * - Plassering (og tegning) av fiender og juveler ut fra kartdata
     * - Kollisjon med terrain
     * - Samle opp juveler (og alt som skjer da)
     * - Definerer grensene til level 1, 2 og 3 og spawner boss
     * 
     */
    class Verden : DrawableGameComponent
    {
        //Konstanter for fysikk
        protected internal const float Sirkel = MathHelper.Pi * 2;
        protected internal const float Gravitasjon = 0.075f;  //Skru ned om det er for "tungt" å fly.
        private readonly SoundEffectInstance _juvelLydInstance;

        //Kart og kollisjon
        protected internal static Color[] KollisjonsKartData; //Fargedata fra kollisjonskartet, brukes for å plassere fiender (turrets)

        //Lagre farger i Kart-texturen
       protected internal static Texture2D KollisjonsKart; //Texture til kart
        
        //Fiende
        private readonly List<Fiende> _fiendeListe; //Alle fiendene våre
        //Spilleren
        private readonly Spiller _spilleren; //Vår helt
        //Juveler
        private readonly List<Juvel> _juvelListe2; //Brukes til backup for å plassere juveler på nytt når man starter spill på nytt etter å ha dødd
        private readonly List<Juvel> _juvelListe; //Alle våre juveler
        private readonly SpriteBatch _spriteBatch;

        //Static fordi de brukes i Collision
        private static int _oppsamledeJuvelerLevel1;
        private static int _oppsamledeJuvelerLevel2;
        private static int _oppsamledeJuvelerLevel3;
        private static int _juvelerILevel1;
        private static int _juvelerILevel2;
        private static int _juvelerILevel3;

        public Verden(Game game) :base(game)
        {
            _juvelListe = new List<Juvel>();
            _fiendeListe = new List<Fiende>();
            _juvelListe2 = new List<Juvel>();
           
            _spilleren = InitGame.Spilleren;
            KollisjonsKart = InitGame.KollisjonsKart;
            SoundEffect juvelLyd = InitGame.JuvelLyd;

           _juvelLydInstance = juvelLyd.CreateInstance();
            _juvelLydInstance.Pitch = 0;
            _oppsamledeJuvelerLevel1 = 0;
            _oppsamledeJuvelerLevel2 = 0;
            _oppsamledeJuvelerLevel3 = 0;
            _juvelerILevel1 = 0;
            _juvelerILevel2 = 0;
            _juvelerILevel3 = 0;
            
            _spriteBatch = new SpriteBatch(game.GraphicsDevice);

            PlaceEnemies(KollisjonsKart); //Plasserer fiender
            PlaceJewels(KollisjonsKart); //Plasserer juveler
            CountJewels(_juvelListe); //Teller antall juveler, bruker metode fordi når brukeren restarter spillet brukes den samme metoden men med en annen (backup) liste
        }

        /// <summary>
        /// DRAW
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            //Tegner alle fiendene
            foreach (Fiende fiende in _fiendeListe) 
            {
                fiende.Draw(_spriteBatch);
            }

            //Tegner alle juveler
            foreach (Juvel juvel in _juvelListe)
            {
                _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
                   DepthStencilState.Default, RasterizerState.CullNone, null, InitGame.Camera.GetTransformasjon());
             
                _spriteBatch.Draw(InitGame.JuvelTexture2D, juvel.Posisjon, juvel.Farge); //Tegner Shard
                _spriteBatch.End();
            }
        }
        /// <summary>
        /// UPDATE
        /// </summary>
        /// <param name="gameTime"></param>
       public override void Update(GameTime gameTime)
       {
           //Oppdater fiende
           foreach (var fiende in _fiendeListe)
           {
               var gunToPlayerDistance = fiende.Posisjon - _spilleren.Posisjon;
               //Skyter bare om spilleren er nærme nok
               if ((Math.Abs(gunToPlayerDistance.X) < 900 && Math.Abs(gunToPlayerDistance.Y) < 900)) 
                   fiende.IsShooting = true;
               else 
                   fiende.IsShooting = false;
               
               //Oppdater alle fiendene
               fiende.Update(gameTime); 
           }

           SamleJuveler(gameTime);

       }

       /** 
        ************ HJELPEMETODER ******************
        * void ReInit(),
        * void SamleJuveler(),
        * bool Collision(Vector2 posisjon),
        * void PlaceEnemies(Texture2D kart1),
        * void PlaceJewels(Texture2D kart1),
        * int GetJuvelSum(Level level),
        */


        /// <summary>
        // Når brukeren trykker space etter å ha dødd for å prøve på nytt
        // Henter data fra backupliste og fyller originalliste på nytt
        /// </summary>
       public void ReInit()
        {
            _juvelListe.Clear();
            _oppsamledeJuvelerLevel1 = 0;
            _oppsamledeJuvelerLevel2 = 0;
            _oppsamledeJuvelerLevel3 = 0;
            _juvelerILevel1 = 0;
            _juvelerILevel2 = 0;
            _juvelerILevel3 = 0;
            CountJewels((_juvelListe2)); //Teller antall juveler på nytt

           foreach (Juvel j in _juvelListe2)
           {
               _juvelListe.Add(j);
           }        

        }
        /// <summary>
        /// SamleJuveler.
        /// Håndterer liste, poeng, helse, og spiller av lyd
        /// </summary>
        /// <param name="gameTime"></param>
       private void SamleJuveler(GameTime gameTime)
       {
           //Samle opp juveler
           if (_juvelListe.Count <= 0 || _juvelListe.Equals(null)) return;

            for (int i = 0; i < _juvelListe.Count; i++)
            {
                var juvel = _juvelListe[i];
                var juvelToPlayerDistance = juvel.Posisjon - _spilleren.Posisjon;

                if (!(juvelToPlayerDistance.Length() < 85)) continue;

                if (juvel.Farge.Equals(Color.LightGreen))
                {
                    _spilleren.Score += (int)JuvelType.Green;
                    if (_spilleren.Helse <= 90)
                        _spilleren.Helse += 10;
                }

                if (juvel.Farge.Equals(Color.Magenta))
                {
                    _spilleren.Score += (int)JuvelType.Magenta;
                    if (_spilleren.Helse <= 50)
                        _spilleren.Helse += 50;
                }

                if (juvel.Farge.Equals(Color.Red))
                {
                    _spilleren.Score += (int)JuvelType.Red;
                    _spilleren.Helse = 100;
                }

                _juvelListe.RemoveAt(i); //Fjerner juvelen spilleren plukket opp


                if (gameTime.ElapsedGameTime.Seconds <= 5)
                    _juvelLydInstance.Pitch += 0.2f; //Pitcher opp lyden om mindre enn 5 sekunder

                if (_juvelLydInstance.Pitch >= 0.6f)
                    _juvelLydInstance.Pitch = 0f; //Resetter pitch om den er for høy

                _juvelLydInstance.Play(); //Spiller av lyden

                //Teller antall juveler spilleren har samlet opp og i hvilken level
                if (juvel.Posisjon.X < (KollisjonsKart.Width / 3)) //Level 1
                    _oppsamledeJuvelerLevel1 += 1;
                if ((juvel.Posisjon.X > (KollisjonsKart.Width / 3) && juvel.Posisjon.X < 2 * (KollisjonsKart.Width / 3))) //Level 2
                    _oppsamledeJuvelerLevel2 += 1;
                if ((juvel.Posisjon.X > 2 * (KollisjonsKart.Width / 3)) && juvel.Posisjon.X < KollisjonsKart.Width) //Level 3
                    _oppsamledeJuvelerLevel3 += 1;
            }
       }
    
        //Sjekker etter svarte piksler i kollisjonskartet.
        //Spilleren kolliderer kun på svarte piksler
        public static bool Collision(Vector2 posisjon)
        {
            try
            {
                //Kollisjon med terrain i kart
                if (posisjon.X <= KollisjonsKart.Width)
                {
                    var index = (int) posisjon.Y*KollisjonsKart.Width + (int) posisjon.X;
                    if (index < 0 || index >= KollisjonsKartData.Length)
                        return true;
                    //Out of bounds  
                    if (KollisjonsKartData[index] == Color.Black)
                        return true;
                }

               //level1 barrier
                if (posisjon.X > (KollisjonsKart.Width / 3)
                    && _oppsamledeJuvelerLevel1 < _juvelerILevel1)
                {
                    return true;
                }
                //level2 barrier
                if (posisjon.X > 2 * (KollisjonsKart.Width / 3)
                    && _oppsamledeJuvelerLevel2 < _juvelerILevel2)
                {
                    return true;
                }

                //level 3 barrier
                if (posisjon.X > (KollisjonsKart.Width)
                    && _oppsamledeJuvelerLevel3 < _juvelerILevel3)
                {
                    return true;
                }

                //Kart end 
                if (posisjon.X <= 50)
                    return true;

                return posisjon.X >= KollisjonsKart.Width;
            }
            catch (Exception)
            {
                return false;
            }
        }


        /// <summary>
        /// Plasserer fiender (turrets) basert på posisjonen til røde piksler i kollisjonskartet
        /// Også nødvendig for generell kollisjon 
        /// </summary>
        /// <param name="kart1"></param>
        public void PlaceEnemies(Texture2D kart1)
        {                  
            //Sjekke kart og lagrer fargedata i et array
           var fiendeDataKart1 = new Color[kart1.Width  * kart1.Height]; //Nytt 1D array 
            kart1.GetData(fiendeDataKart1);  //Hente farger
            KollisjonsKartData = fiendeDataKart1; //Trenger arrayet til å sjekke kollisjon senere

            for (int i = 0; i < fiendeDataKart1.Length; i++)
            {
                //Sjekker etter røde piksler, "konverterer" 1D data til 2D og lagrer posisjonene i en liste
                if (fiendeDataKart1[i] == Color.Red)
                {
                    float x = i%kart1.Width;
                    float y = (float) Math.Floor((double) (i/kart1.Width));

                    var posisjon = new Vector2(x,y);   
                    _fiendeListe.Add(new Fiende(posisjon));
                        
                }              
            }
        }


        /// <summary>
        /// Plasserer juveler basert på posisjonen til grønne, magenta og blå piksler i kollisjonskartet
        /// Lagrer Farge-verdi i stedet for JuvelType fordi fargen brukes i Draw()
        /// </summary>
        /// <param name="kart1"></param>
        /// <param name="kart2"></param>
        public void PlaceJewels(Texture2D kart1)
        {
            //Sjekke kart 1 og lagrer fargedata i et array
           var juvelDataKart1 = new Color[kart1.Width * kart1.Height]; //Nytt array 
           kart1.GetData(juvelDataKart1);  //Hente farger (Magenta, Green, Blue)

            for (int i = 0; i < juvelDataKart1.Length; i++)
            {
                //vektor2 x,y-posisjon basert på plassering i et lineært array med en angitt størrelse på width.
                var posisjon = new Vector2(i % kart1.Width,
                       (int)Math.Floor((double)(i / kart1.Width)));

                var color = juvelDataKart1[i]; 

                //Sjekker etter Grønne piksler, lager Juvel-objekt med lysegrønn farge og lagrer i liste
                if (color == Color.Green)
                    _juvelListe.Add(new Juvel(posisjon, Color.LightGreen));
                //Sjekker etter Magenta piksler, lager Juvel-objekt med magenta farge og lagrer i liste
                if (color == Color.Magenta)
                    _juvelListe.Add(new Juvel(posisjon, Color.Magenta));
                //Sjekker etter blå piksler, lager Juvel-objekt med blå farge og lagrer i liste (som rød..)
                if (color == Color.Blue)
                    _juvelListe.Add(new Juvel(posisjon, Color.Red));
            }

            //Lager en backup for når ReInit() kjøres
            foreach (Juvel juvel in _juvelListe)
            {
                _juvelListe2.Add(juvel);
            }
        }

        /// <summary>
        /// CountJewels
        /// Teller hvor mange juveler som er i hver Level
        /// </summary>
        /// <param name="list"></param>
        protected internal void CountJewels(List<Juvel> list )
        {
            foreach (Juvel j in list)
            {
                //level1
                if (j.Posisjon.X < (KollisjonsKart.Width / 3))
                    _juvelerILevel1 += 1;
                
                //level2
                if ((j.Posisjon.X > (KollisjonsKart.Width / 3)
                    && j.Posisjon.X < 2 * (KollisjonsKart.Width / 3)))
                    _juvelerILevel2 += 1;

                //Level3
                if ((j.Posisjon.X > 2 * (KollisjonsKart.Width / 3))
                    && j.Posisjon.X < KollisjonsKart.Width)
                    _juvelerILevel3 += 1;
            }
        }

        /// <summary>
        /// GetJuvelSum
        /// Teller antall juveler som er igjen i hver Level
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public int GetJuvelSum(Level level)
        {
            switch (level)
            {

                case Level.One:
                    return _juvelerILevel1 - _oppsamledeJuvelerLevel1;
                case Level.Two:
                    return _juvelerILevel2 - _oppsamledeJuvelerLevel2;
                case Level.Three:
                    return _juvelerILevel3 - _oppsamledeJuvelerLevel3;
                case Level.Boss:
                    return (_juvelerILevel1 - _oppsamledeJuvelerLevel1) + (_juvelerILevel2 - _oppsamledeJuvelerLevel2) +
                           (_juvelerILevel3 - _oppsamledeJuvelerLevel3);
            }
            return 0;

        }
    }
}
