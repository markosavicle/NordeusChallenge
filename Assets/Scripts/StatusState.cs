namespace NordeusChallenge.Unity
{
    [System.Serializable]
    public class StatusState
    {
        public string type;
        public int value;
        public int duration;

        public StatusState(string type, int value, int duration)
        {
            this.type = type;
            this.value = value;
            this.duration = duration;
        }
    }
}
