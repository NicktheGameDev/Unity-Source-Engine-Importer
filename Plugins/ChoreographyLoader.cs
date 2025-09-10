
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace uSource
{
    // Full .chr choreography loader
    public class ChoreographyLoader
    {
        public struct TrackKey { public float time; public Vector3 pos; public Quaternion rot; }
        public class Sequence { public string name; public List<TrackKey> keys = new List<TrackKey>(); }

        public static List<Sequence> LoadChr(string path)
        {
            var seqs = new List<Sequence>();
            using(var br = new BinaryReader(File.OpenRead(path)))
            {
                // .chr header parsing
                string id = new string(br.ReadChars(4));
                if(id != "CHR ")
                    return seqs;
                int version = br.ReadInt32();
                int seqCount = br.ReadInt32();
                int keyCount = br.ReadInt32();
                // Read sequence names
                for(int i=0;i<seqCount;i++)
                {
                    var seq = new Sequence();
                    int nameLen = br.ReadInt32();
                    seq.name = new string(br.ReadChars(nameLen));
                    seqs.Add(seq);
                }
                // Read keys
                for(int k=0;k<keyCount;k++)
                {
                    int seqId = br.ReadInt32();
                    float time = br.ReadSingle();
                    float px = br.ReadSingle(), py = br.ReadSingle(), pz = br.ReadSingle();
                    float rx = br.ReadSingle(), ry = br.ReadSingle(), rz = br.ReadSingle(), rw = br.ReadSingle();
                    var key = new TrackKey{time=time, pos=new Vector3(px,py,pz), rot=new Quaternion(rx,ry,rz,rw)};
                    seqs[seqId].keys.Add(key);
                }
            }
            return seqs;
        }
    }
}
