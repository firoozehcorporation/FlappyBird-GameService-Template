using System;
using FiroozehGameServiceAndroid.Utils;
using UnityEngine;
using UnityEngine.UI;

public enum TextType
{
    Play
    ,Achievement
    ,LeaderBoard
    ,Survey
    ,GameOver
    ,Restart
    ,Des
      
}
    
public class FarsiText : MonoBehaviour
{
    public TextType Type;
    public Text Text;

        
    private void Start()
    {
        Text = GetComponent<Text>();
        switch (Type)
        {
           
         case TextType.Restart:
                Text.text = FarsiTextUtil.FixText("دوباره");
                break;
            case TextType.Play:
                Text.text = FarsiTextUtil.FixText("شروع");
                break;
            case TextType.Achievement:
                Text.text = FarsiTextUtil.FixText("دستاورد ها");
                break;
            case TextType.LeaderBoard:
                Text.text = FarsiTextUtil.FixText("لیست های مقایسه ای");
                break;
            case TextType.Survey:
                Text.text = FarsiTextUtil.FixText("نظر سنجی");
                break;
            case TextType.GameOver:
                Text.text = FarsiTextUtil.FixText("باختی!!");
                break;
            case TextType.Des:
                Text.text = FarsiTextUtil.FixText("پرنده جهنده");
                break;
            default:
                break;
        }
    }
}