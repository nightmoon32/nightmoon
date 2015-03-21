using System;
using LeagueSharp;

namespace Oracle
{
    public struct GameObj
    {
        public float Damage;
        public bool Included;
        public string Name;
        public GameObject Obj;
        public int Start;

        public GameObj(string name, GameObject obj, bool included, float incdmg, int start)
        {
            Start = start;
            Name = name;
            Obj = obj;
            Included = included;
            Damage = incdmg;
        }
    }
}