using System;

namespace AutoszervizAdmin
{
    public class Idopontok
    {
        public int Id { get; set; }
        public DateTime Datum { get; set; }
        public string UgyfelNeve { get; set; }
        public string Telefonszam { get; set; }
        public string Rendszam { get; set; }
        public string MunkaTipusa { get; set; }
        public string Statusz { get; set; }
    }

    public class Munkalap
    {
        public string Rendszam { get; set; }
        public string Szerelo { get; set; }
        public string Hibajelenseg { get; set; }
        public string Feladatok { get; set; }
        public string Alkatreszek { get; set; }
        public int Munkaido { get; set; }
        public string Statusz { get; set; }
    }
}