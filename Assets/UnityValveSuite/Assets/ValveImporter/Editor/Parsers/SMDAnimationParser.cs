using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
namespace ValveImporter.Editor.Parsers
{
    public class SMDAnimation
    {
        public float FrameRate=30f;
        public List<SMDFrame> Frames=new();
    }
    public class SMDFrame
    {
        public float Time;
        public Dictionary<int,(Vector3 pos, Quaternion rot)> Bones=new();
    }
    public static class SMDAnimationParser
    {
        public static SMDAnimation Parse(string[] lines){
            var anim=new SMDAnimation();
            SMDFrame cur=null;
            foreach(var raw in lines){
                var line=raw.Trim();
                if(line.StartsWith("time")){
                    if(cur!=null) anim.Frames.Add(cur);
                    cur=new SMDFrame{Time=float.Parse(line.Split(' ')[1])};
                }else if(cur!=null && line.Length>0){
                    var t=line.Split(new[]{' ','\t'},StringSplitOptions.RemoveEmptyEntries);
                    if(t.Length==7){
                        int id=int.Parse(t[0]);
                        var pos=new Vector3(float.Parse(t[1],CultureInfo.InvariantCulture),
                                            float.Parse(t[2],CultureInfo.InvariantCulture),
                                            float.Parse(t[3],CultureInfo.InvariantCulture));
                        var rot=Quaternion.Euler(float.Parse(t[4],CultureInfo.InvariantCulture)*Mathf.Rad2Deg,
                                                 float.Parse(t[5],CultureInfo.InvariantCulture)*Mathf.Rad2Deg,
                                                 float.Parse(t[6],CultureInfo.InvariantCulture)*Mathf.Rad2Deg);
                        cur.Bones[id]=(pos,rot);
                    }
                }
            }
            if(cur!=null) anim.Frames.Add(cur);
            return anim;
        }
    }
}
