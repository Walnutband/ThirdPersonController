// using UnityEngine;

// public class PlayerMovement : MonoBehaviour
// {
//     private CharacterController controller;//内置组件
//     private Vector3 playerVelocity;//
//     private bool groundedPlayer;
//     private float playerSpeed = 2.0f;//基本速度
//     private float jumpHeight = 1.0f;//跳跃高度
//     private float gravityValue = -9.81f;//重力加速度

//     private void Start()
//     {
//         controller = GetComponent<CharacterController>();
//     }

//     private void Update()
//     {
//         groundedPlayer = controller.isGrounded;
//         //如果角色在地面上且垂直速度小于零，将垂直速度设为零。
//         //也就是落到地上时垂直速度骤减为0
//         if (groundedPlayer && playerVelocity.y < 0)
//         {
//             playerVelocity.y = 0f;
//         }
//         //根据水平和垂直输入获取移动向量。
//         /*字符串表示，而不指定确定的按键，则通过修改与字符串对应的键位就可以实现改建
//          Horizontal指的是左右即X，Vertical指的是上下即Y，
//         移动向量move表示的是方向，即0不动，1向正，-1向负。乘上大小即得到速度向量，
//         所以playerSpeed应该是标量速率，即速度的大小*/
//         Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
//         //调用Move函数才能移动CharacterController
//         controller.Move(move * Time.deltaTime * playerSpeed);//距离=速度X时间

//         //只要发生了移动就转向
//         if (move != Vector3.zero)
//         {
//             //此处指的是物体局部坐标系的正z轴，即模型的正前方，也就是转向正对移动的方向
//             gameObject.transform.forward = move;
//         }

//         //如果按下跳跃按钮且角色在地面上，计算跳跃速度
//         if (Input.GetButtonDown("Jump") && groundedPlayer)
//         {
//             //将 jumpHeight 乘以 -3.0f。这是一个调整因子，用于使跳跃速度更合适。通常情况下，你会看到 -2.0f 或 -3.0f，以便在游戏中获得更好的跳跃效果。
//             playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
//         }
//         /*由此可以看到，看似连续的运动过程，其实就是分为每一帧计算并显示，就像微积分
//          无限细分一样，由于正常情况下每秒60帧，每帧的间隔时间对于人来说已经无限接近于0
//         即看起来是完全连续的了。
//         总之，落实到基本单位即一帧来进行深入思考*/
//         playerVelocity.y += gravityValue * Time.deltaTime;
//         controller.Move(playerVelocity * Time.deltaTime);


//     }
// }
