// --------------------------------------------------------------------------------------------------------------------
// <copyright file=NetworkTurn.cs company=League of HTC Vive Developers>
/*
11111111111111111111111111111111111111001111111111111111111111111
11111111111111111111111111111111111100011111111111111111111111111
11111111111111111111111111111111100001111111111111111111111111111
11111111111111111111111111111110000111111111111111111111111111111
11111111111111111111111111111000000111111111111111111111111111111
11111111111111111111111111100000011110001100000000000000011111111
11111111111111111100000000000000000000000000000000011111111111111
11111111111111110111000000000000000000000000000011111111111111111
11111111111111111111111000000000000000000000000000000000111111111
11111111111111111110000000000000000000000000000000111111111111111
11111111111111111100011100000000000000000000000000000111111111111
11111111111111100000110000000000011000000000000000000011111111111
11111111111111000000000000000100111100000000000001100000111111111
11111111110000000000000000001110111110000000000000111000011111111
11111111000000000000000000011111111100000000000000011110001111111
11111110000000011111111111111111111100000000000000001111100111111
11111111000001111111111111111111110000000000000000001111111111111
11111111110111111111111111111100000000000000000000000111111111111
11111111111111110000000000000000000000000000000000000111111111111
11111111111111111100000000000000000000000000001100000111111111111
11111111111111000000000000000000000000000000111100000111111111111
11111111111000000000000000000000000000000001111110000111111111111
11111111100000000000000000000000000000001111111110000111111111111
11111110000000000000000000000000000000111111111110000111111111111
11111100000000000000000001110000001111111111111110001111111111111
11111000000000000000011111111111111111111111111110011111111111111
11110000000000000001111111111111111100111111111111111111111111111
11100000000000000011111111111111111111100001111111111111111111111
11100000000001000111111111111111111111111000001111111111111111111
11000000000001100111111111111111111111111110000000111111111111111
11000000000000111011111111111100011111000011100000001111111111111
11000000000000011111111111111111000111110000000000000011111111111
11000000000000000011111111111111000000000000000000000000111111111
11001000000000000000001111111110000000000000000000000000001111111
11100110000000000001111111110000000000000000111000000000000111111
11110110000000000000000000000000000000000111111111110000000011111
11111110000000000000000000000000000000001111111111111100000001111
11111110000010000000000000000001100000000111011111111110000001111
11111111000111110000000000000111110000000000111111111110110000111
11111110001111111100010000000001111100000111111111111111110000111
11111110001111111111111110000000111111100000000111111111111000111
11111111001111111111111111111000000111111111111111111111111100011
11111111101111111111111111111110000111111111111111111111111001111
11111111111111111111111111111110001111111111111111111111100111111
11111111111111111111111111111111001111111111111111111111001111111
11111111111111111111111111111111100111111111111111111111111111111
11111111111111111111111111111111110111111111111111111111111111111
*/
//   
// </copyright>
// <summary>
//  Chinese Chess VR
// </summary>
// <author>胡良云（CloudHu）</author>
//中文注释：胡良云（CloudHu） 3/22/2017

// --------------------------------------------------------------------------------------------------------------------

using Photon;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using VRTK;
using Lean;

/// <summary>
/// FileName: NetworkTurn.cs
/// Author: 胡良云（CloudHu）
/// Corporation: 
/// Description: 这个脚本用于处理网络回合
/// DateTime: 3/22/2017
/// </summary>
public class NetworkTurn : PunBehaviour, IPunTurnManagerCallbacks {
	
	#region Public Variables  //公共变量区域
	[Tooltip("棋局一回合的时间")]
	public float TurnTime=120f;
    public AudioSource source;
    public enum ChessPlayerType	//棋手类型
    {
        Red,    //红方
        Black,  //黑方
        Guest   //游客
    }
    [Tooltip("玩家类型")]
    public ChessPlayerType localPlayerType = ChessPlayerType.Guest;

	/// <summary>  
	/// 被选中的棋子的ID，若没有被选中的棋子，则ID为-1  
	/// </summary>  
	[Tooltip("被选中的棋子的ID")]
	public int _selectedId=-1;  

	[Tooltip("是否是红子的回合,默认红子先行")]
	public bool _isRedTurn=true;

	public struct step	//每步棋的结构
	{
		public int moveId;
		public int killId;

		public float xFrom;
		public float zFrom;
		public float xTo;
		public float zTo;

		public step(int _moveId,int _killId,float _xFrom,float _zFrom,float _xTo,float _zTo){
			moveId = _moveId;
			killId = _killId;
			xFrom =_xFrom;
			zFrom =_zFrom;
			xTo=_xTo;
			zTo = _zTo;
		}
	}
		
	[Tooltip("保存每一步走棋")]
	public List<step> _steps = new List<step> ();

	[Tooltip("音效:胜利,失败...")]
	public AudioClip selectClap,winMusic,loseMusic,welcomMusic,DrawMusic,hurryUp,JoinClip,LeaveClip;

	[Tooltip("德邦总管")]
	public ChessmanManager chessManManager;
    [Tooltip("之前选中的棋子")]
    public GameObject Selected;
    [Tooltip("路径")]
    public GameObject Path;
    static public NetworkTurn Instance;
    #endregion


    #region Private Variables   //私有变量区域

	[Header("游戏面板")]
	[Tooltip("游戏UI视图")]
	[SerializeField]
	private RectTransform GameUiView;

	[Tooltip("按钮幕布组")]
	[SerializeField]
	private CanvasGroup ButtonCanvasGroup;

	[Tooltip("断连面板")]
	[SerializeField]
	private RectTransform DisconnectedPanel;

	[Tooltip("请求面板")]
	[SerializeField]
	private RectTransform RequestPanel;

	[Header("本地玩家")]
	[Tooltip("本地玩家文本")]
	[SerializeField]
	private Text LocalPlayerNameText;
	[Tooltip("本地玩家时间,得分,回合文本")]
	[SerializeField]
	private Text LocalPlayerTimeText,LocalScoreText,LocalTurnText;
	[Tooltip("本地游戏状态文本")]
	[SerializeField]
	private Text LocalGameStatusText;

	[Header("远程玩家")]
	[Tooltip("远程玩家文本")]
	[SerializeField]
	private Text RemotePlayerNameText;

    [Tooltip("远程玩家时间,得分,回合文本")]
    [SerializeField]
	private Text RemotePlayerTimeText,RemoteScoreText,RemoteTurnText;

    [Tooltip("远程游戏状态文本")]
    [SerializeField]
	private Text RemoteGameStatusText;
	[Header("图片精灵")]
    [Tooltip("输赢图片")]
	[SerializeField]
	private Image WinOrLossImage;

	[Tooltip("胜利精灵")]
	[SerializeField]
	private Sprite SpriteWin;

	[Tooltip("失败精灵")]
	[SerializeField]
	private Sprite SpriteLose;

	[Tooltip("平局精灵")]
	[SerializeField]
	private Sprite SpriteDraw;

    [Tooltip("下一回合")]
    [SerializeField]
    private Sprite Next;

	private ResultType result=ResultType.None;	//结果

	private PunTurnManager turnManager;	//回合管家

	private bool remoteSelection;	//远程玩家选择

	private bool IsShowingResults;	//追踪显示结果的时机来处理游戏逻辑.

    private GameObject instance;

	private PhotonPlayer local;
	private PhotonPlayer remote;

    #endregion

    public enum ResultType	//结果类型枚举
	{
		None = 0,
		Draw,	//和
		LocalWin,	//赢
		LocalLoss	//输
	}
	#region Mono Callbacks //Unity的回调函数

	public void Start()
	{
		this.turnManager = this.gameObject.AddComponent<PunTurnManager>();	//添加组件并赋值
		this.turnManager.TurnManagerListener = this;	//为监听器赋值,从而触发下面的回调函数来完成游戏逻辑
		this.turnManager.TurnDuration = TurnTime;		//初始化回合持续时间
		if (this.source == null) this.source = FindObjectOfType<AudioSource>();
		Instance = this;

		RefreshUIViews();	//刷新UI视图
	}

	public void Update()
	{
		// 检查我们是否脱离了环境.
		if (this.DisconnectedPanel ==null)
		{
			Destroy(this.gameObject);
		}


		if ( ! PhotonNetwork.inRoom)	//不在房间则退出更新
		{
			return;
		}

		// 如果PUN已连接或正在连接则禁用"reconnect panel"（重连面板）
		if (PhotonNetwork.connected && this.DisconnectedPanel.gameObject.GetActive())
		{
			this.DisconnectedPanel.gameObject.SetActive(false);
		}

		if (!PhotonNetwork.connected && !PhotonNetwork.connecting && !this.DisconnectedPanel.gameObject.GetActive())
		{
			this.DisconnectedPanel.gameObject.SetActive(true);
		}


		if (PhotonNetwork.room.PlayerCount>1)
		{
			if (this.turnManager.IsOver)
			{
				return;	//回合结束
			}

			if (this.turnManager.Turn > 0  && ! IsShowingResults)
			{
				float leftTime = this.turnManager.RemainingSecondsInTurn;

				if (_isRedTurn && localPlayerType==ChessPlayerType.Red) {
                    LocalPlayerTimeText.text = leftTime.ToString("F1") + "秒";
				} else {
                    RemotePlayerTimeText.text =leftTime.ToString("F1") + "秒";
				}
			}
		}

	}

	#endregion

    #region Public Methods	//公共方法区域

	/// <summary>
	/// 尝试移动棋子.
	/// </summary>
	/// <param name="killId">击杀棋子ID.</param>
	/// <param name="x">The x coordinate坐标.</param>
	/// <param name="z">The z coordinate坐标.</param>
    public void TryMoveChessman(int killId, float x, float z)
    {
		bool ret = CanMove(_selectedId, killId,x,z);

        if (ret)
        {
			MoveStone(_selectedId, killId, x,z);
            OnMoveChessman(_selectedId, killId, x, z);
        }
    }

	/// <summary>
	/// 判断输赢.
	/// </summary>
    public void Judge(){
		
        if (ChessmanManager.chessman[0]._dead)  //红方：帅死则输，将死则赢
        {
			if (localPlayerType == ChessPlayerType.Red) {
				result = ResultType.LocalLoss;
				PlayMusic (loseMusic);
			}
			if (localPlayerType == ChessPlayerType.Black) {
				result = ResultType.LocalWin;
				PlayMusic(winMusic);
			}
            LocalGameStatusText.text = "恭喜黑方获胜！Black Win！";
        }

        if (ChessmanManager.chessman[16]._dead)
        {
			if (localPlayerType == ChessPlayerType.Black) {
				result = ResultType.LocalLoss;
				PlayMusic (loseMusic);
			}
			if (localPlayerType == ChessPlayerType.Red) {
				result = ResultType.LocalWin;
				PlayMusic(winMusic);
			}
            LocalGameStatusText.text = "恭喜红方获胜！Red Win！";
        }
			

	}



	#endregion

	#region TurnManager Callbacks	//回调区域

	/// <summary>
	/// 发起回合开始事件.
	/// </summary>
	/// <param name="turn">回合.</param>
	public void OnTurnBegins(int turn)
	{
		//Debug.Log("OnTurnBegins() turn: "+ turn);
        _selectedId = -1;
		if (this.LocalTurnText != null)
		{
			this.LocalTurnText.text = (this.turnManager.Turn*0.5f).ToString();	//更新回合数
			RemoteTurnText.text = LocalTurnText.text;

		}
		this.WinOrLossImage.gameObject.SetActive(false);	//关闭输赢的图片
		IsShowingResults = false;	//不展示结果
		ButtonCanvasGroup.interactable = true;	//可以与按钮交互
	}

	/// <summary>
	/// 当回合完成时调用(被所有玩家完成)
	/// </summary>
	/// <param name="turn">回合索引</param>
	/// <param name="obj">Object.</param>
	public void OnTurnCompleted(int obj)
	{
		Debug.Log("OnTurnCompleted: " + obj);

		this.Judge();	//计算输赢
		this.UpdateScores();	//更新得分
		this.OnEndTurn();	//结束回合
	}


	/// <summary>
	/// 当玩家有动作时调用(但是没有完成该回合)
	/// </summary>
	/// <param name="player">玩家引用</param>
	/// <param name="turn">回合索引</param>
	/// <param name="move">移动对象数据</param>
	/// <param name="photonPlayer">Photon player.</param>
	public void OnPlayerMove(PhotonPlayer photonPlayer, int turn, object move)
	{
		//Debug.Log("OnPlayerMove: " + photonPlayer + " turn: " + turn + " action: " + move);
		string strMove = move.ToString ();
		if (strMove.StartsWith ("+C")) {
			
			if (!photonPlayer.IsLocal) {
				string[] strArr = strMove.Split (char.Parse (" "));
				if (strArr [0] == "+ConfirmedSelect") {
				
					ConfirmedSelect (int.Parse (strArr [1]));

				} else {
					CancelSelected (int.Parse (strArr [1]));
				}
			}
		} else {
			switch (strMove) {
			case "Hurry":	//催棋
				PlayMusic (hurryUp);
				break;
			case "Restart":
				if (!photonPlayer.IsLocal)
					PopRequest ("重新开始对局");
				break;
			case "Draw":
				if (!photonPlayer.IsLocal)
					PopRequest ("和棋");
				break;
			case "Back":
				if (!photonPlayer.IsLocal)
					PopRequest ("悔棋");
				break;
			case "重新开始对局Yes":
				Restart ();
				break;
			case "重新开始对局No":
				LocalGameStatusText.text = "重新开局失败";
				break;
			case "和棋Yes":
				turnManager.SendMove ("Draw", true);
				break;
			case "和棋No":
				LocalGameStatusText.text = "和棋失败";
				break;
			case "悔棋Yes":
				BackOne ();
				break;
			case "悔棋No":
				LocalGameStatusText.text = "悔棋失败";
				break;
			default:
				break;
			}
		}

	}


	/// <summary>
	/// 当玩家完成回合时调用(包括该玩家的动作/移动)
	/// </summary>
	/// <param name="player">玩家引用</param>
	/// <param name="turn">回合索引</param>
	/// <param name="move">移动对象数据</param>
	/// <param name="photonPlayer">Photon player.</param>
	public void OnPlayerFinished(PhotonPlayer photonPlayer, int turn, object move)
	{
		//Debug.Log("OnTurnFinished: " + photonPlayer + " turn: " + turn + " action: " + move);
		string tmpStr = move.ToString ();
        if (tmpStr.Contains("s"))
        {
            if (!photonPlayer.IsLocal)
            {
                string[] strArr = tmpStr.Split(char.Parse("s"));
                MoveStone(int.Parse(strArr[0]), int.Parse(strArr[1]), float.Parse(strArr[4]),float.Parse(strArr[5]));
            }
        }
        else
        {
            switch (tmpStr)
            {
                case "BlackDefeat":
                    RemoteGameStatusText.text = "黑方认输！";
                    if (localPlayerType == ChessPlayerType.Red)
                    {
                        result = ResultType.LocalWin;
                    }
                    break;
                case "RedDefeat":
                    RemoteGameStatusText.text = "红方认输！";
                    if (localPlayerType == ChessPlayerType.Black)
                    {
                        result = ResultType.LocalWin;
                    }
                    break;
                case "Draw":
                    result = ResultType.Draw;
                    break;
                default:
                    break;
            }
        }  
			
	}


	/// <summary>
	/// 当回合由于时间限制完成时调用(回合超时)
	/// </summary>
	/// <param name="turn">回合索引</param>
	/// <param name="obj">Object.</param>
	public void OnTurnTimeEnds(int obj)
	{
		if (!IsShowingResults)
		{
			//Debug.Log("OnTurnTimeEnds: Calling OnTurnCompleted");
			if (_isRedTurn) {
				
				//红方超时，判输
				if (localPlayerType==ChessPlayerType.Red) {
					result = ResultType.LocalLoss;
					LocalGameStatusText.text = "您已超时，判负！";
					RemoteGameStatusText.text = "胜利！";
				}
				if (localPlayerType==ChessPlayerType.Black) {
					result = ResultType.LocalWin;
					LocalGameStatusText.text = "对方超时，胜利！";
					RemoteGameStatusText.text = "失败！";
				}

			} else {
				//黑方超时，判输
				if (localPlayerType==ChessPlayerType.Red) {
					result = ResultType.LocalLoss;
					LocalGameStatusText.text = "对方超时，胜利！";
					RemoteGameStatusText.text = "失败！";
				}
				if (localPlayerType==ChessPlayerType.Black) {
					result = ResultType.LocalWin;
					LocalGameStatusText.text = "您已超时，判负！";
					RemoteGameStatusText.text = "胜利！";
				}
			}
				
			OnTurnCompleted(-1);
		}
	}

	/// <summary>
	/// 更新得分
	/// </summary>
	private void UpdateScores()
	{
		if (this.result == ResultType.LocalWin)
		{
			PhotonNetwork.player.AddScore(1);   //这是PhotonPlayer的扩展方法.就是给玩家加分
            UpdatePlayerScoreTexts();
        }
	}
	#endregion

	#region Core Gameplay Methods	//核心玩法


	/// <summary>调用来开始回合 (只有主客户端会发送).</summary>
	public void StartTurn()
	{
		if (PhotonNetwork.isMasterClient)
		{
			this.turnManager.BeginTurn();
		}

		if (_isRedTurn) {

			if (localPlayerType==ChessPlayerType.Red) {
				LocalGameStatusText.text = "您的回合开始！";
				RemoteGameStatusText.text = "等待...";
				return;
			}
			if (localPlayerType==ChessPlayerType.Black) {
				LocalGameStatusText.text = "等待对方走棋！";
				RemoteGameStatusText.text = "思考中...";
			}

		} else {
			if (localPlayerType==ChessPlayerType.Red) {
				LocalGameStatusText.text = "等待对方走棋！";
				RemoteGameStatusText.text = "思考中...";
				return;
			}
			if (localPlayerType==ChessPlayerType.Black) {
				LocalGameStatusText.text = "您的回合开始！";
				RemoteGameStatusText.text = "等待...";
			}
		}
    }


    /// <summary>
    /// 行棋
    /// </summary>
    /// <param name="moveId">选中的棋子</param>
    /// <param name="killId">击杀的棋子</param>
    /// <param name="targetPosition">目标位置</param>
	public void MoveStone(int moveId, int killId,float x,float z)
    {
        // 0.保存记录到列表
        SaveStep(moveId, killId, x, z);
		// 1.若移动到的位置上有棋子，将其吃掉  
        KillChessman(killId);
		// 2.将移动棋子的路径显示出来  
		ShowPath(new Vector3(ChessmanManager.chessman[moveId]._x, 1f, ChessmanManager.chessman[moveId]._z), x,z);
		// 3.将棋子移动到目标位置  
		MoveChessman(moveId, x,z);
        
    }

    /// <summary>
    /// 回合结束时调用
    /// </summary>
    public void OnEndTurn()
	{
		ButtonCanvasGroup.interactable = false;	//禁用按钮交互
		IsShowingResults = true;

		switch (result) //根据结果展示不同的图片
		{
		case ResultType.None:
			this.StartTurn();
			break;
		case ResultType.Draw:
			this.WinOrLossImage.sprite = this.SpriteDraw;
			this.Restart();
			break;
		case ResultType.LocalWin:
			this.WinOrLossImage.sprite = this.SpriteWin;
			this.Restart();
			break;
		case ResultType.LocalLoss:
			this.WinOrLossImage.sprite = this.SpriteLose;
			this.Restart();
			break;
		default:

			break;
		}

		this.WinOrLossImage.gameObject.SetActive(true);
	}
		

	/// <summary>
	/// 结束游戏
	/// </summary>
	public void EndGame()
	{
		Debug.Log("EndGame");
		Application.Quit ();
	}

    /// <summary>  
    /// 悔棋，退回一步  
    /// </summary>  
    public void BackOne()
    {
        if (_steps.Count == 0) return;

        step tmpStep = _steps[_steps.Count - 1];
        _steps.RemoveAt(_steps.Count - 1);
        Back(tmpStep);
    }

    #endregion

    
    #region Handling Of Buttons	//处理按钮

	public void SendMassage(string massage){
		this.turnManager.SendMove(massage, false);
		PlayMusic(selectClap);
	}


	/// <summary>
	/// 同意.
	/// </summary>
	/// <param name="t">T.</param>
	public void OnAgree(Text t){
		this.turnManager.SendMove(t.text+"Yes", false);
		PlayMusic(selectClap);
	}

	/// <summary>
	/// 拒绝.
	/// </summary>
	/// <param name="t">T.</param>
	public void OnDisagree(Text t){
		this.turnManager.SendMove(t.text+"No", false);
		PlayMusic(selectClap);
	}

	/// <summary>
	/// 认输.
	/// </summary>
	public void OnDefeat(){
		result = ResultType.LocalLoss;
		PlayMusic(selectClap);
		if (localPlayerType==ChessPlayerType.Black) {
			this.turnManager.SendMove("BlackDefeat", true);
		}
		if (localPlayerType==ChessPlayerType.Red) {
			this.turnManager.SendMove("RedDefeat", true);
		}
	}
		
    public void OnCancelSelected(int targetId)
    {
		this.turnManager.SendMove("+CancelSelected "+targetId.ToString(), false);

		CancelSelected (targetId);
    }
		

    public void OnSelectChessman(int selectId,float x,float z)
    {
        if (_selectedId == -1)
        {
            TrySelectChessman(selectId);
        }
        else
        {
            TryMoveChessman(selectId, x, z);
        }
        
    }

    /// <summary>
    /// 连接
    /// </summary>
    public void OnClickConnect()
	{
		PlayMusic(selectClap);
		PhotonNetwork.ConnectUsingSettings(null);
		PhotonHandler.StopFallbackSendAckThread();  // 这在案例中被用于后台超时!
	}

	/// <summary>
	/// 重新连接并重新加入
	/// </summary>
	public void OnClickReConnectAndRejoin()
	{
		PlayMusic(selectClap);
		PhotonNetwork.ReconnectAndRejoin();
		PhotonHandler.StopFallbackSendAckThread();  // this is used in the demo to timeout in background!
	}

    #endregion

    #region Call PositionManager //调用棋子规则


    /// <summary>  
    /// 判断走棋是否符合走棋的规则  
    /// </summary>  
    /// <param name="selectedId">选中的棋子</param>  
    /// <param name="killId">击杀的棋子</param>
    /// <param name="targetPosition">目标位置</param>
    /// <returns></returns>
	bool CanMove(int moveId, int killId,float x,float z)
    {
        if (killId!=-1) {    //如果是同阵营的棋子，则取消原来的选择，选择新的棋子
			if (SameColor (moveId, killId)) {
				OnCancelSelected (moveId);
				TrySelectChessman (killId);
				return false;
			}
			switch (ChessmanManager.chessman[moveId]._type) {
			case ChessmanManager.Chessman.TYPE.KING:
			case ChessmanManager.Chessman.TYPE.GUARD:
			case ChessmanManager.Chessman.TYPE.ROOK:
			case ChessmanManager.Chessman.TYPE.PAWN:
				return isObstacle (killId);
			case ChessmanManager.Chessman.TYPE.ELEPHANT:
				return CanMoveElephant (killId,moveId,x,z);
			case ChessmanManager.Chessman.TYPE.HORSE:
				return CanMoveHorse (killId,moveId,x,z);
			case ChessmanManager.Chessman.TYPE.CANNON:
				return CanMoveCannon (killId,moveId,x,z);
			}

        }
			
        return true;
    }

    /// <summary>  
    /// 判断点击的棋子是否可以被选中，即点击的棋子是否在它可以移动的回合  
    /// </summary>  
    /// <param name="id">棋子ID</param>  
    /// <returns></returns>  
    bool CanSelect(int id)
    {
        return _isRedTurn == ChessmanManager.chessman[id]._red;
    }

	/// <summary>
	/// Is the obstacle. | 判断目标棋子是否可以击杀，通过障碍来判断，如果该目标是障碍中的一员，则可以击杀
	/// </summary>
	/// <returns><c>true</c>, if obstacle was ised, <c>false</c> otherwise.</returns>
	/// <param name="killId">Kill identifier.</param>
	bool isObstacle(int killId){
		if (chessManManager.DetectedObstacles.Count>0) {
			for (int i = 0; i < chessManManager.DetectedObstacles.Count; i++) {
				if (chessManManager.DetectedObstacles[i].name==killId.ToString()) {
					return true;
				}
			}
		}
		return false;
	}
	 
	/// <summary>
	/// Determines whether this instance can move elephant the specified killId moveId x z. | 在障碍物中寻找符合步长条件的目标击杀
	/// </summary>
	/// <returns><c>true</c> if this instance can move elephant the specified killId moveId x z; otherwise, <c>false</c>.</returns>
	/// <param name="killId">Kill identifier.</param>
	/// <param name="moveId">Move identifier.</param>
	/// <param name="x">The x coordinate.</param>
	/// <param name="z">The z coordinate.</param>
	bool CanMoveElephant(int killId,int moveId,float x,float z){
		if (chessManManager.DetectedObstacles.Count>0) {
			for (int i = 0; i < chessManManager.DetectedObstacles.Count; i++) {
				if (chessManManager.DetectedObstacles[i].name==killId.ToString()) {
					float _x = ChessmanManager.chessman [moveId]._x;
					float _z = ChessmanManager.chessman [moveId]._z;
					if (Mathf.Abs(x-_x)==6f && Mathf.Abs(z-_z)==6f) {
						return true;
					}
				}
			}
		}
		return false;
	}

	/// <summary>
	/// Determines whether this instance can move horse the specified killId moveId x z. | 在障碍物中寻找符合步长条件的目标击杀
	/// </summary>
	/// <returns><c>true</c> if this instance can move horse the specified killId moveId x z; otherwise, <c>false</c>.</returns>
	/// <param name="killId">Kill identifier.</param>
	/// <param name="moveId">Move identifier.</param>
	/// <param name="x">The x coordinate.</param>
	/// <param name="z">The z coordinate.</param>
	bool CanMoveHorse(int killId,int moveId,float x,float z){
		if (chessManManager.DetectedObstacles.Count>0) {
			for (int i = 0; i < chessManManager.DetectedObstacles.Count; i++) {
				if (chessManManager.DetectedObstacles[i].name==killId.ToString()) {
					float _x = ChessmanManager.chessman [moveId]._x;
					float _z = ChessmanManager.chessman [moveId]._z;
					if (Mathf.Abs(x-_x)==6f || Mathf.Abs(z-_z)==6f) {
						return true;
					}
				}
			}
		}
		return false;
	}

	/// <summary>
	/// Determines whether this instance can move cannon the specified killId moveId x z. | 首先判断目标是否在障碍物中，然后判断是否有炮台（即炮打翻山的隔子）
	/// </summary>
	/// <returns><c>true</c> if this instance can move cannon the specified killId moveId x z; otherwise, <c>false</c>.</returns>
	/// <param name="killId">Kill identifier.</param>
	/// <param name="moveId">Move identifier.</param>
	/// <param name="x">The x coordinate.</param>
	/// <param name="z">The z coordinate.</param>
	bool CanMoveCannon(int killId,int moveId,float x,float z){
		
		if (chessManManager.DetectedObstacles.Count>0) {
			int ob = 0;
			bool isOb = false;
			for (int i = 0; i < chessManManager.DetectedObstacles.Count; i++) {
				float ox = chessManManager.DetectedObstacles [i].transform.localPosition.x;
				float oz = chessManManager.DetectedObstacles [i].transform.localPosition.z;

				if (ox==x && oz==z) {
					isOb = true;
					continue;
				}
				float _x = ChessmanManager.chessman [moveId]._x;
				float _z = ChessmanManager.chessman [moveId]._z;
				if (_x == x) {
					if (z > _z) {
						if (oz > _z && oz < z) {
							ob += 1;
						}
					} else {
						if (oz < _z && oz > z) {
							ob += 1;
						}
					}
				} else {
					if (x > _x) {
						if (ox > _x && ox < x) {
							ob += 1;
						}
					} else {
						if (ox < _x && ox > x) {
							ob += 1;
						}
					}
				}

			}
			if (isOb && ob==1) {
				return true;
			}
		}
		return false;
	}
    #endregion

    #region PUN Callbacks   //重新PUN回调函数


    /// <summary>
    /// 当本地用户/客户离开房间时调用。
    /// </summary>
    /// <remarks>当离开一个房间时，PUN将你带回主服务器。
    /// 在您可以使用游戏大厅和创建/加入房间之前，OnJoinedLobby()或OnConnectedToMaster()会再次被调用。</remarks>
    public override void OnLeftRoom()
    {
        Debug.Log("OnLeftRoom()");
        UpdatePlayerScoreTexts();
		PlayMusic (LeaveClip);
        RefreshUIViews();
    }

    /// <summary>
    /// 当进入一个房间（通过创建或加入）时被调用。在所有客户端（包括主客户端）上被调用.
    /// </summary>
    /// <remarks>这种方法通常用于实例化玩家角色。
    /// 如果一场比赛必须“积极地”被开始，你也可以调用一个由用户的按键或定时器触发的PunRPC 。
    /// 
    /// 当这个被调用时，你通常可以通过PhotonNetwork.playerList访问在房间里现有的玩家。
    /// 同时，所有自定义属性Room.customProperties应该已经可用。检查Room.playerCount就知道房间里是否有足够的玩家来开始游戏.</remarks>
    public override void OnJoinedRoom()
    {
        local = PhotonNetwork.player;
		remote = PhotonNetwork.player.GetNext();
		if (PhotonNetwork.isMasterClient && local !=null) {

			localPlayerType = ChessPlayerType.Red;
			LocalGameStatusText.text = "您是红方棋手……";
			this.LocalPlayerNameText.text = local.NickName + ":红方";
			ExitGames.Client.Photon.Hashtable playerType = new ExitGames.Client.Photon.Hashtable ();
			playerType.Add ("playerType", "红方选手");
			local.SetCustomProperties (playerType, null, false);
		} else {
			this.LocalPlayerNameText.text = local.NickName+":黑方";
			ExitGames.Client.Photon.Hashtable playerType = new ExitGames.Client.Photon.Hashtable();
			playerType.Add("playerType", "黑方选手");
			local.SetCustomProperties(playerType, null, false);
			localPlayerType = ChessPlayerType.Black;
			LocalGameStatusText.text = "您是黑方棋手……";
		}

		if (PhotonNetwork.room.PlayerCount == 2 && this.turnManager.Turn == 0)
        {
				LocalGameStatusText.text = "开局！";
                // 当房间内有两个玩家,则开始首回合
                this.StartTurn();
				PlayMusic (welcomMusic);
            
        }

        if (PhotonNetwork.room.PlayerCount > 2)
        {
            if (localPlayerType == ChessPlayerType.Guest)
            {
                LocalGameStatusText.text = "棋局已开始，正在进入观棋模式……";
            }
        }
		if (remote != null)
		{
			RemoteGameStatusText.text = "已匹配！";
			// 应该是这种格式: "name        00"
			if (PhotonNetwork.isMasterClient) {
				this.RemotePlayerNameText.text = remote.NickName + "—黑方";
			} else {
				this.RemotePlayerNameText.text = remote.NickName + "—红方" ;
			}
		}
		else
		{
			this.RemotePlayerNameText.text = "匹配...";
			LocalGameStatusText.text="等待其他玩家...";
			RemoteGameStatusText.text = "正在匹配...";
		}
			
        UpdatePlayerScoreTexts();
		RefreshUIViews();
   	}
		


    /// <summary>
    /// 当未知因素导致连接失败（在建立连接之后）时调用，接着调用OnDisconnectedFromPhoton()。
    /// </summary>
    /// <remarks>如果服务器不能一开始就被连接，就会调用OnFailedToConnectToPhoton。错误的原因会以DisconnectCause的形式提供。</remarks>
    /// <param name="cause">Cause.</param>
    public override void OnConnectionFail(DisconnectCause cause)
    {
        this.DisconnectedPanel.gameObject.SetActive(true);
    }

	/// <summary>
	/// 当一个远程玩家进入房间时调用。这个PhotonPlayer在这个时候已经被添加playerlist玩家列表.
	/// </summary>
	/// <remarks>如果你的游戏开始时就有一定数量的玩家，这个回调在检查Room.playerCount并发现你是否可以开始游戏时会很有用.</remarks>
	/// <param name="newPlayer">New player.</param>
	public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
	{
		//Debug.Log("Other player arrived");
		LocalGameStatusText.text = "欢迎"+newPlayer.NickName+"加入游戏！";
		PlayMusic (JoinClip);
	}

	/// <summary>
	/// 当一个远程玩家离开房间时调用。这个PhotonPlayer 此时已经从playerlist玩家列表删除.
	/// </summary>
	/// <remarks>当你的客户端调用PhotonNetwork.leaveRoom时，PUN将在现有的客户端上调用此方法。当远程客户端关闭连接或被关闭时，这个回调函数会在经过几秒钟的暂停后被执行.</remarks>
	/// <param name="otherPlayer">Other player.</param>
	public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
	{
		PlayMusic (LeaveClip);
		//Debug.Log("Other player disconnected! isInactive: " + otherPlayer.IsInactive);
		LocalGameStatusText.text ="玩家"+ otherPlayer.NickName+"已离开游戏";
        if (!otherPlayer.IsLocal)
        {
            result = ResultType.LocalWin;
            UpdateScores();
        }
        Restart();
	}
    #endregion


	#region Private Methods //私有方法

	/// <summary>
	/// 播放指定音效.
	/// </summary>
	/// <param name="targetAudio">目标音效</param>
	void PlayMusic(AudioClip targetAudio)
	{

		if (targetAudio != null && !source.isPlaying)
		{
			this.source.PlayOneShot(targetAudio);
		}
	}

	/// <summary>
	/// 更新玩家文本信息
	/// </summary>
	private void UpdatePlayerScoreTexts()
	{
        if (remote != null)
		{
			// 应该是这种格式: "00"
            RemoteScoreText.text= remote.GetScore().ToString("D2");
		}

		if (local != null)
		{
            LocalScoreText.text= local.GetScore().ToString("D2");
		}
	}

	void MoveError(int moveId, float x,float z)
	{
		GameObject chessman = chessManManager.transform.FindChild(moveId.ToString()).gameObject;
		Vector3 oldPosition = new Vector3(ChessmanManager.chessman[moveId]._x, 1f, ChessmanManager.chessman[moveId]._z);
		HidePath ();
		ShowPath (oldPosition,x,z);
		LocalGameStatusText.text = "MoveError:"+chessman.name+"不能移动到目标位置:"+x;
	}

	void TrySelectChessman(int selectId)
	{
		if (selectId==-1)
		{
			return;
		}

		if (localPlayerType == ChessPlayerType.Guest) return;   //游客只能观看

		if (selectId>=16)    //黑子无法被红方或红色回合内选定
		{
			if (localPlayerType==ChessPlayerType.Red || _isRedTurn)
			{
				return;
			}
		}
		else    //红子同样无法被其他阵营选定
		{
			if (localPlayerType == ChessPlayerType.Black || !_isRedTurn)
			{
				return;
			}
		}

		if (!CanSelect(selectId)) return;
		this.turnManager.SendMove ("+ConfirmedSelect "+selectId.ToString(),false);
		ConfirmedSelect (selectId);
	}

	void ConfirmedSelect(int selectId){
		_selectedId = selectId;
		Transform chessman=chessManManager.transform.FindChild(selectId.ToString());
		chessman.GetComponent<ChessmanController> ().SelectedChessman ();
		HidePath ();
	}

	/// <summary>  
	/// 设置棋子死亡  
	/// </summary>  
	/// <param name="id"></param>  
	void KillChessman(int id)
	{
		if (id == -1) return;
		if (_isRedTurn && localPlayerType==ChessPlayerType.Black) {	//红方回合被击杀的必然是黑方减分
			CameraRigManager.LocalPlayerInstance.GetComponent<CameraRigManager> ().ApplyDamage ();
		}
		if (!_isRedTurn && localPlayerType==ChessPlayerType.Red) {
			CameraRigManager.LocalPlayerInstance.GetComponent<CameraRigManager> ().ApplyDamage ();
		}
		ChessmanManager.chessman[id]._dead = true;
		Transform chessman=chessManManager.transform.FindChild(id.ToString());
		chessman.GetComponent<ChessmanController> ().SwitchDead ();
	}

	/// <summary>  
	/// 复活棋子  
	/// </summary>  
	/// <param name="id"></param>  
	void ReliveChess(int id)
	{
		if (id == -1) return;

		//因GameObject.Find();函数不能找到active==false的物体，故先找到其父物体，再找到其子物体才可以找到active==false的物体  
		ChessmanManager.chessman[id]._dead = false;
		GameObject Stone = chessManManager.transform.Find(id.ToString()).gameObject;
		Stone.SetActive(true);
	}

	/// <summary>  
	/// 将移动的棋子ID、吃掉的棋子ID以及棋子从A点的坐标移动到B点的坐标都记录下来  
	/// </summary>  
	/// <param name="moveId">选中的棋子ID</param>  
	/// <param name="killId">击杀的棋子ID</param>  
	/// <param name="toX">目标X坐标</param>  
	/// <param name="toZ">目标Z坐标</param>  
	void SaveStep(int moveId, int killId, float toX, float toZ)
	{
		step tmpStep = new step();
		//当前棋子的位置
		float fromX = ChessmanManager.chessman[moveId]._x;
		float fromZ = ChessmanManager.chessman[moveId]._z;

		tmpStep.moveId = moveId;
		tmpStep.killId = killId;
		tmpStep.xFrom = fromX;
		tmpStep.zFrom = fromZ;
		tmpStep.xTo = toX;
		tmpStep.zTo = toZ;

		_steps.Add(tmpStep);

	}

	/// <summary>  
	/// 设置上一步棋子走过的路径，即将上一步行动的棋子的位置留下标识，并标识该棋子  
	/// </summary>  
	void ShowPath(Vector3 oldPosition, float x,float z)
	{
		Vector3 newPosition = new Vector3 (x, 0.57f, z);
		if (!Selected.activeSelf) {
			Selected.SetActive(true);
            Selected.transform.localPosition = newPosition;
        }

		if (!Path.activeSelf) {
			Path.SetActive(true);
            Path.transform.localPosition = oldPosition;
        }

	}


	/// <summary>  
	/// 隐藏路径  
	/// </summary>  
	void HidePath()
	{
		if (Selected.activeSelf)
		{
			Selected.SetActive(false);
			Path.SetActive(false);
		}

	}



	/// <summary>  
	/// 移动棋子到目标位置  
	/// </summary>  
	/// <param name="targetPosition">目标位置</param>  
	void MoveChessman(int moveId,float x,float z)
	{
		Transform chessman = chessManManager.transform.FindChild(moveId.ToString());
		chessman.GetComponent<ChessmanController>().SetTarget(x,z);
		_isRedTurn = !_isRedTurn;
	}

	/// <summary>  
	/// 通过记录的步骤结构体来返回上一步  
	/// </summary>  
	/// <param name="_step"></param>  
	void Back(step _step)
	{
		if (_step.killId != -1) {
			ReliveChess (_step.killId);
		}
		MoveChessman(_step.moveId, _step.xFrom, _step.zFrom);

		HidePath();
		if (_selectedId != -1)
		{      
			_selectedId = -1;
		}
	}

	/// <summary>
	/// 正在移动棋子.
	/// </summary>
	/// <param name="selectedId">选择的棋子ID.</param>
	/// <param name="killId">要击杀的棋子ID.</param>
	/// <param name="toX">目标To x.</param>
	/// <param name="toZ">目标To z.</param>
	void OnMoveChessman(int selectedId,int killId,float toX,float toZ)
	{
		float fromX = ChessmanManager.chessman[selectedId]._x;
		float fromZ = ChessmanManager.chessman[selectedId]._z;
		string tmpStr = selectedId.ToString() +"s"+ killId.ToString()+"s"+fromX.ToString()+"s"+fromZ.ToString()+"s"+toX.ToString()+"s"+toZ.ToString();
		this.turnManager.SendMove(tmpStr, true);	//弃用step结构体来传递信息的原因是Photon不能序列化,所以采用字符串来同步信息
	}

	/// <summary>
	/// 刷新UI视图
	/// </summary>
	void RefreshUIViews()
	{
		GameUiView.gameObject.SetActive(PhotonNetwork.inRoom);
		ButtonCanvasGroup.interactable = PhotonNetwork.room != null ? PhotonNetwork.room.PlayerCount > 1 : false;
	}

	bool IsRed(int id)
	{
		return ChessmanManager.chessman[id]._red;
	}

	bool IsDead(int id)
	{
		if (id == -1) return true;
		return ChessmanManager.chessman[id]._dead;
	}

	bool SameColor(int id1, int id2)
	{
		if (id1 == -1 || id2 == -1) return false;

		return IsRed(id1) == IsRed(id2);
	}

	void PopRequest(string title){
		if (RequestPanel == null && RequestPanel.gameObject.activeSelf) {
			return;
		} else {
			RequestPanel.gameObject.SetActive (true);
			RequestPanel.transform.FindChild ("Title/Text").GetComponent<Text> ().text=title;
		}

	}

	void Restart(){
		if (PhotonNetwork.isMasterClient) {
			PhotonNetwork.LoadLevel ("ChineseChessVR0");
			Debug.Log ("Restarted!");
		}

	}

	void CancelSelected(int cancelId){
		if (_selectedId==cancelId)
		{
			_selectedId = -1;
		}
	}

	#endregion


}
