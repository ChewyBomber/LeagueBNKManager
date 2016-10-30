using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNKManager
{
    class Program
    {
        static void Main(string[] args)
        {
            WwiseBank myBank = new WwiseBank(@"C:\Wooxy\extract\lol_game_client\DATA\Sounds\Wwise\SFX\Characters\Azir\Skins\Base\Azir_Base_SFX_audio.bnk");
            myBank.Save(@"C:\Wooxy\extract\lol_game_client\DATA\Sounds\Wwise\SFX\Characters\Azir\Skins\Base\Azir_Base_SFX_audio2.bnk");
        }
    }
}
