using Microsoft.Xna.Framework;

/**
  * Thomas K. Johansen, Thea Alnæs
  * Programmering 3 prosjekt
  * 30.05.2014
  */

namespace LullaWorld
{
   
    /**
     * Translaterer og transformerer textures utifra spillerposisjon
     */
   public class Camera
    {
        private Matrix _transformasjon;
        private Vector2 _posisjon;
        private const float Hastighet = 0.05f;
        private readonly int _maxHøyre;

       public Camera(int maxBredde, Vector2 pos)
        {
            _posisjon = pos;
            _maxHøyre = maxBredde + 1000;
        }

        public Matrix GetTransformasjon()
        {
            _transformasjon = Matrix.CreateTranslation(new Vector3(-_posisjon.X, -_posisjon.Y*2, 0))
                * Matrix.CreateScale(new Vector3(0.5f, 0.5f, 1)) 
                * Matrix.CreateTranslation(new Vector3(800 * 0.5f, 480 * 0.5f, 0));
            return _transformasjon;
        }


        public Vector2 Posisjon
        {
            
            get { return _posisjon; }
            set
            {
                //Grenser for Y
                 if (value.Y > 360)
                     _posisjon.Y = MathHelper.Lerp(_posisjon.Y, 360, Hastighet);
                else if (value.Y < -100)
                     _posisjon.Y = MathHelper.Lerp(_posisjon.Y, -100, Hastighet);
                else
                {
                    _posisjon.Y = MathHelper.Lerp(_posisjon.Y, value.Y, Hastighet);
                }

                //Grenser for X
                if (value.X < 700)
                    _posisjon.X = MathHelper.Lerp(_posisjon.X, 700, Hastighet);
                else if (value.X > _maxHøyre)
                    _posisjon.X = MathHelper.Lerp(_posisjon.X, _maxHøyre, Hastighet);
                else
                {
                    _posisjon.X = MathHelper.Lerp(_posisjon.X, value.X, Hastighet);
                }
              
            }
        }
    }
}
