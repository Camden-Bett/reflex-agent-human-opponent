using UnityEngine;
using UnityEngine.InputSystem;

namespace BettencourtCamden.Lab6
{
    public class Respawner : MonoBehaviour
    {
        [SerializeField] private GameObject playerToRespawn;
        [SerializeField] private GameObject agentToRespawn;
        private InputAction respawnAction;
        private Vector3 playerSpawnCoords;
        private Vector3 agentSpawnCoords;
        private Quaternion playerSpawnRotation;
        private Quaternion agentSpawnRotation;

        public void Initialize(InputAction respawn)
        {
            respawnAction = respawn;
            respawnAction.Enable();
        }

        private void Start()
        {
            playerSpawnCoords = playerToRespawn.transform.position;
            agentSpawnCoords = agentToRespawn.transform.position;
            playerSpawnRotation = playerToRespawn.transform.rotation;
            agentSpawnRotation = agentToRespawn.transform.rotation;
        }

        public void Respawn()
        {
            playerToRespawn.transform.position = playerSpawnCoords;
            playerToRespawn.transform.rotation = playerSpawnRotation;

            agentToRespawn.transform.position = agentSpawnCoords;
            playerToRespawn.transform.rotation = agentSpawnRotation;

            playerToRespawn.GetComponent<Rigidbody>().velocity = Vector3.zero;
            agentToRespawn.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }
}
