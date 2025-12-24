using System.Collections.Generic;
using UnityEngine;

namespace Unity.Cinemachine.Samples
{
    /// <summary>
    /// This is an add-on for SimplePlayerController that controls the player's Aiming Core.
    /// 
    /// This component expects to be in a child object of a player that has a SimplePlayerController 
    /// behaviour.  It works intimately with that component.
    //
    /// The purpose of the aiming core is to decouple the camera rotation from the player rotation.  
    /// Camera rotation is determined by the rotation of the player core GameObject, and this behaviour 
    /// provides input axes for controlling it.  When the player core is used as the target for 
    /// a CinemachineCamera with a ThirdPersonFollow component, the camera will look along the core's 
    /// forward axis, and pivot around the core's origin.
    /// 
    /// The aiming core is also used to define the origin and direction of player shooting, if player 
    /// has that ability.  
    /// 
    /// To implement player shooting, add a SimplePlayerShoot behaviour to this GameObject.
    /// </summary>
    public class SimplePlayerAimController : MonoBehaviour, IInputAxisOwner
    {
        public enum CouplingMode { Coupled, CoupledWhenMoving, Decoupled }

        [Tooltip("How the player's rotation is coupled to the camera's rotation.  Three modes are available:\n"
            + "<b>Coupled</b>: The player rotates with the camera.  Sideways movement will result in strafing.\n"
            + "<b>Coupled When Moving</b>: Camera can rotate freely around the player when the player is stationary, "
                + "but the player will rotate to face camera forward when it starts moving.\n"
            + "<b>Decoupled</b>: The player's rotation is independent of the camera's rotation.")]
        public CouplingMode PlayerRotation;

        [Tooltip("How fast the player rotates to face the camera direction when the player starts moving.  "
            + "Only used when Player Rotation is Coupled When Moving.")]
        public float RotationDamping = 0.2f;

        [Tooltip("Horizontal Rotation.  Value is in degrees, with 0 being centered.")]
        public InputAxis HorizontalLook = new () { Range = new Vector2(-180, 180), Wrap = true, Recentering = InputAxis.RecenteringSettings.Default };

        [Tooltip("Vertical Rotation.  Value is in degrees, with 0 being centered.")]
        public InputAxis VerticalLook = new () { Range = new Vector2(-70, 70), Recentering = InputAxis.RecenteringSettings.Default };

        SimplePlayerControllerBase m_Controller;
        Transform m_ControllerTransform;    // cached for efficiency
        Quaternion m_DesiredWorldRotation;

        /// Report the available input axes to the input axis controller.
        /// We use the Input Axis Controller because it works with both the Input package
        /// and the Legacy input system.  This is sample code and we
        /// want it to work everywhere.
        void IInputAxisOwner.GetInputAxes(List<IInputAxisOwner.AxisDescriptor> axes)
        {
            //Tip：实际控制这里的控制器就应当在其检视器中设置Mouse Delta的X和Y值作为这里的输入值，那么该对象就可以与相机一样直接受到鼠标控制旋转了。
            axes.Add(new () { DrivenAxis = () => ref HorizontalLook, Name = "Horizontal Look", Hint = IInputAxisOwner.AxisDescriptor.Hints.X });
            axes.Add(new () { DrivenAxis = () => ref VerticalLook, Name = "Vertical Look", Hint = IInputAxisOwner.AxisDescriptor.Hints.Y });
        }

        void OnValidate()
        {
            HorizontalLook.Validate();
            VerticalLook.Range.x = Mathf.Clamp(VerticalLook.Range.x, -90, 90);
            VerticalLook.Range.y = Mathf.Clamp(VerticalLook.Range.y, -90, 90);
            VerticalLook.Validate();
        }
        
        void OnEnable()
        {
            m_Controller = GetComponentInParent<SimplePlayerControllerBase>();
            if (m_Controller == null)
                Debug.LogError("SimplePlayerController not found on parent object");
            else
            {
                /*Tip：这里的执行顺序很关键，只有这样才能达到目的。*/

                //先减再加，避免重复注册，代码水平的体现。
                m_Controller.PreUpdate -= UpdatePlayerRotation;
                m_Controller.PreUpdate += UpdatePlayerRotation;
                m_Controller.PostUpdate -= PostUpdate;
                m_Controller.PostUpdate += PostUpdate;
                m_ControllerTransform = m_Controller.transform;
            }
        }

        void OnDisable()
        {
            if (m_Controller != null)
            {
                m_Controller.PreUpdate -= UpdatePlayerRotation;
                m_Controller.PostUpdate -= PostUpdate;
                m_ControllerTransform = null;
            }
        }

        //重定向角色。
        /// <summary>Recenters the player to match my rotation</summary>
        /// <param name="damping">How long the recentering should take</param>
        public void RecenterPlayer(float damping = 0)
        {
            if (m_ControllerTransform == null)
                return;

            // Get my rotation relative to parent
            var rot = transform.localRotation.eulerAngles;
            rot.y = NormalizeAngle(rot.y);
            var delta = rot.y;
            delta = Damper.Damp(delta, damping, Time.deltaTime);

            // Rotate the parent towards me
            m_ControllerTransform.rotation = Quaternion.AngleAxis(
                delta, m_ControllerTransform.up) * m_ControllerTransform.rotation;

            // Rotate me in the opposite direction
            //Tip：因为父对象会带动子对象旋转，这里只是还原，以便子对象即该对象的世界旋转值不变，而父对象的世界旋转值变成与该对象相同。
            HorizontalLook.Value -= delta;
            rot.y -= delta;
            transform.localRotation = Quaternion.Euler(rot);
        }

        /// <summary>
        /// Set my rotation to look in this direction, without changing player rotation.
        /// Here we only set the axis values, we let the player controller do the actual rotation.
        /// </summary>
        /// <param name="worldspaceDirection">Direction to look in, in worldspace</param>
        public void SetLookDirection(Vector3 worldspaceDirection)
        {
            if (m_ControllerTransform == null)
                return;
            var rot = (Quaternion.Inverse(m_ControllerTransform.rotation) 
                * Quaternion.LookRotation(worldspaceDirection, m_ControllerTransform.up)).eulerAngles;
            HorizontalLook.Value = HorizontalLook.ClampValue(rot.y);
            VerticalLook.Value = VerticalLook.ClampValue(NormalizeAngle(rot.x));
        }
        

        // This is called by the player controller before it updates its own rotation.
        void UpdatePlayerRotation()
        {
            var t = transform;
            //与相机旋转输入同步
            t.localRotation = Quaternion.Euler(VerticalLook.Value, HorizontalLook.Value, 0);
            //世界坐标就是站在同一参考系上来记录和比较。在这里
            m_DesiredWorldRotation = t.rotation; 
            switch (PlayerRotation)
            {
                case CouplingMode.Coupled: 
                {
                    m_Controller.SetStrafeMode(true);
                    RecenterPlayer();
                    break;
                }
                //Tip：在角色不动的时候旋转镜头时不会带动角色旋转，这应该是更舒服的手感。
                case CouplingMode.CoupledWhenMoving:
                {
                    // If the player is moving, rotate its yaw to match the camera direction,
                    // otherwise let the camera orbit
                    m_Controller.SetStrafeMode(true);
                    if (m_Controller.IsMoving)
                        RecenterPlayer(RotationDamping);
                    break;
                }
                case CouplingMode.Decoupled: 
                {
                    m_Controller.SetStrafeMode(false);
                    break;
                }
            }
            VerticalLook.UpdateRecentering(Time.deltaTime, VerticalLook.TrackValueChange());
            HorizontalLook.UpdateRecentering(Time.deltaTime, HorizontalLook.TrackValueChange());
        }

        // Callback for player controller to update our rotation after it has updated its own.
        void PostUpdate(Vector3 vel, float speed)
        {
            /*Tip：父对象会带动子对象，也就是说原本就应该是Coupled模式，而为了实现Decoupled模式，就有了这里的逻辑，就是在作为父对象的角色运动之后，在此处
            就是将在当前帧角色运动之前的旋转值记录下来，然后在运动之后在此处恢复回来，也就是在鼠标不动、只有角色运动的时候，该对象的世界旋转值是保持不变的。
            而这里的重点是，父对象的旋转变了，但是要保持子对象的世界旋转不变，所以就就必须改变子对象的局部旋转，也就是这里所做的事，求出子对象应该的局部旋转值，
            记录到InputAxis的Value上，然后在下一帧调用UpdatePlayerRotation时，两个Look的值加上相机旋转的输入值就是当前帧的局部旋转值，同时也将此时的世界旋转值存储起来，再次循环逻辑。
            由此一来，就实现了，在鼠标不动、只有角色运动的时候，该对象的世界旋转值是保持不变的，或者更准确地说，这样处理之后，该对象的旋转是独立于父对象旋转的，也就是Decoupled模式。
            */
            if (PlayerRotation == CouplingMode.Decoupled)
            {
                // After player has been rotated, we subtract any rotation change 
                // from our own transform, to maintain our world rotation
                transform.rotation = m_DesiredWorldRotation;
                //因为该组件所在对象是m_ControllerTransform所在对象的子对象，会被带动，在此处将该对象的世界值反向旋转父对象的世界值，就得到了相对于父对象的局部旋转值。
                var delta = (Quaternion.Inverse(m_ControllerTransform.rotation) * m_DesiredWorldRotation).eulerAngles;
                //这里对应的就是局部旋转值。
                VerticalLook.Value = NormalizeAngle(delta.x);
                HorizontalLook.Value = NormalizeAngle(delta.y);
            }
        }

        //将角度转换到[-180,180]，因为这个范围内就能够表示所有角度了。
        float NormalizeAngle(float angle)
        {
            while (angle > 180)
                angle -= 360;
            while (angle < -180)
                angle += 360;
            return angle;
        }
    }
}
