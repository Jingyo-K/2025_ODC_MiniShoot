using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 1. 조준탄 3회 발사 후 방사탄 2회 발사
/// 2. 5개영역 경고후 포격
/// 3. 전방향탄 발사
/// </summary>
public class Boss1Phase3 : EnemyState
{
    // Start is called before the first frame update

    public override void Enter()
    {
        base.Enter();
    }
    public override void Execute()
    {
        base.Execute();
    }
    public override void Exit()
    {
        base.Exit();
    }
    private void AimingBullet()
    {
        // Implement the aiming bullet behavior here
    }

    private void SpreadBullet()
    {
        // Implement the spread bullet behavior here
    }

    private void Shelling()
    {
        // Implement the shelling behavior here
    }
}
