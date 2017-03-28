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

    internal class TurretLaser
    {
        public Vector2 Posisjon;
        public Vector2 GammelPosisjon;
        public Vector2 Retning;
        public float TextureRotasjon;
        public float Hastighet;
        public int LivsTid;
        public int TotalLivsTid;


        public TurretLaser(Vector2 posisjon, float rotasjon, float hastighet, int livsTid)
        {
            Posisjon = posisjon;
            Retning = new Vector2((float) Math.Cos(rotasjon), (float) Math.Sin(rotasjon))*5f;
            TextureRotasjon = rotasjon;
            Hastighet = hastighet;
            LivsTid = livsTid;
            TotalLivsTid = 0;
        }

 
    }
}
