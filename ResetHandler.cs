using UnityEngine.InputSystem;

namespace BettencourtCamden.Lab6
{
    public class ResetHandler
    {
        private Respawner respawner;

        public ResetHandler(InputAction ResetAction, Respawner resp)
        {
            respawner = resp;
            ResetAction.performed += ResetAction_performed;
            ResetAction.Enable();
        }
        private void ResetAction_performed(InputAction.CallbackContext obj)
        {
            if(respawner != null)
            {
                respawner.Respawn();
            }
        }
    }
}
