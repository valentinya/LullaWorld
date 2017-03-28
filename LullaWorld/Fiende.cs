using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace LullaWorld
{

    /**
     * Thomas K. Johansen, Thea Alnæs
     * Programmering 3 prosjekt
     * 30.05.2014
     */


    /**
     * Fiende.cs
     * Tar seg av tegning og oppdatering av turrets, inkludert kollisjon mellom laser og spiller/boss
     * Vi lagde IMyDrawable etter at vi lagde Fiende, og Boss.cs bruker samme textures, derfor har vi beholdt at textures lastes i InitGame. 
     * Vi kunne ha gjort om TurretLaser.cs slik at den også brukte IMyDrawable, men det var samtidig behagelig at den ikke tok seg av det.
     * Boss var ikke planlagt, bare noe vi bestemte oss for på sparket da vi bestemte oss for å bare bruke ett av kartene vi hadde laget.
     * Så det var ikke lagt opp til et hierarki der, og Fiende.cs ble bygd opp med den tanken at den eneste fienden vi hadde var turrets.
     */

    class Fiende : IMyDrawable
    {
        protected internal bool IsShooting { get; set; }
        protected internal Vector2 Posisjon;
       
        private readonly List<TurretLaser> _laserBeamList;
        private Vector2 _gunPosisjon;
        private readonly Texture2D _texture2D;
        private readonly Texture2D _texture2DLaser;
       
        // Deler opp texture , som er 2 bilder samlet i et og definerer gyldig område for tegningen.
        private readonly Rectangle _gunRectangle = new Rectangle(32, 0, 32, 32);
        private readonly Rectangle _hullRectangle = new Rectangle(0, 0, 32, 32);

        //Setter rotasjonspunkter for hver del
        private readonly Vector2 _rotasjonsPunktGun = new Vector2(18, 16);
        private readonly Vector2 _rotasjonsPunktHull = new Vector2(16, 16);

        private const int SkytePause = 400;
        private int _skyteTimer;
        private readonly Spiller _spiller;
        private readonly Boss _boss;
        private Matrix _matrix;
        private float _rotasjonGun;

        public Fiende(Vector2 posisjon)
        {
            _skyteTimer = 0;
            _laserBeamList = new List<TurretLaser>();
            _rotasjonGun = 0f;
            _gunPosisjon = posisjon + new Vector2(0, -2);
            Posisjon = posisjon;
            IsShooting = false;
            _texture2D = InitGame.FiendeGunTexture2D; //Texture fra InitGame
            _texture2DLaser = InitGame.FiendeLaserTexture2D; //Texture fra InitGame
            _spiller =  InitGame.Spilleren; 
            _boss = InitGame.TheBoss;
        }

      /// <summary>
      /// DRAW
      /// Tegner fiende OG laserstråler
      /// </summary>
      /// <param name="spriteBatch"></param>
        public void Draw(SpriteBatch spriteBatch)
        {
            _matrix = InitGame.Camera.GetTransformasjon();

            //Tegner selve turreten
               spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, 
                    DepthStencilState.Default, RasterizerState.CullNone, null, _matrix);
                spriteBatch.Draw(
                    _texture2D,
                    Posisjon, 
                    _hullRectangle, 
                    Color.White, 
                    0, 
                    _rotasjonsPunktHull,
                    4f, SpriteEffects.None,  1); //Tegner basen til turret

                spriteBatch.Draw(
                    _texture2D, 
                    _gunPosisjon,
                    _gunRectangle,
                    Color.White, 
                    _rotasjonGun,
                    _rotasjonsPunktGun,
                    4f, SpriteEffects.None, 1); //Tegner løpet til turret som roterer om punktet til basen.
            spriteBatch.End();

            
            foreach (var laserBeam in _laserBeamList) //tegner laserskuddene i lista
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
                             DepthStencilState.Default, RasterizerState.CullNone, null, _matrix);
                spriteBatch.Draw(
                    _texture2DLaser,
                     laserBeam.Posisjon,
                     null,
                     Color.White,
                     laserBeam.TextureRotasjon,
                     new Vector2(
                          _texture2DLaser.Width / 2,
                          _texture2DLaser.Height / 2),
                          1.0f, SpriteEffects.None, 1.0f); //Tegner laserbeams
                spriteBatch.End();    
            }  
        }


        /// <summary>
        /// UPDATE
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime) 
        {
            var rand = new Random();
            //For å få fienden til å skyte èn og èn partikkel med jevne mellomrom (minst 400ms)
            _skyteTimer += gameTime.ElapsedGameTime.Milliseconds;

            if (IsShooting)
            {
                //For å få fienden til å skyte èn og èn partikkel med jevne mellomrom (minst 400ms)
                _skyteTimer += gameTime.ElapsedGameTime.Milliseconds;
              
                //Endrer rotasjonen til fienden slik at den følger posisjonen til spilleren
                _rotasjonGun = (float)Math.Atan2(_spiller.Posisjon.Y - Posisjon.Y, _spiller.Posisjon.X - Posisjon.X);

                // Random tidsintervall mellom skuddene
                if (_skyteTimer * rand.NextDouble()*2 > SkytePause * rand.NextDouble()*5.0f)
                {
                    _skyteTimer = 0; //resette timeren
                    var laserPartikkel = new TurretLaser(
                         new Vector2(_gunPosisjon.X + 0.5f, _gunPosisjon.Y), //Rotasjonspunkt
                         _rotasjonGun, //Rotasjonensvinkelen til fienden
                         5, // hastighet
                         700 // Hvor lenge den skal "leve" (i ms)
                         );

                    _laserBeamList.Add(laserPartikkel);
                }
            }

            ShootLaserBeam(gameTime);
        }


        /// <summary>
        /// ShootLaserBeam()
        /// ... skyter laser fra fiende.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="spiller"></param>
        private void ShootLaserBeam(GameTime gameTime)
        {
            if (_laserBeamList.Count <= 0 || _laserBeamList.Equals(null)) return;
            try //try/catch tilfelle out of bound.
            {
                for (int i = 0; i < _laserBeamList.Count; i++)
                {

                    _laserBeamList[i].GammelPosisjon = _laserBeamList[i].Posisjon;
                    _laserBeamList[i].Posisjon += _laserBeamList[i].Retning * _laserBeamList[i].Hastighet;
                    //Holde styr på hvor lenge den har "levd" sånn at vi kan fjerne den når den overgår en viss grense
                    _laserBeamList[i].TotalLivsTid += gameTime.ElapsedGameTime.Milliseconds;

                    if (_laserBeamList[i].Posisjon.Y <= 5) //Fjernes om den går for høyt
                    {
                        _laserBeamList.RemoveAt(i);
                        break;
                    }
                    //Avstand mellom spiller og laserstråle
                    Vector2 laserBeamToPlayerDistance = _laserBeamList[i].Posisjon - _spiller.Posisjon;

                    //Avstand mellom boss og laserstråle
                    Vector2 laserBeamToBossDistance = _laserBeamList[i].Posisjon - _boss.Posisjon;

                    //Fjerner partikkel om den overskrider livstiden sin
                    if (_laserBeamList[i].TotalLivsTid > _laserBeamList[i].LivsTid)
                        _laserBeamList.RemoveAt(i);

                    //Gjør skade om den treffer, og fjerner particle
                    if (laserBeamToPlayerDistance.LengthSquared() < 900)
                    {
                        _spiller.Helse -= 2;
                        _spiller.Farge = Color.Red; // bytter farge
                        _laserBeamList.RemoveAt(i); //fjerner partikkel
                    }

                    //Skyte på boss
                    if (InitGame.CurrentLevel == Level.Boss)
                    {
                        //Gjør skade om den treffer, og fjerner particle
                        if (laserBeamToBossDistance.LengthSquared() < 900)
                        {
                            _boss.Helse -= 20;
                            _boss.Farge = Color.Red; // bytter farge
                            _laserBeamList.RemoveAt(i); //fjerner partikkel
                        }
                    }

                    //samme kollisjonsmetode mot terrain som brukes for spiller
                    var index = (int)_laserBeamList[i].Posisjon.Y * Verden.KollisjonsKart.Width +
                                (int)_laserBeamList[i].Posisjon.X;

                    if (Verden.KollisjonsKartData[index] == Color.Black) // Svartfarge indikerer bakke og kollisjon
                        _laserBeamList.RemoveAt(i); //fjerne partikkel
                 }
             }
            catch
            {
                //out of bound exception
            }
        }


        //Både boss og Fiende bruker samme content, så det loades i InitGame
        public void LoadContent(Game game)
        {
            throw new NotImplementedException();
        }

    }

}
