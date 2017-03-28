using System;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LullaWorld
{
    /**
    * Thomas K. Johansen, Thea Alnæs
    * Programmering 3 prosjekt
    * 30.05.2014
    */


    internal struct InnloggetBruker
    {
        internal string Navn;
        internal int Id;
        internal int HighScore;
    }

    /**
     * Spiller.cs
     * Håndterer brukerinput (for skipet), tegning, bevegelse, oppdatering, kollisjonspunkter på skip
     * Bruker IMyDrawable (vi er så kreative med navn...) fordi vi synes vi fikk bedre kontroll over 
     * hva som skjer når ved å ikke bruke components (eller innebygd XNA interfaces).
     */
    public class Spiller : IMyDrawable
    {
     
        protected internal Vector2 Posisjon { get; set; }
        protected internal Vector2 Hastighet { get; set; }
        protected internal Color Farge { get; set; }
        protected internal double Helse { get; set; }
        protected internal Int32 Score { get; set; }
        protected internal bool IsDead { get; set; }

        private Texture2D _texture2D;
        private Texture2D _laserTexture;
        private Color _jetColor = Color.White;

        private int _fargeTimer;
        private float _rotasjon;
        private float _spillerAksellerasjon;

        private SoundEffect _jetLyd;
        private SoundEffectInstance _jetLydInstance;

        private Vector2 _rotasjonsPunkt;
        private Vector2 _gammelPosisjon;
        private Vector2 _høyreVinge;
        private Vector2 _venstreVinge;
        private Vector2 _haleVenstre;
        private Vector2 _haleHøyre;
        private Vector2 _snuteHøyre;
        private Vector2 _snuteVenstre;
      

        public Spiller(Vector2 posisjon)
        {
          
            Posisjon = posisjon;
            _rotasjon = 0;
            _gammelPosisjon = Posisjon;
            Helse = 100;
            Score = 0;
            IsDead = false;
           Farge = Color.LightPink;
     
        }

        /// <summary>
        /// LOAD CONTENT
        /// </summary>
        /// <param name="game"></param>
        public void LoadContent(Game game)
        {
            _texture2D = game.Content.Load<Texture2D>("ship.png");
            _laserTexture = game.Content.Load<Texture2D>("jetBeam.png");
            _jetLyd = game.Content.Load<SoundEffect>("_accel");
            _jetLydInstance = _jetLyd.CreateInstance();
            _rotasjonsPunkt = new Vector2(_texture2D.Width / 2, _texture2D.Height / 2);
        }


        /// <summary>
        /// DRAW
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void Draw(SpriteBatch spriteBatch)
        {
          if(!_jetLydInstance.IsLooped) //stopper lyden om jetmotoren er inaktiv.
              _jetLydInstance.Stop();

          spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.AnisotropicClamp, 
              DepthStencilState.Default, RasterizerState.CullNone, null, InitGame.Camera.GetTransformasjon());

            spriteBatch.Draw( //Tegne spiller
                _texture2D, 
                Posisjon, 
                null, 
                Farge, 
                _rotasjon, 
                _rotasjonsPunkt, 
                new Vector2(0.3f, 0.3f), 
                SpriteEffects.None, 0f
                );
            spriteBatch.End(); //End tegne spiller

            if (Keyboard.GetState().IsKeyDown(Keys.W)) 
            {
                _jetLydInstance.Play();
                _jetLydInstance.IsLooped = true;

                // motorflamme på alle tre motorer ved fremdrift
                // oppretter xy komponenter av rotasjon og skalerer opp og legger til spillerposisjon,
                // rotasjonspunktet er forskjøvet med verdier som ser okay ut visuelt.
                 spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.AnisotropicClamp, 
                     DepthStencilState.Default, RasterizerState.CullNone, null, InitGame.Camera.GetTransformasjon());
               
                spriteBatch.Draw( //Jet 1
                     _laserTexture,
                     new Vector2((float)Math.Cos(_rotasjon) * -40f + Posisjon.X, 
                     (float)Math.Sin(_rotasjon) * -40f + (Posisjon.Y)), 
                     null, _jetColor, _rotasjon, 
                     new Vector2(_rotasjonsPunkt.X-130f, _rotasjonsPunkt.Y-125f),
                     new Vector2(0.9f, 0.9f), 
                     SpriteEffects.None, 0f
                     );

                 spriteBatch.Draw( //Jet 2
                     _laserTexture, 
                     new Vector2((float)Math.Cos(_rotasjon) * -40f + Posisjon.X, 
                         (float)Math.Sin(_rotasjon) * -40f + (Posisjon.Y)),
                         null, _jetColor, _rotasjon,
                         new Vector2(_rotasjonsPunkt.X - 130f, _rotasjonsPunkt.Y - 125f / 1.5f), 
                         new Vector2(0.9f, 0.9f), 
                         SpriteEffects.None, 0f
                         );

                 spriteBatch.Draw( //Jet 3
                     _laserTexture, 
                     new Vector2((float)Math.Cos(_rotasjon) * -40f + Posisjon.X,
                         (float)Math.Sin(_rotasjon) * -40f + (Posisjon.Y)), 
                         null, _jetColor, _rotasjon,
                         new Vector2(_rotasjonsPunkt.X - 130f, _rotasjonsPunkt.Y - 165f), 
                         new Vector2(0.9f, 0.9f),
                         SpriteEffects.None, 0f
                         );

                    spriteBatch.End(); //End tegne jets
            }
            else _jetLydInstance.IsLooped = false; //slår av loop på lyden.

            if (Keyboard.GetState().IsKeyDown(Keys.A)) // motorflamme sprite høyre
            {
                _jetLydInstance.Play();
                _jetLydInstance.IsLooped = true;

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.AnisotropicClamp,
                    DepthStencilState.Default, RasterizerState.CullNone, null, InitGame.Camera.GetTransformasjon());
                spriteBatch.Draw(
                    _laserTexture,
                    new Vector2((float) Math.Cos(_rotasjon)*-40f + Posisjon.X,
                        (float) Math.Sin(_rotasjon)*-40f + (Posisjon.Y)),
                    null, _jetColor, _rotasjon,
                    new Vector2(_rotasjonsPunkt.X - 130f,
                        _rotasjonsPunkt.Y - 165f),
                    new Vector2(0.9f, 0.9f),
                    SpriteEffects.None, 0f
                    );
                spriteBatch.End();
            }

            if (Keyboard.GetState().IsKeyDown(Keys.D)) // motorflamme sprite venstre
            {
                _jetLydInstance.Play();
                _jetLydInstance.IsLooped = true;
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.AnisotropicClamp,
                    DepthStencilState.Default, RasterizerState.CullNone, null, InitGame.Camera.GetTransformasjon());
                spriteBatch.Draw(
                    _laserTexture, 
                    new Vector2((float)Math.Cos(_rotasjon) * -40f + Posisjon.X, 
                        (float)Math.Sin(_rotasjon) * -40f + (Posisjon.Y)), 
                        null, _jetColor, _rotasjon,
                        new Vector2(_rotasjonsPunkt.X - 130f, _rotasjonsPunkt.Y - 125f / 1.5f),
                        new Vector2(0.9f, 0.9f),
                        SpriteEffects.None, 0f
                        );  
                spriteBatch.End(); 
            }          
        }

   
        /// <summary>
        /// UPDATE
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        { 
            _gammelPosisjon = Posisjon;       //kontrollverdier
            _fargeTimer += gameTime.ElapsedGameTime.Milliseconds;  // så ikke blinkingen av farge skjer umiddelbart og for raskt        
           
           //lager x og y komponenter av rotasjonsvinkel og skalerer opp med spilleraksellerasjon.
           Hastighet += new Vector2((float)Math.Cos(_rotasjon), (float)Math.Sin(_rotasjon)) *_spillerAksellerasjon;  
           Hastighet += Verden.Gravitasjon * new Vector2(0, 1f); //Gravitasjon , positiv Y-akse nedover.
           //legger Hastighetsvektoren til posisjon.         
           Posisjon += Hastighet;

            //max fartsgrense for spiller
            if (Hastighet.X >= 7f || Hastighet.Y >= 7)
                Hastighet *= 0.98f;
            if (Hastighet.X <= -7f || Hastighet.Y <= -7)
                Hastighet *= 0.98f;

            if (_fargeTimer > 500 && Farge.Equals(Color.Red))
            {     
               _fargeTimer = 0; //nullstiller timer, så fargen kan returnere til normal ved et definert tidspunkt.
               Farge = Color.Pink;               
            }

            if (Helse <= 0) // om helse er under 0
                IsDead = true;  //game over
   
    
           if (Posisjon.Y <= 5) //grense på "taket" til himmelen
               Hastighet *= -1;
     
           //Nullstille rotasjon slik at rotasjon alltid er mellom 0 og 2Pi
            if (_rotasjon == MathHelper.TwoPi) _rotasjon = 0;
            if (_rotasjon == MathHelper.TwoPi * -1) _rotasjon = 0;

            CheckInput(gameTime); //Sjekker keyboardinput for w-a-d og turbo
            CheckCollision(); //Sjekker kollisjon mellom skip og terrain

           
        }

        /// <summary>
        /// Sjekker kollisjon, setter fargen på skipet til rødt og spilleren mister helse (eller dør) 
        /// </summary>
        private void CheckCollision()
        {
            //Kollisjonspunkter på skipet ift posisjon
            _høyreVinge = new Vector2(Posisjon.X + 40, Posisjon.Y);
            _venstreVinge = new Vector2(Posisjon.X - 40, Posisjon.Y);
            _haleVenstre = new Vector2(Posisjon.X - 40, Posisjon.Y + 20);
            _haleHøyre = new Vector2(Posisjon.X + 40, Posisjon.Y + 20);
            _snuteHøyre = new Vector2(Posisjon.X + 40, Posisjon.Y - 20);
            _snuteVenstre = new Vector2(Posisjon.X - 40, Posisjon.Y - 20);

            //Kollidere med terrain (det som er i KollisjonsKart)
            if (Verden.Collision(_venstreVinge) || Verden.Collision(_høyreVinge) ||
                Verden.Collision(_haleVenstre) || Verden.Collision(_haleHøyre) ||
                Verden.Collision(_snuteVenstre) || Verden.Collision(_snuteHøyre))
            {
                Posisjon = _gammelPosisjon;

                //Liten if/else fordi man kunne henge seg opp i bakken når man "lander"
                if (Hastighet.Y > 0.1 || Hastighet.Y < 0.1) 
                    Hastighet *= -0.3f;
                else Hastighet = Hastighet;

                if (Hastighet.Y >= 2 || Hastighet.Y <= -2)  //skade basert på spillerhastighet.
                {
                    Helse -= 3;
                    Farge = Color.Red;
                }

                else if (Hastighet.Y >= 4 || Hastighet.Y <= -4) //ekstra skade for høyere hastighet enn 4
                {
                    Helse -= 10;
                    Farge = Color.Red;
                }

                else if (Hastighet.Y >= 5 || Hastighet.Y <= -5) //dør om hastighet er større enn 5
                {
                    Helse = 0;
                    IsDead = true;

                }
            }
        }

        /// <summary>
        /// Sjekker brukerinput for styring med W-A-D og turbo-boost med CTRL+W
        /// </summary>
        /// <param name="gameTime"></param>
        private void CheckInput(GameTime gameTime)
        {
           
            if (Keyboard.GetState().IsKeyDown(Keys.D)) //rotere
            {
                _rotasjon += 0.05f;
                _rotasjon = _rotasjon % Verden.Sirkel;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.A)) //rotere
            {
                _rotasjon -= 0.05f;
                _rotasjon = _rotasjon % Verden.Sirkel;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.W))  //fremdrift
            {
                if (_spillerAksellerasjon < 0.06f)
                    _spillerAksellerasjon += 0.1f;
            }
            else
            {
                if (_spillerAksellerasjon > 0)          //redusere fremdrift 
                    _spillerAksellerasjon -= 0.02f;
            }

            ////////////////////////////////////////////////////////////////////
            //turbofart input   ctrl + W , gir turboFart på bekostning av helse.
            ////////////////////////////////////////////////////////////////////

            if (Keyboard.GetState().IsKeyDown(Keys.LeftControl) && Keyboard.GetState().IsKeyDown(Keys.W))
            {
                _jetColor = Color.Pink;
                if (_jetLydInstance.Pitch < 0.9f) _jetLydInstance.Pitch += 0.01f; //Pitchen øker når du holder turbo inne
                if (_jetLydInstance.Volume < 0.9f) _jetLydInstance.Volume += 0.1f; //Volum øker om du holder turbo inne
                Hastighet += new Vector2((float)Math.Cos(_rotasjon), (float)Math.Sin(_rotasjon)) * _spillerAksellerasjon * 4.0f;
                Helse -= gameTime.ElapsedGameTime.Milliseconds * 0.008f;     //Mister mer helse om du holder turbo inne i lengre tidsperioder
            }
            else
            {
                if (_jetLydInstance.Pitch >= 0.1f) _jetLydInstance.Pitch -= 0.1f;
                if (_jetLydInstance.Volume >= 0.1f) _jetLydInstance.Volume -= 0.1f;
                _jetColor = Color.White;
            }
        }        
    }
}
