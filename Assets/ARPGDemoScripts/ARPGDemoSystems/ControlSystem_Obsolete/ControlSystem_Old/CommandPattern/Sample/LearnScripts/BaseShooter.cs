using System.Collections;
using System.Collections.Generic;
using CommandPattern.LearnScripts;
using CommandPattern.Scripts;
using UnityEngine;
/*Tip：这里的思路就是在初始化时就构造好所有可能用到的命令实例，然后每帧根据输入判断使用哪个命令，就调用其Execute方法传入自身来执行命令，
所以从这个逻辑上来看，其实不太符合“生成命令、执行命令”的描述，因为命令是在起初就生成好了，运行时只是根据当前帧输入来决定执行哪个命令而已。
但是这样好像才是合理的，因为一个角色能够执行的命令本来就是预先确定好的，*/

public abstract class BaseShooter : MonoBehaviour, IGameActor
{
    protected ICommand moveForward;
    protected ICommand moveBack;
    protected ICommand moveLeft;
    protected ICommand moveRight;
    protected ICommand turnLeft;
    protected ICommand turnRight;
    protected ICommand fire;
    protected ICommand emptyCommand;

    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private GameObject BulletPrefab;
    [SerializeField] private float BulletSpeed;
    [SerializeField] private Transform GunRoot;
    [SerializeField] private float ShootInterval;
    private float _lastShootTime;

    // Start is called before the first frame update
    void Start()
    {
        moveForward = CommandFactory.Create(KeyCode.W);
        moveBack = CommandFactory.Create(KeyCode.S);
        moveLeft = CommandFactory.Create(KeyCode.A);
        moveRight = CommandFactory.Create(KeyCode.D);
        turnLeft = CommandFactory.Create(KeyCode.Q);
        turnRight = CommandFactory.Create(KeyCode.E);
        fire = CommandFactory.Create(KeyCode.J);
        emptyCommand = CommandFactory.Create(default);
    }

    // Update is called once per frame
    void Update()
    {
        var command = HandleInput();
        command?.Execute(this);
    }

    protected abstract ICommand HandleInput();


    public void Fire()
    {
        if (Time.time - _lastShootTime < ShootInterval)
        {
            return;
        }

        _lastShootTime = Time.time;
        var bulletGo = GameObject.Instantiate(BulletPrefab);
        bulletGo.transform.position = GunRoot.position;
        bulletGo.transform.forward = GunRoot.forward;
        var bullet = bulletGo.AddComponent<Bullet>();
        bullet.Speed = BulletSpeed;
    }

    public void MoveBack()
    {
        transform.position -= (transform.forward * (moveSpeed * Time.deltaTime) );
    }

    public void MoveForward()
    {
        transform.position += (transform.forward * (moveSpeed * Time.deltaTime) );
    }

    public void MoveLeft()
    {
        transform.position += (-transform.right * (moveSpeed * Time.deltaTime) );
    }

    public void MoveRight()
    {
        transform.position += (transform.right * (moveSpeed * Time.deltaTime) );
    }

    public void TurnLeft()
    {
        var lastEuler = transform.localEulerAngles;
        transform.localEulerAngles = new Vector3(lastEuler.x, lastEuler.y - rotateSpeed* Time.deltaTime, lastEuler.z);
    }

    public void TurnRight()
    {
        var lastEuler = transform.localEulerAngles;
        transform.localEulerAngles = new Vector3(lastEuler.x, lastEuler.y + rotateSpeed* Time.deltaTime, lastEuler.z);
    }
}
