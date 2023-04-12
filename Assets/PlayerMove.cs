// ---------------------------------------------------------  
// PlayerMove.cs  
//   
// 作成日:  2021/12/7
// 作成者:  MasterM
// ---------------------------------------------------------  
using UnityEngine;
using System.Collections;
using SimpleMan.VisualRaycast;

public class PlayerMove : MonoBehaviour
{

    #region 変数  
    private float g_moveSpeed = 50;
    private float g_xMoveValue = 0;
    private float g_zMoveValue = 0;

    private float g_xMaxSpeed = 5;
    private float g_zMaxSpeed = 5;

    private float g_jumpPower = 5;
    private bool g_isJump = false;
    private bool g_isGround = false;


    private Rigidbody g_playerRigidbody = default;

    private Animator g_playerAnimator = default;

    private PlayerState g_playerState = PlayerState.WAIT;
    #endregion

    #region プロパティ  
    enum PlayerState {
        WAIT,
        RUN,
        DEAD,
    }
    #endregion

    #region メソッド  
    private void OnDrawGizmos() {
        // ボックスキャストを描画
        //Gizmos.DrawCube(this.transform.position, new Vector3(0.5f, 0.05f, 0.5f));
    }

    private void Start() {
        // リジッドボディ取得
        g_playerRigidbody = this.GetComponent<Rigidbody>();
        // アニメータ取得
        g_playerAnimator = this.GetComponent<Animator>();
    }

    private void Update() {
        //RaycastHit hit;
        CastResult castResult;

        // BoxCastは見えないボックスを飛ばす
        // 第1引数:開始位置の中心位置　第2引数:ボックスサイズ（半径指定）　第3引数:向き　第4引数:ヒットしたときのRaycastHit　第5引数:角度　第6引数:長さ
        //Physics.BoxCast(this.transform.position + (transform.up * 0.5f), new Vector3(0.5f, 0.05f, 0.5f)/2, -transform.up, out hit, Quaternion.identity, 0.5f);
        castResult = this.Boxcast(false, transform.position + (transform.up * 0.5f), -transform.up, new Vector3(0.5f, 0.05f, 0.5f),0.5f);

        //print(hit.collider.name);
        // 着地判定
        if (castResult.FirstHit.collider != null) {
            g_isGround = true;
        } else {
            g_isGround = false;
        }
        if (g_isGround && !g_isJump) {
            g_playerAnimator.SetBool("Jump", false);
        }

        // 状態判定
        switch (g_playerState) {
            case PlayerState.WAIT:

                g_xMoveValue = Input.GetAxis("Horizontal");
                g_zMoveValue = Input.GetAxis("Vertical");

                // 動いたら状態をRUNにする
                if (g_xMoveValue != 0 || g_zMoveValue != 0) {
                    g_playerState = PlayerState.RUN;
                }
                break;

            case PlayerState.RUN:

                g_playerAnimator.SetBool("Run",true);
                g_xMoveValue = Input.GetAxis("Horizontal");
                g_zMoveValue = Input.GetAxis("Vertical");
                // 動かなくなったらWAIT状態にする
                if (g_xMoveValue == 0 && g_zMoveValue == 0) {
                    g_playerAnimator.SetBool("Run", false);
                    g_playerState = PlayerState.WAIT;
                } else {
                    // 向き変更
                    this.transform.localScale = new Vector3(
                        Mathf.Abs(this.transform.localScale.x) * Mathf.Sign(g_xMoveValue), 
                        this.transform.localScale.y, 
                        this.transform.localScale.z);
                }
                break;

            case PlayerState.DEAD:
                return;
        }
        if (Input.GetButtonDown("Jump")) {

            if (g_isGround) {
                g_isJump = true;
                g_playerAnimator.SetBool("Jump", true);
            }
        }
    }
    private void FixedUpdate() {
        // 状態判定
        switch (g_playerState) {
            case PlayerState.WAIT:
                break;

            case PlayerState.RUN:

                g_playerRigidbody.AddForce(
                    g_xMoveValue * g_moveSpeed,
                    0,
                    g_zMoveValue * g_moveSpeed,
                    ForceMode.Impulse);
                
                // xのスピード制御
                if (Mathf.Abs(g_playerRigidbody.velocity.x) > g_xMaxSpeed) {
                    g_playerRigidbody.velocity = new Vector3(
                        g_xMaxSpeed * Mathf.Sign(g_playerRigidbody.velocity.x),
                        g_playerRigidbody.velocity.y,
                        g_playerRigidbody.velocity.z);
                }
                // zのスピード制御
                if (Mathf.Abs(g_playerRigidbody.velocity.z) > g_zMaxSpeed) {
                    g_playerRigidbody.velocity = new Vector3(
                        g_playerRigidbody.velocity.x,
                        g_playerRigidbody.velocity.y,
                        g_zMaxSpeed * Mathf.Sign(g_playerRigidbody.velocity.z));
                }

                break;

            case PlayerState.DEAD:
                return;
        }
        if (g_isJump) {
            g_playerRigidbody.velocity = new Vector3(
                g_playerRigidbody.velocity.x,
                g_jumpPower,
                g_playerRigidbody.velocity.z
                );
            g_isJump = false;
        }
    }
    #endregion
}