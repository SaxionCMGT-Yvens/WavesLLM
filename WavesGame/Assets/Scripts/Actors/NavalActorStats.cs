using System;

namespace Actors
{
    [Serializable]
    public struct NavalActorStats
    {
        public int Strength; //Offensive
        public int Speed; //Movement
        public int Stability; //Constitution against waves
        public int Scrutiny; //Awareness
        public int Sturdiness; //Constitution against enemies
        public int Spirit; //Morale

        public NavalActorStats(int strength, int speed, int stability, int scrutiny, int sturdiness, int spirit)
        {
            Strength = strength;
            Speed = speed;
            Stability = stability;
            Scrutiny = scrutiny;
            Sturdiness = sturdiness;
            Spirit = spirit;
        }
    }
}