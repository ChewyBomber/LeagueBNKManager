using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BNKManager
{
    public class LoLSoundBankManager
    {
        public List<LoLSoundBank> banksList;

        public LoLSoundBankManager(List<LoLSoundBank> banksList)
        {
            this.banksList = banksList;
        }

        public WPKSoundBank GetWPKBank()
        {
            return (WPKSoundBank)this.banksList.Find(x => x is WPKSoundBank);
        }

        public BankSection GetSection(string sectionName)
        {
            BankSection found = null;
            foreach (LoLSoundBank bank in this.banksList)
            {
                if (bank is WwiseBank)
                {
                    found = ((WwiseBank)bank).GetSection(sectionName);
                    if (found != null)
                    {
                        return found;
                    }
                }

            }
            return found;
        }

        public LoLSoundBank GetBank(uint bankId)
        {
            return this.banksList.Find(x => (x is WwiseBank) && (((WwiseBank)x).GetID() == bankId));
        }

        public byte[] GetFileData(uint fileID)
        {
            DATASection data = (DATASection)this.GetSection("DATA");
            if (data != null)
            {
                for (int i = 0; i < data.wemFiles.Count; i++)
                {
                    if (data.wemFiles[i].info.ID == fileID)
                    {
                        return data.wemFiles[i].data;
                    }
                }
            }
            //File not found yet
            WPKSoundBank wpkBank = this.GetWPKBank();
            if (wpkBank != null)
            {
                foreach (WPKSoundBank.WPKSoundBankWEMFile wemFile in wpkBank.wemFiles)
                {
                    if (wemFile.ID == fileID)
                    {
                        return wemFile.data;
                    }
                }
            }
            //File not found :(
            return null;
        }

        public List<WEMFile> GetAudioFiles()
        {
            List<WEMFile> newList = new List<WEMFile>();
            DIDXSection dataIndex = (DIDXSection)this.GetSection("DIDX");
            DATASection data = (DATASection)this.GetSection("DATA");
            HIRCSection hirc = (HIRCSection)this.GetSection("HIRC");
            // List all audio files
            if (dataIndex != null && data != null)
            {
                foreach (DATASection.WEMFile wem in data.wemFiles)
                {
                    newList.Add(new WEMFile(wem.info.ID, 0, GetAudioFileSeconds(ref wem.data)));
                }
            }
            WPKSoundBank wpkBank = this.GetWPKBank();
            if (wpkBank != null)
            {
                foreach (WPKSoundBank.WPKSoundBankWEMFile wem in wpkBank.wemFiles)
                {
                    newList.Add(new WEMFile(wem.ID, 0, GetAudioFileSeconds(ref wem.data)));
                }
            }
            // Eventually find associated events
            if (hirc != null)
            {
                foreach (WEMFile wem in newList)
                {
                    SoundSFXVoiceWwiseObject foundSfx = (SoundSFXVoiceWwiseObject)hirc.objects.Find(x => x.objectType == WwiseObjectType.Sound_SFX__Sound_Voice && ((SoundSFXVoiceWwiseObject)x).audioFileID == wem.ID);
                    if (foundSfx != null)
                    {
                        EventActionWwiseObject foundEventAction = (EventActionWwiseObject)hirc.objects.Find(x => x.objectType == WwiseObjectType.Event_Action && ((EventActionWwiseObject)x).gameObject == foundSfx.ID);
                        if (foundEventAction != null)
                        {
                            EventWwiseObject foundEvent = (EventWwiseObject)hirc.objects.Find(x => x.objectType == WwiseObjectType.Event && ((EventWwiseObject)x).eventActionList.Contains(foundEventAction.ID));
                            if (foundEvent != null)
                            {
                                wem.eventID = foundEvent.ID;
                            }

                        }
                    }

                }
            }
            return newList;
        }

        public static int GetAudioFileSeconds(ref byte[] data)
        {
            // Approximated
            int result = 0;
            using (BinaryReader br = new BinaryReader(new MemoryStream(data)))
            {
                br.BaseStream.Seek(4, SeekOrigin.Begin);
                float fileSize = br.ReadUInt32();
                br.BaseStream.Seek(28, SeekOrigin.Begin);
                float bytesPerSecond = br.ReadUInt32();
                float seconds = fileSize / bytesPerSecond;
                result = (int)Math.Round(seconds);
            }
            return result;
        }

        public void EditAudioFile(uint fileID, byte[] newData)
        {
            DIDXSection dataIndex = (DIDXSection)this.GetSection("DIDX");
            DATASection data = (DATASection)this.GetSection("DATA");
            if (dataIndex != null && data != null)
            {
                uint lastOffset = 0;
                for (int i = 0; i < dataIndex.embeddedWEMFiles.Count; i++)
                {
                    dataIndex.embeddedWEMFiles[i].offset = lastOffset;
                    if (dataIndex.embeddedWEMFiles[i].ID == fileID)
                    {
                        dataIndex.embeddedWEMFiles[i].length = (uint)newData.Length;
                    }
                    lastOffset += dataIndex.embeddedWEMFiles[i].length + 10;
                }

                for (int i = 0; i < data.wemFiles.Count; i++)
                {
                    if (data.wemFiles[i].info.ID == fileID)
                    {
                        data.wemFiles[i].data = newData;
                    }

                }
                HIRCSection hirc = (HIRCSection)this.GetSection("HIRC");
                if (hirc != null)
                {
                    foreach (WwiseObject obj in hirc.objects)
                    {
                        if (obj.objectType == WwiseObjectType.Sound_SFX__Sound_Voice)
                        {
                            SoundSFXVoiceWwiseObject soundObj = (SoundSFXVoiceWwiseObject)obj;
                            DIDXSection.EmbeddedWEM gotEmbedded = dataIndex.GetEmbeddedWEM(soundObj.audioFileID);
                            if (gotEmbedded != null)
                            {
                                soundObj.fileOffset = (uint)data.dataStartOffset + gotEmbedded.offset;
                                soundObj.fileLength = gotEmbedded.length;
                            }
                        }

                    }
                }
            }
            WPKSoundBank wpkBank = this.GetWPKBank();
            if (wpkBank != null)
            {
                foreach (WPKSoundBank.WPKSoundBankWEMFile wemFile in wpkBank.wemFiles)
                {
                    if (wemFile.ID == fileID)
                    {
                        wemFile.data = newData;
                    }
                }
            }
        }

        public void Save()
        {
            foreach (LoLSoundBank bank in this.banksList)
            {
                bank.Save();
            }
        }

        public class WEMFile
        {
            public uint ID;
            public uint eventID;
            public int seconds;

            public WEMFile(uint ID, uint eventID, int seconds)
            {
                this.eventID = eventID;
                this.ID = ID;
                this.seconds = seconds;
            }
        }
    }
}
