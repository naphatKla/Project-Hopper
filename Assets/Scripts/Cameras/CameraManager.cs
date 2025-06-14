using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;

namespace Cameras
{
    public class CameraManager : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera currentCamera;
    
        private void Awake()
        {
            if (!currentCamera.TryGetComponent(out CinemachinePositionComposer composer)) return;

            composer.Composition.DeadZone.Enabled = false;
            DOVirtual.DelayedCall(1f, () =>
            {
                composer.Composition.DeadZone.Enabled = true;
            });
        }

    }
}
