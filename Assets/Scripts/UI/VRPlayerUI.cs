// --------------------------------------------------------------------------------------------------------------------
// <copyright file=PlayerUI.cs company=League of HTC Vive Developers>
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
//中文注释：胡良云（CloudHu） 3/24/2017

// --------------------------------------------------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// FileName: PlayerUI.cs
/// Author: 胡良云（CloudHu）
/// Corporation: 
/// Description: 
/// DateTime: 3/24/2017
/// </summary>
[RequireComponent(typeof(Canvas))]
public class VRPlayerUI : MonoBehaviour {
	
	#region Public Variables  //公共变量区域

	[Tooltip("显示在棋子英雄头顶的文本.")]
	public string displayText;
	[Tooltip("显示文本的字体大小.")]
	public int fontSize = 14;
	[Tooltip("字体颜色.")]
	public Color fontColor = Color.black;

	[Tooltip("整个外框的大小.")]
	public Vector2 containerSize = new Vector2(0.1f, 0.03f);
	[Tooltip("字体框背景色.")]
	public Color containerColor = Color.black;
	[Tooltip("生命值")]
	public Slider PlayerHealthSlider;
	[Tooltip("和目标之间的偏移值")]
	public Vector3 ScreenOffset = new Vector3(0f,5f,0f);

	#endregion


	#region Private Variables   //私有变量区域

	[SerializeField]
	private PhotonVoiceRecorder recorder;

	[SerializeField]
	private PhotonVoiceSpeaker speaker;

	[SerializeField]
	private Image recorderSprite;

	[SerializeField]
	private Image speakerSprite;

	[SerializeField]
	private Text bufferLagText;

	private bool showSpeakerLag;

	GameObject _target;	 //目标

	CameraRigManager manager;

	#endregion


	#region MonoBehaviour CallBacks //回调函数区域
	// Use this for initialization
	void Start () {
		_target = transform.parent.gameObject;

		recorder = _target.GetComponent<PhotonVoiceRecorder> ();
		speaker = _target.GetComponent<PhotonVoiceSpeaker> ();
		object playerType;
		manager = _target.GetComponent<CameraRigManager> ();
		manager.photonView.owner.CustomProperties.TryGetValue ("playerType", out playerType);
        if (playerType.ToString()== "红方选手")
        {
            fontColor = Color.red;
        }
		UpdateText (playerType.ToString()+" ： "+manager.photonView.owner.NickName);
		ResetUI();
	}

	// Update is called once per frame
	void Update () {
		if (_target == null) {
			Destroy(this.gameObject);
			return;
		}

		recorderSprite.enabled = recorder != null && recorder.IsTransmitting;
		speakerSprite.enabled = speaker != null && speaker.IsPlaying;
		bufferLagText.enabled = showSpeakerLag && speaker.IsPlaying && speaker.IsVoiceLinked;
		bufferLagText.text = string.Format("{0}", speaker.CurrentBufferLag);
		// Reflect the Player Health
		if (PlayerHealthSlider != null) {
			PlayerHealthSlider.value = manager.Health;
		}
	}
		
	#endregion

	#region Public Methods	//公共方法区域

	public void ResetUI()
	{
		SetContainer();
		SetText("UITextFront");
		SetText("UITextReverse");

		if (PlayerHealthSlider != null) {
			PlayerHealthSlider.value = manager.Health;
		}
	}

	public void UpdateText(string newText)
	{
		displayText = newText;
		ResetUI();
	}
		

	#endregion


	private void SetContainer()
	{
		transform.FindChild("Canvas").GetComponent<RectTransform>().sizeDelta = containerSize;
		var tmpContainer = transform.FindChild("Canvas/UIContainer");
		tmpContainer.GetComponent<RectTransform>().sizeDelta = containerSize;
		tmpContainer.GetComponent<Image>().color = containerColor;
	}

	private void SetText(string name)
	{
		var tmpText = transform.FindChild("Canvas/" + name).GetComponent<Text>();
		tmpText.material = Resources.Load("UIText") as Material;
		tmpText.text = displayText.Replace("\\n", "\n");
		tmpText.color = fontColor;
		tmpText.fontSize = fontSize;
	}
	
}
