
using System;
using System.Collections.Generic;
using System.Windows.Forms;


namespace LullaWorld
{

    /**
     * Thomas K. Johansen, Thea Alnæs
     * Programmering 3 prosjekt
     * 30.05.2014
     */

    public static class Program
    {
        [STAThread]
        static void Main()
        {

            using (InitGame game = new InitGame())
            {
                game.Run();
            }
        }
    }

}
