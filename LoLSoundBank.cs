using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BNKManager
{
    public abstract class LoLSoundBank
    {
        public string fileLocation;
        public abstract void Save();
        public abstract void Save(string fileLocation);
        protected LoLSoundBank(string fileLocation)
        {
            this.fileLocation = fileLocation;
        }

    }
}
