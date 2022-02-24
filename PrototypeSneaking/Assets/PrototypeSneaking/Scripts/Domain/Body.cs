using UnityEngine;

namespace PrototypeSneaking.Domain
{
    public interface IBody
    {
        string Name { get; }
        int GetInstanceID();
    }
    public class Body : MonoBehaviour
    {
        public string Name;

        public void Tell(string name)
        {
            Name = name;
        }

        public int GetInstanceID()
        {
            return gameObject.GetInstanceID();
        }
    }
}
