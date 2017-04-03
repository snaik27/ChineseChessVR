// --------------------------------------------------------------------------------------------------------------------
// <copyright file=Tips.cs company=League of HTC Vive Developers>
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
//中文注释：胡良云（CloudHu） 4/1/2017
// --------------------------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// FileName: Tips.cs
/// Author: 胡良云（CloudHu）
/// Corporation: 
/// Description: 
/// DateTime: 4/1/2017
/// </summary>
public class Tips : MonoBehaviour {
	
	#region Public Variables  //公共变量区域
	
	[Tooltip("The text that is displayed on the tooltip.")]
	public string displayText;
	[Tooltip("The size of the text that is displayed.")]
	public int fontSize = 14;
	[Tooltip("The size of the tooltip container where `x = width` and `y = height`.")]
	public Vector2 containerSize = new Vector2(0.1f, 0.03f);

	[Tooltip("The colour to use for the text on the tooltip.")]
	public Color fontColor = Color.black;
	[Tooltip("The colour to use for the background container of the tooltip.")]
	public Color containerColor = Color.black;
	[Tooltip("Pixel offset from the player target")]
	public Vector3 ScreenOffset = new Vector3(0f,30f,0f);
	#endregion


	#region Private Variables   //私有变量区域

	#endregion
	
	
	#region MonoBehaviour CallBacks //回调函数区域
	protected virtual void Start()
	{
		ResetTooltip("0");
        ResetTooltip("1");
    }

	protected virtual void Update()
	{
		//保持UI看向本地玩家
		if (CameraRigManager.LocalPlayerInstance != null)
		{
			transform.LookAt(CameraRigManager.LocalPlayerInstance.transform);
		}
	}
	#endregion
	
	#region Public Methods	//公共方法区域
	
	/// <summary>
	/// The ResetTooltip method resets the tooltip back to its initial state.
	/// </summary>
	public void ResetTooltip(string index)
	{
		SetContainer(index);
		SetText("UITextFront"+index);
		SetText("UITextReverse"+index);
	}

	/// <summary>
	/// The UpdateText method allows the tooltip text to be updated at runtime.
	/// </summary>
	/// <param name="newText">A string containing the text to update the tooltip to display.</param>
	public void UpdateText(string newText,string index)
	{
		displayText = newText;
		ResetTooltip(index);
	}
	#endregion
	
	#region Private Methods	//私有方法区域
	
	private void SetContainer(string index)
	{
		transform.FindChild("TooltipCanvas").GetComponent<RectTransform>().sizeDelta = containerSize;
		var tmpContainer = transform.FindChild("TooltipCanvas/UIContainer"+index);
		tmpContainer.GetComponent<RectTransform>().sizeDelta = containerSize;
		tmpContainer.GetComponent<Image>().color = containerColor;
	}

	private void SetText(string name)
	{
		var tmpText = transform.FindChild("TooltipCanvas/" + name).GetComponent<Text>();
		tmpText.material = Resources.Load("UIText") as Material;
		tmpText.text = displayText.Replace("\\n", "\n");
		tmpText.color = fontColor;
		tmpText.fontSize = fontSize;
	}

	#endregion
}