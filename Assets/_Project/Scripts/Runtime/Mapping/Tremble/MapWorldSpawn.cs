using TinyGoose.Tremble;

namespace GGJ.Mapping.Tremble
{
    public class MapWorldSpawn : Worldspawn
    {
        public static MapWorldSpawn Instance;
        
        private void OnEnable()
        {
            Instance = this;
        }

        private void OnDisable()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
