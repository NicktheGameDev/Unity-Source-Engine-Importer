using System.Collections.Generic;
using UnityEngine;
namespace Localization
{
    public static class LocalizationManager
    {
        public static string CurrentLanguage=LocalizationLanguage.English;
        static readonly Dictionary<string,Dictionary<string,string>> _dict=new(){
            {LocalizationLanguage.Japanese,LocalizationData.Japanese}
        };
        public static void SetLanguage(string lang){ if(_dict.ContainsKey(lang)||lang==LocalizationLanguage.English) CurrentLanguage=lang;}
        public static string Translate(string txt){
            if(CurrentLanguage==LocalizationLanguage.English) return txt;
            if(_dict[CurrentLanguage].TryGetValue(txt,out var trans)) return trans;
            return txt;
        }
    }
}
