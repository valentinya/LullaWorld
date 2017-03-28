using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LullaWorld
{

    /**
     * Thomas K. Johansen, Thea Alnæs
     * Programmering 3 prosjekt
     * 30.05.2014
     */

    /**
     * Klasse for bossen.
     * Spawner når brukeren har tatt alle juvelene i alle kart
     * (sjekkes i InitGame.cs og CurrentLevel settes til Level.Boss)
     *  
     * Tar seg av: Tegne, Oppdatere, skyte laser på spilleren
     * Trenger ikke være component(?), så vi lagde vårt eget interface
     */
    public class Boss : IMyDrawable 
    {


        protected internal  Vector2 Posisjon { get; set; } //static for at Fiende skal få tak i den
        protected internal Color Farge { get; set; }
        protected internal int Helse { get; set; }
        protected internal bool IsDead { get; set; }

        private Texture2D _bossTexture2D;
        private Texture2D _texture2DLaser;
        private readonly List<TurretLaser> _bossLaserList; //Holder på laserstrålene
        private int _skyteTimer;
        private float _rotasjonBossGun;

        public Boss()
        {
            _bossLaserList=new List<TurretLaser>();
            Posisjon = new Vector2(1200, 300);
            Farge = Color.White;
            Helse = 100;
        }



        /// <summary>
        /// LOAD CONTENT
        /// </summary>
        /// <param name="game"></param>
        public void LoadContent(Game game)
        {
            _bossTexture2D = game.Content.Load<Texture2D>("boss");
            _texture2DLaser = InitGame.FiendeLaserTexture2D;
        }


        /// <summary>
        /// UPDATE
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            if (Helse <= 0) { 
                IsDead = true;
            }

            var spiller = InitGame.Spilleren;
            //Først en Lerp sånn at han følger etter spilleren
            Posisjon = Vector2.Lerp(Posisjon, spiller.Posisjon, 0.01f);
            //For at bevegelsen ikke skal være lineær
            Posisjon = new Vector2(Posisjon.X + 
                (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds) 
                * 20f, Posisjon.Y);

            //Avstand mellom boss og spiller 
            var bossToPlayerDistance = Posisjon - spiller.Posisjon;

            //Skyter bare om spilleren er nærme nok
            if ((Math.Abs(bossToPlayerDistance.X) < 1450 && !(Math.Abs(bossToPlayerDistance.X) < 150) )
                && (Math.Abs(bossToPlayerDistance.Y) < 1450) && !(Math.Abs(bossToPlayerDistance.Y) < 150 ) )
            {

                //Plusser på skyteTimer og roterer laserstrålen
                    _skyteTimer += gameTime.ElapsedGameTime.Milliseconds;
                    _rotasjonBossGun = (float)Math.Atan2((spiller.Posisjon.Y - Posisjon.Y)
                        - _bossTexture2D.Height / 2, (spiller.Posisjon.X - Posisjon.X)
                        - _bossTexture2D.Width / 2);

                    if (_skyteTimer >= 300)
                    {
                        _skyteTimer = 0; //resette timeren
                        var laserPartikkelBoss = new TurretLaser(
                            new Vector2(Posisjon.X + _bossTexture2D.Width / 2, Posisjon.Y + _bossTexture2D.Height / 2),
                            //Rotasjonspunkt
                            _rotasjonBossGun, //Rotasjonensvinkelen
                            5, // hastighet
                            1000 // Hvor lenge den skal "leve" (i ms)
                            );

                        _bossLaserList.Add(laserPartikkelBoss);
                    }
               }

            //Skyter laserstråler
            ShootLaserBeam(gameTime, spiller);     
        }


      /// <summary>
      /// DRAW
      /// </summary>
      /// <param name="spriteBatch"></param>
        public void Draw(SpriteBatch spriteBatch)
        {
            
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
                     DepthStencilState.Default, RasterizerState.CullNone, null, InitGame.Camera.GetTransformasjon());
            spriteBatch.Draw(
                _bossTexture2D, 
                Posisjon, 
                Color.White);

            spriteBatch.End();

            foreach (var laserBeam in _bossLaserList) //tegner laserskuddene i lista
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
                             DepthStencilState.Default, RasterizerState.CullNone, null, InitGame.Camera.GetTransformasjon());
            
                spriteBatch.Draw(
                    _texture2DLaser,
                     laserBeam.Posisjon,
                     null,
                     Color.White,
                     laserBeam.TextureRotasjon,
                     new Vector2(
                          _texture2DLaser.Width / 2, 
                          _texture2DLaser.Height / 2),
                          1.0f, SpriteEffects.None, 1.0f);
                spriteBatch.End();
            }
        }


        /// <summary>
        /// Skyter på spilleren
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="spiller"></param>
        private void ShootLaserBeam(GameTime gameTime, Spiller spiller)
        {
            if (_bossLaserList.Count <= 0 || _bossLaserList.Equals(null)) return;

            for (int i = 0; i < _bossLaserList.Count; i++)
            {

                _bossLaserList[i].GammelPosisjon = _bossLaserList[i].Posisjon;
                _bossLaserList[i].Posisjon += _bossLaserList[i].Retning * _bossLaserList[i].Hastighet;

                //Holde styr på hvor lenge den har "levd" sånn at vi kan fjerne den når den overgår en viss grense
                _bossLaserList[i].TotalLivsTid += gameTime.ElapsedGameTime.Milliseconds;

                if (_bossLaserList[i].Posisjon.Y <= 5) //Fjerne når den går for høyt
                {
                    _bossLaserList.RemoveAt(i); 
                    break;
                }

                //Avstand mellom spiller og laserstråle
                var laserBeamToPlayerDistance = _bossLaserList[i].Posisjon - spiller.Posisjon;
                //Fjerner partikkel om den overskrider livstiden sin
                if (_bossLaserList[i].TotalLivsTid > _bossLaserList[i].LivsTid)
                    _bossLaserList.RemoveAt(i);

                //Gjør skade om den treffer, og fjerner particle
                if (laserBeamToPlayerDistance.LengthSquared() < 900)
                {

                    spiller.Farge = Color.Red; // bytter farge
                    spiller.Helse -= 10;
                    _bossLaserList.RemoveAt(i); //fjerner partikkel
                }

                try //try/catch tilfelle out of bound.
                {
                    //samme kollisjonsmetode som brukes for spiller
                    var index = (int)_bossLaserList[i].Posisjon.Y * Verden.KollisjonsKart.Width +
                                (int)_bossLaserList[i].Posisjon.X;

                    if (Verden.KollisjonsKartData[index] == Color.Black) // Svartfarge indikerer bakke og kollisjon
                        _bossLaserList.RemoveAt(i); //fjerne partikkel
                }
                catch
                {
                    //out of bound exception
                }
            }
        }
    }
}
