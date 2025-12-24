#if CINEMACHINE_PHYSICS
using UnityEngine;

namespace Unity.Cinemachine
{
    /// <summary>
    /// An add-on module for CinemachineCamera that forces the LookAt
    /// point to the center of the screen, based on the Follow target's orientation,
    /// cancelling noise and other corrections.
    /// This is useful for third-person style aim cameras that want a dead-accurate
    /// aim at all times, even in the presence of positional or rotational noise.
    /// </summary>
    [AddComponentMenu("Cinemachine/Procedural/Rotation Control/Cinemachine Third Person Aim")]
    [ExecuteAlways]
    [SaveDuringPlay]
    [DisallowMultipleComponent]
    [HelpURL(Documentation.BaseURL + "manual/CinemachineThirdPersonAim.html")]
    public class CinemachineThirdPersonAim : CinemachineExtension
    {
        /// <summary>Objects on these layers will be detected.</summary>
        [Header("Aim Target Detection")]
        [Tooltip("Objects on these layers will be detected")]
        public LayerMask AimCollisionFilter;

        /// <summary>Objects with this tag will be ignored.  
        /// It is a good idea to set this field to the target's tag.</summary>
        [TagField]
        [Tooltip("Objects with this tag will be ignored.  "
            + "It is a good idea to set this field to the target's tag")]
        public string IgnoreTag = string.Empty;

        /// <summary>How far to project the object detection ray.</summary>
        [Tooltip("How far to project the object detection ray")]
        public float AimDistance;

        /// <summary>If set, camera noise will be adjusted to stabilize target on screen.</summary>
        [Tooltip("If set, camera noise will be adjusted to stabilize target on screen")]
        public bool NoiseCancellation = true;

        /// <summary>World space position of where the player would hit if a projectile were to 
        /// be fired from the player origin.  This may be different
        /// from state.ReferenceLookAt due to camera offset from player origin.</summary>
        public Vector3 AimTarget { get; private set; } //就是实际发射出去会命中的位置

        void OnValidate()
        {
            AimDistance = Mathf.Max(1, AimDistance);
        }

        void Reset()
        {
            AimCollisionFilter = 1;
            IgnoreTag = string.Empty;
            AimDistance = 200.0f;
            NoiseCancellation = true;
        }
        
        /// <summary>
        /// Sets the ReferenceLookAt to be the result of a raycast in the direction of camera forward.
        /// If an object is hit, point is placed there, else it is placed at AimDistance along the ray.
        /// </summary>
        /// <param name="vcam">The virtual camera being processed</param>
        /// <param name="stage">The current pipeline stage</param>
        /// <param name="state">The current virtual camera state</param>
        /// <param name="deltaTime">The current applicable deltaTime</param>
        protected override void PostPipelineStageCallback(
            CinemachineVirtualCameraBase vcam,
            CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
        {
            switch (stage)
            {
                case CinemachineCore.Stage.Body:
                {
                    if (NoiseCancellation)
                    {
                        // Raycast to establish what we're actually aiming at
                        var player = vcam.Follow;
                        if (player != null)
                        {
                            state.ReferenceLookAt = ComputeLookAtPoint(state.GetCorrectedPosition(), player, player.forward);
                            AimTarget = ComputeAimTarget(state.ReferenceLookAt, player);
                        }
                    }
                    break;
                }
                case CinemachineCore.Stage.Finalize:
                {
                    if (NoiseCancellation)
                    {
                        // Stabilize the LookAt point in the center of the screen
                        var dir = state.ReferenceLookAt - state.GetFinalPosition();
                        if (dir.sqrMagnitude > 0.01f)
                        {
                            state.RawOrientation = Quaternion.LookRotation(dir, state.ReferenceUp);
                            state.OrientationCorrection = Quaternion.identity; //取消修正。
                        }
                    }
                    else
                    {
                        // Raycast to establish what we're actually aiming at.
                        // In this case we do it without cancelling the noise.
                        var player = vcam.Follow;
                        if (player != null)
                        {
                            state.ReferenceLookAt = ComputeLookAtPoint(
                                state.GetCorrectedPosition(), player, state.GetCorrectedOrientation() * Vector3.forward);
                            AimTarget = ComputeAimTarget(state.ReferenceLookAt, player);
                        }
                    }
                    break;
                }
            }
        }

        Vector3 ComputeLookAtPoint(Vector3 camPos, Transform player, Vector3 fwd)
        {
            // We don't want to hit targets behind the player
            var aimDistance = AimDistance;
            var playerOrientation = player.rotation;
            /*Ques：这里转换很奇怪，我知道是从player的局部坐标系转换到世界坐标系，但是出于什么理由？我能想到的是camPos基本是参考player的局部坐标系，而这里其实是世界坐标，
            所以转换之后取其z值才是所期望的z轴方向上的值，否则player的局部坐标系可能“歪”得很厉害（尽管通常游戏中使用得都比较简单）。
            */
            var playerPosLocal = Quaternion.Inverse(playerOrientation) * (player.position - camPos);
            //z大于0，说明camPos与player.position不在同一个xy平面内，并且player在camPos的前方。
            if (playerPosLocal.z > 0)
            {
                //将camPos移动到player局部坐标系的xy平面上，注意是沿其z轴正方向移动，即camPos相对于该坐标系x和y值不变。
                //aimDistance就是减去camPos所移动的距离，因为aimDistance的值就是相对于相机原本位置来设置的。
                camPos += fwd * playerPosLocal.z;
                aimDistance -= playerPosLocal.z;
            }

            aimDistance = Mathf.Max(1, aimDistance);
            bool hasHit = RuntimeUtility.RaycastIgnoreTag(new Ray(camPos, fwd), 
                out RaycastHit hitInfo, aimDistance, AimCollisionFilter, IgnoreTag);
            
            //击中了可行目标，就以击中点作为看向位置，否则就是取沿前方的给定最大长度位置。
            return hasHit ? hitInfo.point : camPos + fwd * aimDistance;
        }
        
        /*Tip：这里的偏移指的是，对于TPS，相机的中心往往并非角色的中心，而从实际情况来看，子弹应该从从角色射出而非从相机中心射出，但是瞄准点是基于屏幕中心的，也就是相机的中心，
        因为这样设计会更加符合现实直觉，并且射击游戏也一直都是如此设计的，而通过这样射线检测找到击中点之后，就确定了终点，然后再以角色为起点，就可以两点定一线、得到射击轨道了，
        而显然，这样有可能在屏幕中心不会碰撞的物体，以角色为起点之后就会碰到了，比如角色靠墙的时候，第三人称相机可以看到墙边，但从角色的视角来说是看不到的或者说射不过去的。
        */
        Vector3 ComputeAimTarget(Vector3 cameraLookAt, Transform player)
        {
            // Adjust for actual player aim target (may be different due to offset)
            var playerPos = player.position;
            var dir = cameraLookAt - playerPos;
            //以角色为起点（出射点）之后，路线上碰撞到了其他对象。
            if (RuntimeUtility.RaycastIgnoreTag(new Ray(playerPos, dir), 
                out RaycastHit hitInfo, dir.magnitude, AimCollisionFilter, IgnoreTag))
                return hitInfo.point;
            return cameraLookAt;
        }
    }
}
#endif
