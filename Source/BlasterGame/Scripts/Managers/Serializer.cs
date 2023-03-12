using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System;

namespace Manager
{
    public class Serializer : MonoBehaviour
    {
        public void SaveProfile(PlayerProfile pl)
        {
            ProfileSave s = new ProfileSave();
            s.lastCharId = pl.charId;
            s.profileName = pl.playerName;
            s.lastMW = pl.mainWeapon;
            s.lastSW = pl.secWeapon;
            s.px = pl.px;
            s.py = pl.py;
            s.pz = pl.pz;
            s.rx = pl.prx;
            s.ry = pl.pry;
            s.rz = pl.prz;           
            s.mwMods.AddRange(pl.mainWeaponMods);
            s.swMods.AddRange(pl.secWeaponMods);   

            string saveLocation = SaveLocation();
            saveLocation += "/Data";

            if (!Directory.Exists(saveLocation))
                Directory.CreateDirectory(saveLocation);

            saveLocation += "/" + s.profileName;

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(saveLocation, FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, s);
            stream.Close();

            Debug.Log("Profile saved at " + saveLocation);
        }

        public ProfileSave TryToLoadProfile(string pfName)
        {
            ProfileSave r = null;
            string saveFile = SaveLocation();
            saveFile += "/Data";

            if (!Directory.Exists(saveFile))
                return null;

            saveFile += "/" + pfName;

            if (File.Exists(saveFile))
            {
                IFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(saveFile, FileMode.Open);

                ProfileSave save = (ProfileSave)formatter.Deserialize(stream);
                r = save;
                stream.Close();
            }

            return r;
        }

        public List<PlayerProfile> GetProfiles()
        {
            List<PlayerProfile> r = new List<PlayerProfile>();
            List<String> fnames = LoadProfiles();

            for (int i = 0; i < fnames.Count; i++)
            {
                ProfileSave s = TryToLoadProfile(fnames[i]);

                if (s == null)
                    continue;

                PlayerProfile p = new PlayerProfile();
                p.playerName = s.profileName;
                p.charId = s.lastCharId;
                p.mainWeapon = s.lastMW;
                p.secWeapon = s.lastSW;
                p.px = s.px;
                p.py = s.py;
                p.pz = s.pz;
                p.prx = s.rx;
                p.pry = s.ry;
                p.prz = s.rz;
                p.mainWeaponMods.AddRange(s.mwMods);
                p.secWeaponMods.AddRange(s.swMods);
                r.Add(p);
            }

            Debug.Log("Loaded " + r.Count + " profiles");

            return r;
        }

        public List<String> LoadProfiles()
        {
            List<String> r = new List<string>();

            string saveLocation = SaveLocation();
            saveLocation += "/Data";

            if (!Directory.Exists(saveLocation))
                return r;

            DirectoryInfo dirInfo = new DirectoryInfo(saveLocation);
            FileInfo[] fileInfo = dirInfo.GetFiles();

            foreach (FileInfo f in fileInfo)
            {
                string[] readName = f.Name.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
       
                if (readName.Length == 1)
                {
                    r.Add(f.Name);
                }         
            }

            return r;
        }

        static string SaveLocation()
        {
            string saveLocation = Application.streamingAssetsPath;

            if (!Directory.Exists(saveLocation))
            {
                Directory.CreateDirectory(saveLocation);
            }

            return saveLocation;

        }

        static public Serializer singleton;
        void Awake()
        {
            singleton = this;
        }
    }

    [System.Serializable]
    public class ProfileSave
    {
        public string profileName;
        public string lastCharId;
        public string lastMW;
        public string lastSW;
        public List<string> mwMods = new List<string>();
        public List<string> swMods = new List<string>();
        public int progression;
        public float px;
        public float py;
        public float pz;
        public float rx;
        public float ry;
        public float rz;
    }
}
