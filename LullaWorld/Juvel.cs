using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LullaWorld
{
    /**
   * Thomas K. Johansen, Thea Alnæs
   * Programmering 3 prosjekt
   * 30.05.2014
   */
    internal enum JuvelType { Green = 500, Magenta = 2000, Red = 5000 }; //Holder på "verdien" av forskjellige typer juveler

    internal struct Juvel
    {
        public Vector2 Posisjon;
        public Color Farge; //Brukes til tegning og for å sjekke juveltype

        public Juvel(Vector2 posisjon, Color farge) : this()
        {
            Posisjon = posisjon;
            Farge = farge;
        }

    }
}
